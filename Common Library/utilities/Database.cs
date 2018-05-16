using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;

namespace hp.utilities
{
    public class Database
    {
        private DbConnection Connection = null;
        private int _CommandTimeOut = 1000;

        public delegate void BoolEventDelegate(bool Value);

        public const string StandardDateFormat = "yyyy'-'MM'-'dd";
        public const string StandardDateTimeFormat = "yyyy'-'MM'-'dd HH':'mm':'ss'.'fff";
        public const string StandardDateTimeFormatAccess = "yyyy'-'MM'-'dd HH':'mm':'ss";
        public const string UTCDateTimeFormat = "ddd MMM d HH:mm:ss UTCzzzzz yyyy";

        public bool AutoOpen = true;
        public bool AutoClose = true;
        public string LastSQLString = "";
        public string LastErrorMessage = "";

        #region Constructor, Parameters, Destructor

        // Constructors
        public Database()
        {
        }

        // Cleanup resources
        public void Dispose()
        {
            Close();
        }

        private string mConnectionString = "";

        public string ConnectionString
        {
            get { return mConnectionString; }
            set
            {
                if (mConnectionString != value)
                {
                    bool WasOpen = IsOpen;
                    if (WasOpen)
                        Close();
                    mConnectionString = value;
                    if (WasOpen)
                        Open();
                }
            }
        }

        #endregion

        #region Fundamentals

        // Open Database Connection
        private bool Open(ref DbConnection ThisConnection)
        {
            Close(ref ThisConnection);

            ThisConnection = new SqlConnection();
            ThisConnection.ConnectionString = mConnectionString;
            bool ReturnValue = false;
            try
            {
                ThisConnection.Open();
                ReturnValue = true;
            }
            catch (Exception e)
            {
                LastErrorMessage = e.Message;
                Close();
            }
            return ReturnValue;
        }

        public bool Open()
        {
            return Open(ref Connection);
        }

        public void BeginOpen(BoolEventDelegate Callback)
        {
            Thread OpenThread = new Thread(DoBeginThread);
            OpenThread.Start(new object[] {Callback});
        }

        private void DoBeginThread(object parameters)
        {
            BoolEventDelegate Callback = (BoolEventDelegate) ((object[]) parameters)[0];
            bool ReturnValue = Open();
            if (Callback != null)
                Callback(ReturnValue);
        }

        // Close Database Connection
        private void Close(ref DbConnection ThisConnection)
        {
            try
            {
                if (ThisConnection != null)
                {
                    ThisConnection.Close();
                    ThisConnection.Dispose();
                    ThisConnection = null;
                }
            }
            catch
            {
            }
        }

        public void Close()
        {
            Close(ref Connection);
        }

        // Close All Database Connections (release any connection pools)
        public void CloseAll()
        {
            try
            {
                if (Connection != null)
                {
                    Connection.Close();
                    if (Connection.GetType().Equals(typeof (SqlConnection)))
                        SqlConnection.ClearPool((SqlConnection) Connection);
                    Connection.Dispose();
                    Connection = null;
                }
            }
            catch
            {
            }
        }

        // See if Database Connection is open, or open/close it
        private bool IsConnectionOpen(ref DbConnection ThisConnection)
        {
            return ThisConnection != null && ThisConnection.State != ConnectionState.Closed &&
                   ThisConnection.State != ConnectionState.Broken;
        }

        public bool IsOpen
        {
            get { return IsConnectionOpen(ref Connection); }
            set
            {
                if (value != IsOpen)
                {
                    if (IsOpen)
                        Close();
                    else
                        Open();
                }
            }
        }

        public bool TestConnection()
        {
            Open();
            bool ReturnValue = IsOpen;
            Close();
            return ReturnValue;
        }

        private DbTransaction Transaction = null;

        // Start a transaction (a series of database commands which may be rolled back on failure)
        public bool TransactionBegin()
        {
            if (!IsOpen && AutoOpen && mConnectionString != "")
            {
                Close();
                Open();
            }
            if (IsOpen)
            {
                // In case another transaction has already begun, commit it now
                TransactionCommit();
                // Start a new transaction
                Transaction = Connection.BeginTransaction();
            }

            return Transaction != null;
        }

        // Commit and end the current transaction
        public bool TransactionCommit()
        {
            bool ReturnValue = false;
            if (Transaction != null)
            {
                try
                {
                    Transaction.Commit();
                    ReturnValue = true;
                }
                catch
                {
                }
                finally
                {
                    Transaction.Dispose();
                    Transaction = null;
                }
            }
            return ReturnValue;
        }

        // Transaction aborted (because of failure or otherwise), Rollback the current transaction
        public void TransactionRollback()
        {
            if (Transaction != null)
            {
                try
                {
                    Transaction.Rollback();
                }
                catch
                {
                }
                finally
                {
                    Transaction.Dispose();
                    Transaction = null;
                }
            }
        }

        public class SQLParameterizedQuery
        {
            public string SQL;
            public object[] Parameters = null;

            public SQLParameterizedQuery(string SQL, params object[] Parameters)
            {
                this.SQL = SQL;
                this.Parameters = Parameters;
            }
        }

        // Execute a SQL query with no return value
        // Execute a SQL query with no return value
        public bool Execute(params SQLParameterizedQuery[] Queries)
        {
            bool ReturnValue = false;

            if (!IsOpen && AutoOpen && mConnectionString != "")
            {
                Close();
                Open();
            }

            if (IsOpen)
            {
                DbCommand Command = null;
                try
                {
                    Command = Connection.CreateCommand();
                    Command.CommandTimeout = _CommandTimeOut;
                    if (Transaction != null)
                        Command.Transaction = Transaction;
                    for (int i = 0; i < Queries.Length; i++)
                    {
                        Command.CommandText = Queries[i].SQL;
                        if (Queries[i].Parameters != null && Queries[i].Parameters.Length > 0)
                        {
                            List<string> FieldNames = new List<string>();
                            string SQL = Queries[i].SQL;
                            while (SQL.Contains("@"))
                            {
                                string ThisField = SQL.Substring(SQL.IndexOf('@'));
                                int EndIndex = ThisField.IndexOfAny(new char[] {' ', ',', ')', '\'', '"'});
                                if (EndIndex < 0)
                                    SQL = "";
                                else
                                {
                                    SQL = ThisField.Substring(EndIndex);
                                    ThisField = ThisField.Substring(0, EndIndex);
                                }
                                FieldNames.Add(ThisField);
                            }
                            if (FieldNames.Count != Queries[i].Parameters.Length)
                                throw new Exception("Supplied parameter count (" +
                                                    Queries[i].Parameters.Length.ToString() +
                                                    ") does not equal require parameter count (" +
                                                    FieldNames.Count.ToString() + ")!");

                            for (int j = 0; j < Queries[i].Parameters.Length; j++)
                            {
                                string ThisField = FieldNames[j];
                                int Suffix = 0;
                                while (Command.Parameters.IndexOf(ThisField) >= 0)
                                {
                                    Suffix++;
                                    ThisField = FieldNames[j] + Suffix.ToString();
                                }
                                object ThisParameter = Queries[i].Parameters[j];
                                if (ThisParameter == null || ThisParameter.GetType().Equals(typeof (DBNull)))
                                {
                                    if (ThisField.ToUpper() == "@IMAGE")
                                    {
                                        SqlParameter p = new SqlParameter(ThisField, SqlDbType.Image);
                                        p.Value = ThisParameter;
                                        Command.Parameters.Add(p);
                                    }
                                    else
                                        Command.Parameters.Add(new SqlParameter(ThisField, DBNull.Value));
                                }
                                else
                                {
                                    SqlDbType ThisType = SqlDbType.NVarChar;
                                    switch (ThisParameter.GetType().Name)
                                    {
                                        case "Boolean":
                                            ThisType = SqlDbType.Bit;
                                            break;

                                        case "Byte":
                                            ThisType = SqlDbType.TinyInt;
                                            break;

                                        case "Int16":
                                            ThisType = SqlDbType.SmallInt;
                                            break;

                                        case "Int32":
                                            ThisType = SqlDbType.Int;
                                            break;

                                        case "Single":
                                            ThisType = SqlDbType.Real;
                                            break;

                                        case "Double":
                                            ThisType = SqlDbType.Float;
                                            break;

                                        case "TimeSpan":
                                            ThisType = SqlDbType.Time;
                                            break;

                                        case "DateTime":
                                            ThisType = SqlDbType.DateTime;
                                            break;

                                        case "Guid":
                                            ThisType = SqlDbType.UniqueIdentifier;
                                            break;

                                        case "Byte[]":
                                            ThisType = SqlDbType.Image;
                                            break;

                                        case "String":
                                            if (ThisParameter.ToString().Length > 4000)
                                                ThisType = SqlDbType.NText;
                                            break;
                                    }
                                    SqlParameter p = new SqlParameter(ThisField, ThisType);
                                    p.Value = ThisParameter;
                                    Command.Parameters.Add(p);
                                }
                            }

                        }
                        LastSQLString = Queries[i].SQL;
                        Command.ExecuteNonQuery();
                    }
                    ReturnValue = true;
                }
                catch (Exception e)
                {
                    LastErrorMessage = e.Message + "<br>SQL: " + LastSQLString;
                }
                finally
                {
                    if (Command != null)
                        Command.Dispose();
                }
                if (AutoClose)
                    Close();
            }
            return ReturnValue;
        }

        public bool Execute(string SQL, params object[] Parameters)
        {
            return Execute(new SQLParameterizedQuery(SQL, Parameters));
        }

        public bool Execute(string[] SQLLines)
        {
            List<SQLParameterizedQuery> Queries = new List<SQLParameterizedQuery>();
            foreach (string SQL in SQLLines)
            {
                string ThisQuerySQL = SQL.Trim('\r', '\n');
                if (!string.IsNullOrEmpty(ThisQuerySQL))
                    Queries.Add(new SQLParameterizedQuery(ThisQuerySQL));
            }
            return Execute(Queries.ToArray());
        }

        private string GetValueSQLString(object Value)
        {
            if (Value == null || Value.GetType().Equals(typeof (DBNull)))
                return "NULL";
            if (Value.GetType().Equals(typeof (DateTime)))
                return "'" + ((DateTime) Value).ToString(StandardDateTimeFormat) + "'";
            if (Value.GetType().Equals(typeof (TimeSpan)))
                return "'" + ((TimeSpan) Value).ToString() + "'";
            if (Value.GetType().Equals(typeof (string)))
                return "'" + ((string) Value).Replace("'", "''") + "'";
            if (Value.GetType().Equals(typeof (bool)))
                return (bool) Value ? "1" : "0";
            return Value.ToString();
        }

        private string GetValueSQLString(object Value, CultureInfo Culture)
        {
            if (Value == null || Value.GetType().Equals(typeof (DBNull)))
                return "NULL";
            if (Value.GetType().Equals(typeof (DateTime)))
                return "'" + ((DateTime) Value).ToString(StandardDateTimeFormat, Culture.DateTimeFormat) + "'";
            if (Value.GetType().Equals(typeof (TimeSpan)))
                return "'" + ((TimeSpan) Value).ToString() + "'";
            if (Value.GetType().Equals(typeof (string)))
                return "'" + ((string) Value).Replace("'", "''") + "'";
            if (Value.GetType().Equals(typeof (bool)))
                return (bool) Value ? "1" : "0";
            if (Value.GetType().Equals(typeof (byte)))
                return ((byte) Value).ToString(Culture.NumberFormat);
            if (Value.GetType().Equals(typeof (short)))
                return ((short) Value).ToString(Culture.NumberFormat);
            if (Value.GetType().Equals(typeof (ushort)))
                return ((ushort) Value).ToString(Culture.NumberFormat);
            if (Value.GetType().Equals(typeof (int)))
                return ((int) Value).ToString(Culture.NumberFormat);
            if (Value.GetType().Equals(typeof (uint)))
                return ((uint) Value).ToString(Culture.NumberFormat);
            if (Value.GetType().Equals(typeof (float)))
                return ((float) Value).ToString(Culture.NumberFormat);
            if (Value.GetType().Equals(typeof (double)))
                return ((double) Value).ToString(Culture.NumberFormat);
            if (Value.GetType().Equals(typeof (decimal)))
                return ((decimal) Value).ToString(Culture.NumberFormat);
            return Value.ToString();
        }

        // Add an entry to a table
        public bool Insert(string TableName, string[] FieldNames, object[] Values)
        {
            string FieldsString = "";
            string ValuesString = "";
            foreach (string FieldName in FieldNames)
            {
                if (FieldsString != "")
                    FieldsString += ", ";
                FieldsString += "[" + FieldName + "]";

                if (ValuesString != "")
                    ValuesString += ", ";

                ValuesString += "@" + FieldName;
            }
            return Execute("INSERT INTO [" + TableName + "] (" + FieldsString + ") VALUES (" + ValuesString + ")",
                           Values);
        }

        // Add an entry to a table and return the ID of the new row (only works for tables with an 'identity' ID column)
        public int InsertAndGetID(string TableName, string[] FieldNames, object[] Values)
        {
            if (Insert(TableName, FieldNames, Values))
                return GetInteger("SELECT MAX([ID]) FROM " + TableName);
            return int.MinValue;
        }

        // Delete an entry from a table
        public bool Delete(string TableName, string[] FieldNames, object[] Values)
        {
            string SQLString = "DELETE FROM " + TableName;
            if (FieldNames != null && Values != null)
            {
                string DataString = "";
                for (int i = 0; i < FieldNames.Length; i++)
                {
                    if (DataString != "")
                        DataString += " AND ";
                    DataString += "[" + FieldNames[i].Replace(".", "].[") + "]";
                    if (Values[i] == null || Values[i].GetType().Equals(typeof (DBNull)))
                        DataString += " IS NULL";

                    DataString += " = @" + FieldNames[i].Replace('.', '_');
                }
                SQLString += " WHERE " + DataString;
            }

            return Execute(SQLString, Values);
        }

        // Update an entry
        public bool Update(string TableName, string[] SetFieldNames, object[] SetValues, string[] WhereFieldNames,
                           object[] WhereValues)
        {
            string SQLString = "UPDATE [" + TableName + "] SET ";
            string DataString = "";
            for (int i = 0; i < SetFieldNames.Length; i++)
            {
                if (DataString != "")
                    DataString += ", ";
                DataString += "[" + SetFieldNames[i] + "] = ";

                DataString += " @" + SetFieldNames[i];
            }
            SQLString += DataString;
            DataString = "";
            for (int i = 0; i < WhereFieldNames.Length; i++)
            {
                if (DataString != "")
                    DataString += " AND ";
                DataString += "[" + WhereFieldNames[i] + "]";

                DataString += " = @" + WhereFieldNames[i];
            }
            SQLString += " WHERE " + DataString;

            List<object> Values = new List<object>(SetValues);
            Values.AddRange(WhereValues);
            return Execute(SQLString, Values.ToArray());
        }

        // Execute a SQL query and return data in a data table (if applicable)
        public DataTable GetDataTable(string SQLString)
        {
            DataTable ReturnValue = null;

            if (!IsOpen && AutoOpen && mConnectionString != "")
            {
                Close();
                Open();
            }

            if (IsOpen)
            {
                DbCommand Command = null;
                DbDataAdapter DataAdapter = null;

                try
                {
                    Command = Connection.CreateCommand();
                    Command.CommandTimeout = _CommandTimeOut;
                    if (SQLString.ToUpper().IndexOf("CREATE") == 0 ||
                        SQLString.ToUpper().IndexOf("INSERT") == 0 ||
                        SQLString.ToUpper().IndexOf("UPDATE") == 0 ||
                        SQLString.ToUpper().IndexOf("DELETE") == 0)
                    {
                        // These SQL queries have no return value
                        Command.CommandText = SQLString;
                        LastSQLString = SQLString;
                        Command.ExecuteNonQuery();
                    }
                    else
                    {
                        Command.CommandText = SQLString;
                        LastSQLString = SQLString;
                        if (Connection.GetType().Equals(typeof (SqlConnection)))
                            DataAdapter = new SqlDataAdapter();
                        else
                            DataAdapter = new OdbcDataAdapter();
                        DataAdapter.SelectCommand = Command;
                        if (Transaction != null)
                            DataAdapter.SelectCommand.Transaction = Transaction;
                        ReturnValue = new DataTable();
                        DataAdapter.Fill(ReturnValue);
                    }
                }
                catch (Exception e)
                {
                    LastErrorMessage = e.Message + "<br>SQL: " + SQLString;
                    ReturnValue = null;
                }
                finally
                {
                    if (Command != null)
                        Command.Dispose();
                    if (DataAdapter != null)
                        DataAdapter.Dispose();
                }
                if (AutoClose)
                    Close();
            }
            return ReturnValue;
        }

        // Search for and return specific data in a table
        public DataTable GetDataTable(string TableName, string[] ReturnFieldNames, string[] SearchFieldNames,
                                      object[] Values)
        {
            string SQLString = "SELECT ";
            string DataString = "";

            if (ReturnFieldNames.Length == 0)
            {
                DataString = "*";
            }
            else
            {
                foreach (string ReturnFieldName in ReturnFieldNames)
                {
                    if (DataString != "")
                        DataString += ", ";
                    if (ReturnFieldName.Contains("."))
                        DataString += ReturnFieldName;
                    else
                        DataString += "[" + ReturnFieldName + "]";
                }
            }
            SQLString += DataString + " FROM " + TableName;

            if (SearchFieldNames.Length > 0)
                SQLString += " WHERE ";

            DataString = "";
            for (int i = 0; i < SearchFieldNames.Length; i++)
            {
                if (DataString != "")
                    DataString += " AND ";
                if (SearchFieldNames[i].Contains("."))
                    DataString += SearchFieldNames[i];
                else
                    DataString += "[" + SearchFieldNames[i] + "]";
                if (Values[i] == null)
                    DataString += " IS NULL";
                else
                {
                    bool FoundBefore = false;
                    bool FoundAfter = false;
                    if (Values[i] is int || Values[i] is float || Values[i] is DateTime)
                    {
                        for (int j = 0; j < i; j++)
                        {
                            if (SearchFieldNames[j] == SearchFieldNames[i])
                            {
                                FoundBefore = true;
                                break;
                            }
                        }
                        for (int j = i + 1; j < SearchFieldNames.Length; j++)
                        {
                            if (SearchFieldNames[j] == SearchFieldNames[i])
                            {
                                FoundAfter = true;
                                break;
                            }
                        }
                    }
                    if (FoundBefore)
                        DataString += " < ";
                    else if (FoundAfter)
                        DataString += " >= ";
                    else
                        DataString += " = ";
                    DataString += GetValueSQLString(Values[i]);
                }
            }
            return GetDataTable(SQLString + DataString);
        }

        // Execute a SQL query and return data in a data reader (if applicable)
        public DbDataReader GetDataReader(ref DbConnection ThisConnection, string SQLString)
        {
            DbDataReader ReturnValue = null;

            // Note DataReader must use its own DbConnection since no other transactions can happen on
            // the connection until the DataReader is closed.
            if (!IsConnectionOpen(ref ThisConnection) && mConnectionString != "")
                Open(ref ThisConnection);

            if (IsConnectionOpen(ref ThisConnection))
            {
                DbCommand Command = null;

                try
                {
                    Command = ThisConnection.CreateCommand();
                    Command.CommandTimeout = _CommandTimeOut;
                    if (SQLString.ToUpper().IndexOf("CREATE") == 0 ||
                        SQLString.ToUpper().IndexOf("INSERT") == 0 ||
                        SQLString.ToUpper().IndexOf("UPDATE") == 0 ||
                        SQLString.ToUpper().IndexOf("DELETE") == 0)
                    {
                        // These SQL queries have no return value
                        Command.CommandText = SQLString;
                        LastSQLString = SQLString;
                        Command.ExecuteNonQuery();
                    }
                    else
                    {
                        Command.CommandText = SQLString;
                        LastSQLString = SQLString;
                        ReturnValue = Command.ExecuteReader();
                    }
                }
                catch (Exception e)
                {
                    LastErrorMessage = e.Message + "<br>SQL: " + SQLString;
                    ReturnValue = null;
                }
                finally
                {
                    if (Command != null)
                        Command.Dispose();
                }
            }
            return ReturnValue;
        }

        public bool ClearTable(string TableName)
        {
            if (Execute("DELETE FROM [" + TableName + "]"))
                return ResetIdentity(TableName);
            return false;
        }

        public bool ClearAllTables()
        {
            string[] TableNames = GetTableList();
            List<string> ClearedTables = new List<string>(TableNames);
            while (ClearedTables.Count > 0)
            {
                int NumTables = ClearedTables.Count;
                for (int i = 0; i < ClearedTables.Count; i++)
                {
                    if (Execute("DELETE FROM [" + ClearedTables[i] + "]"))
                    {
                        ResetIdentity(ClearedTables[i]);
                        ClearedTables.RemoveAt(i);
                        i--;
                    }
                }
                // If we didn't manage to clear any tables, give up now
                if (ClearedTables.Count == NumTables)
                    break;
            }
            bool ReturnValue = ClearedTables.Count == 0;
            ClearedTables.Clear();
            return ReturnValue;
        }

        public bool DeleteTable(string TableName)
        {
            if (Execute("DROP TABLE [" + TableName + "]"))
                return true;
            return false;
        }

        public bool DeleteAllTables()
        {
            string[] TableNames = GetTableList();
            List<string> DeleteTables = new List<string>(TableNames);
            while (DeleteTables.Count > 0)
            {
                int NumTables = DeleteTables.Count;
                for (int i = 0; i < DeleteTables.Count; i++)
                {
                    if (DeleteTable(DeleteTables[i]))
                    {
                        DeleteTables.RemoveAt(i);
                        i--;
                    }
                }
                // If we didn't manage to delete any tables, give up now
                if (DeleteTables.Count == NumTables)
                    break;
            }
            bool ReturnValue = DeleteTables.Count == 0;
            DeleteTables.Clear();
            return ReturnValue;
        }

        public bool CreateTable(string TableName, string[] ColumnNames, Type[] ColumnTypes)
        {
            string SQLString = "CREATE TABLE [" + TableName + "] (";
            string ColumnsSQLString = "";
            for (int i = 0; i < ColumnNames.Length; i++)
            {
                if (ColumnsSQLString != "")
                    ColumnsSQLString += ", ";
                ColumnsSQLString += "[" + ColumnNames[i] + "] ";
                if (ColumnTypes.Equals(typeof (int)))
                    ColumnsSQLString += "INT";
                // TODO
            }
            return false;
        }

        public bool CreateTable(string TableName, string[] ColumnNames, string[] ColumnTypes)
        {
            string SQLString = "CREATE TABLE [" + TableName + "] (";
            string ColumnsSQLString = "";
            for (int i = 0; i < ColumnNames.Length; i++)
            {
                if (ColumnsSQLString != "")
                    ColumnsSQLString += ", ";
                ColumnsSQLString += "[" + ColumnNames[i] + "] ";

                ColumnsSQLString += ColumnTypes[i];
            }
            ColumnsSQLString += ")";

            return Execute(SQLString + ColumnsSQLString);
        }

        public bool ResetIdentity(string TableName, int Seed)
        {
            return Execute("DBCC CHECKIDENT('" + TableName + "', RESEED, " + (Seed - 1).ToString() + ")");
        }

        public bool ResetIdentity(string TableName)
        {
            return ResetIdentity(TableName, 1);
        }

        public bool DoBulkCopy(string TableName, DataTable SourceTable)
        {
            SqlBulkCopy BulkCopy = null;

            try
            {
                if (Transaction != null)
                {
                    SqlConnection ThisConnection = new SqlConnection();
                    ThisConnection.ConnectionString = ConnectionString;

                    BulkCopy = new SqlBulkCopy(ThisConnection, SqlBulkCopyOptions.Default, (SqlTransaction) Transaction);
                }
                else
                    BulkCopy = new SqlBulkCopy(ConnectionString,
                                               SqlBulkCopyOptions.UseInternalTransaction | SqlBulkCopyOptions.TableLock);

                BulkCopy.BulkCopyTimeout = 50000;
                BulkCopy.DestinationTableName = TableName;
                BulkCopy.WriteToServer(SourceTable);

                return true;
            }
            catch (Exception e)
            {
                LastErrorMessage = e.Message;
                return false;
            }
            finally
            {
                try
                {
                    BulkCopy.Close();
                }
                catch
                {
                }
            }
        }

        #endregion

        #region Scalar Queries

        // Query a single value
        public object GetValue(string SQLString, params object[] Parameters)
        {
            object ReturnValue = null;

            if (!IsOpen && AutoOpen && mConnectionString != "")
            {
                Close();
                Open();
            }

            if (IsOpen)
            {
                DbCommand Command = null;
                try
                {
                    Command = Connection.CreateCommand();
                    Command.CommandTimeout = _CommandTimeOut;
                    Command.CommandText = SQLString;
                    if (Transaction != null)
                        Command.Transaction = Transaction;
                    Command.CommandText = SQLString;
                    if (Parameters != null && Parameters.Length > 0)
                    {
                        List<string> FieldNames = new List<string>();
                        string SQL = SQLString;
                        while (SQL.Contains("@"))
                        {
                            string ThisField = SQL.Substring(SQL.IndexOf('@'));
                            int EndIndex = ThisField.IndexOfAny(new char[] {' ', ',', ')', '\'', '"'});
                            if (EndIndex < 0)
                                SQL = "";
                            else
                            {
                                SQL = ThisField.Substring(EndIndex);
                                ThisField = ThisField.Substring(0, EndIndex);
                            }
                            FieldNames.Add(ThisField);
                        }
                        if (FieldNames.Count != Parameters.Length)
                            throw new Exception("Supplied parameter count (" + Parameters.Length.ToString() +
                                                ") does not equal require parameter count (" +
                                                FieldNames.Count.ToString() +
                                                ")!");

                        for (int j = 0; j < Parameters.Length; j++)
                        {
                            string ThisField = FieldNames[j];
                            object ThisParameter = Parameters[j];
                            if (ThisParameter == null)
                            {
                                Command.Parameters.Add(new SqlParameter(ThisField, DBNull.Value));
                            }
                            else
                            {
                                SqlDbType ThisType = SqlDbType.NVarChar;
                                switch (ThisParameter.GetType().Name)
                                {
                                    case "Boolean":
                                        ThisType = SqlDbType.Bit;
                                        break;

                                    case "Byte":
                                        ThisType = SqlDbType.TinyInt;
                                        break;

                                    case "Int16":
                                        ThisType = SqlDbType.SmallInt;
                                        break;

                                    case "Int32":
                                        ThisType = SqlDbType.Int;
                                        break;

                                    case "Single":
                                        ThisType = SqlDbType.Real;
                                        break;

                                    case "Double":
                                        ThisType = SqlDbType.Float;
                                        break;

                                    case "TimeSpan":
                                        ThisType = SqlDbType.Time;
                                        break;

                                    case "DateTime":
                                        ThisType = SqlDbType.DateTime;
                                        break;

                                    case "String":
                                        if (ThisParameter.ToString().Length > 4000)
                                            ThisType = SqlDbType.NText;
                                        break;
                                }

                                SqlParameter p = new SqlParameter(ThisField, ThisType);
                                p.Value = ThisParameter;
                                Command.Parameters.Add(p);
                            }
                        }
                    }
                    LastSQLString = SQLString;
                    ReturnValue = Command.ExecuteScalar();
                    if (ReturnValue != null && ReturnValue.GetType().Equals(typeof (DBNull)))
                        ReturnValue = null;
                }
                catch (Exception e)
                {
                    LastErrorMessage = e.Message + "<br>SQL: " + SQLString;
                }
                finally
                {
                    if (Command != null)
                        Command.Dispose();
                }
                if (AutoClose)
                    Close();
            }
            return ReturnValue;
        }

        public object GetValue(string TableName, string ReturnField, string[] FieldNames, object[] Values)
        {
            string SQLString = "SELECT [" + ReturnField.Replace(".", "].[") + "] FROM " + TableName + " WHERE ";
            string DataString = "";
            for (int i = 0; i < FieldNames.Length; i++)
            {
                if (DataString != "")
                    DataString += " AND ";
                DataString += "[" + FieldNames[i].Replace(".", "].[") + "] = ";
                DataString += "@" + FieldNames[i].Replace('.', '_');
            }
            return GetValue(SQLString + DataString, Values);
        }

        // Query a single Boolean
        public bool GetBoolean(string SQLString)
        {
            object ReturnValue = GetValue(SQLString);
            if (ReturnValue != null &&
                ReturnValue.GetType().Equals(typeof (bool)))
                return (bool) ReturnValue;
            return false;
        }

        // Query a single Integer
        public int GetInteger(string SQLString)
        {
            object ReturnValue = GetValue(SQLString);
            if (ReturnValue != null &&
                (ReturnValue.GetType().Equals(typeof (char)) ||
                 ReturnValue.GetType().Equals(typeof (byte)) ||
                 ReturnValue.GetType().Equals(typeof (short)) ||
                 ReturnValue.GetType().Equals(typeof (ushort)) ||
                 ReturnValue.GetType().Equals(typeof (int)) ||
                 ReturnValue.GetType().Equals(typeof (uint)) ||
                 ReturnValue.GetType().Equals(typeof (Int64)) ||
                 ReturnValue.GetType().Equals(typeof (UInt64))))
                return Convert.ToInt32(ReturnValue);
            return int.MinValue;
        }

        // Query a single Float
        public float GetFloat(string SQLString)
        {
            object ReturnValue = GetValue(SQLString);
            if (ReturnValue != null &&
                (ReturnValue.GetType().Equals(typeof (float)) ||
                 ReturnValue.GetType().Equals(typeof (decimal))))
                return Convert.ToSingle(ReturnValue);
            return float.NaN;
        }

        // Query a single String
        public string GetString(string SQLString)
        {
            object ReturnValue = GetValue(SQLString);
            if (ReturnValue != null &&
                ReturnValue.GetType().Equals(typeof (string)))
                return ReturnValue.ToString();
            return null;
        }

        public string GetString(string TableName, string ReturnField, string[] FieldNames, object[] Values)
        {
            string SQLString = "SELECT [" + ReturnField.Replace(".", "].[") + "] FROM " + TableName + " WHERE ";
            string DataString = "";
            for (int i = 0; i < FieldNames.Length; i++)
            {
                if (DataString != "")
                    DataString += " AND ";
                DataString += "[" + FieldNames[i].Replace(".", "].[") + "]";
                if (Values[i] == null)
                    DataString += " IS NULL";
                else
                    DataString += " = " + GetValueSQLString(Values[i]);
            }
            object ReturnValue = GetValue(SQLString + DataString);
            if (ReturnValue != null &&
                ReturnValue.GetType().Equals(typeof (string)))
                return ReturnValue.ToString();
            return null;
        }

        // Query a single DateTime
        public DateTime GetDateTime(string SQLString)
        {
            object ReturnValue = GetValue(SQLString);
            if (ReturnValue != null &&
                ReturnValue.GetType().Equals(typeof (DateTime)))
                return (DateTime) ReturnValue;
            return DateTime.MinValue;
        }

        #endregion

        #region Stored Procedures

        public bool ExecuteProcedure(string ProcedureName, string[] Parameters, object[] Values)
        {
            bool ReturnValue = false;

            if (!IsOpen && AutoOpen && mConnectionString != "")
            {
                Close();
                Open();
            }

            if (IsOpen)
            {
                DbCommand Command = null;
                try
                {
                    Command = Connection.CreateCommand();
                    Command.CommandTimeout = _CommandTimeOut;
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = ProcedureName;

                    for (int i = 0; i < Parameters.Length; i++)
                    {
                        SqlParameter ParameterItem = new SqlParameter();
                        ParameterItem.ParameterName = Parameters[i];
                        ParameterItem.Value = Values[i];
                        ParameterItem.SqlDbType = GetParameterType(Values[i]);
                        Command.Parameters.Add(ParameterItem);
                    }
                    Command.ExecuteNonQuery();
                    ReturnValue = true;
                }
                catch (Exception e)
                {
                    LastErrorMessage = e.Message + "<br>SQL: " + LastSQLString;
                }
                finally
                {
                    if (Command != null)
                        Command.Dispose();
                }
                if (AutoClose)
                    Close();
            }
            return ReturnValue;
        }

        public DataTable GetDataTable(string ProcedureName, string[] Parameters, object[] Values)
        {
            DataTable ReturnValue = null;

            if (!IsOpen && AutoOpen && mConnectionString != "")
            {
                Close();
                Open();
            }

            if (IsOpen)
            {
                DbCommand Command = null;
                try
                {
                    Command = Connection.CreateCommand();
                    Command.CommandTimeout = _CommandTimeOut;
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = ProcedureName;

                    for (int i = 0; i < Parameters.Length; i++)
                    {
                        SqlParameter ParameterItem = new SqlParameter();
                        ParameterItem.ParameterName = Parameters[i];
                        ParameterItem.Value = Values[i];
                        ParameterItem.SqlDbType = GetParameterType(Values[i]);
                        Command.Parameters.Add(ParameterItem);
                    }
                    DbDataAdapter DataAdapter = new SqlDataAdapter();
                    DataAdapter.SelectCommand = Command;
                    ReturnValue = new DataTable();
                    DataAdapter.Fill(ReturnValue);
                }
                catch (Exception e)
                {
                    LastErrorMessage = e.Message + "<br>SQL: " + LastSQLString;
                }
                finally
                {
                    if (Command != null)
                        Command.Dispose();
                }
                if (AutoClose)
                    Close();
            }
            return ReturnValue;
        }

        public DataSet GetDataSet(string ProcedureName, string[] Parameters, object[] Values)
        {
            DataSet ReturnValue = null;

            if (!IsOpen && AutoOpen && mConnectionString != "")
            {
                Close();
                Open();
            }

            if (IsOpen)
            {
                DbCommand Command = null;
                try
                {
                    Command = Connection.CreateCommand();
                    Command.CommandTimeout = _CommandTimeOut;
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = ProcedureName;

                    for (int i = 0; i < Parameters.Length; i++)
                    {
                        SqlParameter ParameterItem = new SqlParameter();
                        ParameterItem.ParameterName = Parameters[i];
                        ParameterItem.Value = Values[i];
                        ParameterItem.SqlDbType = GetParameterType(Values[i]);
                        Command.Parameters.Add(ParameterItem);
                    }
                    DbDataAdapter DataAdapter = new SqlDataAdapter();
                    DataAdapter.SelectCommand = Command;
                    ReturnValue = new DataSet();
                    DataAdapter.Fill(ReturnValue);
                }
                catch (Exception e)
                {
                    LastErrorMessage = e.Message + "<br>SQL: " + LastSQLString;
                }
                finally
                {
                    if (Command != null)
                        Command.Dispose();
                }
                if (AutoClose)
                    Close();
            }
            return ReturnValue;

        }

        public object GetValue(string ProcedureName, string[] Parameters, object[] Values)
        {
            object ReturnValue = null;

            if (!IsOpen && AutoOpen && mConnectionString != "")
            {
                Close();
                Open();
            }

            if (IsOpen)
            {
                DbCommand Command = null;
                try
                {
                    Command = Connection.CreateCommand();
                    Command.CommandTimeout = _CommandTimeOut;
                    Command.CommandType = CommandType.StoredProcedure;
                    Command.CommandText = ProcedureName;

                    for (int i = 0; i < Parameters.Length; i++)
                    {
                        SqlParameter ParameterItem = new SqlParameter();
                        ParameterItem.ParameterName = Parameters[i];
                        ParameterItem.Value = Values[i];
                        ParameterItem.SqlDbType = GetParameterType(Values[i]);
                        Command.Parameters.Add(ParameterItem);
                    }

                    LastSQLString = ProcedureName;
                    ReturnValue = Command.ExecuteScalar();

                    if (ReturnValue != null && ReturnValue.GetType().Equals(typeof (DBNull)))
                        ReturnValue = null;
                }
                catch (Exception e)
                {
                    LastErrorMessage = e.Message + "<br>SQL: " + LastSQLString;
                }
                finally
                {
                    if (Command != null)
                        Command.Dispose();
                }
                if (AutoClose)
                    Close();
            }
            return ReturnValue;
        }

        private SqlDbType GetParameterType(object ParameterValue)
        {
            string TypeString = ParameterValue.GetType().ToString();

            switch (TypeString)
            {
                case "System.Int16":
                    return SqlDbType.SmallInt;
                case "System.Int32":
                    return SqlDbType.Int;
                case "System.Int64":
                    return SqlDbType.BigInt;
                case "System.Boolean":
                    return SqlDbType.Bit;
                case "System.DateTime":
                    return SqlDbType.DateTime;
                case "System.Double":
                    return SqlDbType.Float;
                case "System.Decimal":
                    return SqlDbType.Decimal;
                default:
                    return SqlDbType.NVarChar;
            }
        }

        #endregion

        #region Helper Functions

        public static Type GetTypeFromString(string DataType)
        {
            switch (DataType.ToUpper())
            {
                case "INTEGER":
                    return typeof (int);

                case "NUMBER":
                    return typeof (double);

                case "BOOLEAN":
                    return typeof (bool);

                case "DATE":
                case "DATETIME":
                    return typeof (DateTime);

                case "TIME":
                    return typeof (TimeSpan);

                case "STRING":
                    return typeof (string);
            }
            return null;
        }

        public static string GetTypeString(Type DataType)
        {
            if (DataType == null)
                return null;

            switch (DataType.Name)
            {
                case "Byte":
                case "SByte":
                case "Int16":
                case "Int32":
                    return "Integer";

                case "Single":
                case "Double":
                    return "Number";

                case "TimeSpan":
                    return "Time";
            }
            return DataType.Name;
        }

        public int GetDataTypeID(string DataType)
        {
            return GetID("DataTypes", new string[] {"Type"}, new object[] {DataType});
        }

        public int GetDataTypeID(Type DataType)
        {
            return GetDataTypeID(GetTypeString(DataType));
        }

        public string GetServerName()
        {
            // This only works on SQL Server (not CE)
            return GetString("SELECT SERVERPROPERTY('SERVERNAME')");
        }

        public int GetID(string TableName, string[] FieldNames, object[] Values, string[] InsertFieldNames,
                         object[] InsertValues)
        {
            string SQLString = "SELECT [ID] FROM " + TableName + " WHERE ";
            string DataString = "";
            for (int i = 0; i < FieldNames.Length; i++)
            {
                if (DataString != "")
                    DataString += " AND ";
                DataString += "[" + FieldNames[i] + "]";
                if (Values[i] == null)
                    DataString += " IS NULL";
                else
                    DataString += " = " + GetValueSQLString(Values[i]);
            }
            int ReturnValue = GetInteger(SQLString + DataString);
            if (ReturnValue == int.MinValue)
            {
                List<string> NewFieldNames = new List<string>(FieldNames);
                NewFieldNames.AddRange(InsertFieldNames);
                List<object> NewValues = new List<object>(Values);
                NewValues.AddRange(InsertValues);
                ReturnValue = InsertAndGetID(TableName, NewFieldNames.ToArray(), NewValues.ToArray());
            }
            return ReturnValue;
        }

        public int GetID(string TableName, string[] FieldNames, object[] Values, bool AddIfNotFound)
        {
            string SQLString = "SELECT [ID] FROM " + TableName + " WHERE ";
            string DataString = "";
            for (int i = 0; i < FieldNames.Length; i++)
            {
                if (DataString != "")
                    DataString += " AND ";
                DataString += "[" + FieldNames[i] + "]";
                if (Values[i] == null)
                    DataString += " IS NULL";
                else
                    DataString += " = " + GetValueSQLString(Values[i]);
            }
            int ReturnValue = GetInteger(SQLString + DataString);
            if (ReturnValue == int.MinValue && AddIfNotFound)
                ReturnValue = InsertAndGetID(TableName, FieldNames, Values);
            return ReturnValue;
        }

        public int GetID(string TableName, string[] FieldNames, object[] Values)
        {
            return GetID(TableName, FieldNames, Values, false);
        }

        public string[] GetTableList()
        {
            // This only works on SQL Server & CE
            DataTable TableList =
                GetDataTable("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME > '__%'");
            if (TableList != null)
            {
                List<string> ReturnValue = new List<string>();
                foreach (DataRow DR in TableList.Rows)
                    ReturnValue.Add(DR[0].ToString());
                TableList.Dispose();
                return ReturnValue.ToArray();
            }
            return null;
        }

        public class ColumnDefinition
        {
            public string ColumnName;
            public Type DataType;

            public ColumnDefinition(string ColumnName, Type DataType)
            {
                this.ColumnName = ColumnName;
                this.DataType = DataType;
            }
        }

        public ColumnDefinition[] GetColumns(string TableName)
        {
            // This only works on SQL Server & CE
            DataTable ColumnList =
                GetDataTable("SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE (TABLE_NAME = '" +
                             TableName + "')");
            if (ColumnList != null && ColumnList.Rows.Count > 0)
            {
                ColumnDefinition[] ReturnValue = new ColumnDefinition[ColumnList.Rows.Count];
                for (int i = 0; i < ColumnList.Rows.Count; i++)
                    ReturnValue[i] = new ColumnDefinition(ColumnList.Rows[i][0].ToString(),
                                                          GetTypeFromString(ColumnList.Rows[i][1].ToString()));
                ColumnList.Dispose();
                return ReturnValue;
            }
            return null;
        }

        public bool ColumnExists(string TableName, string ColumnName)
        {
            return
                GetValue("INFORMATION_SCHEMA.COLUMNS", "COLUMN_NAME", new string[] {"TABLE_NAME", "COLUMN_NAME"},
                         new object[] {TableName, ColumnName}) != null;
        }

        public int GetRecordCount(string TableName, string[] FieldNames, object[] Values)
        {
            string SQLString = "SELECT COUNT(*) FROM " + TableName + " WHERE ";
            string DataString = "";
            for (int i = 0; i < FieldNames.Length; i++)
            {
                if (DataString != "")
                    DataString += " AND ";
                DataString += "[" + FieldNames[i] + "]";
                if (Values[i] == null)
                    DataString += " IS NULL";
                else
                    DataString += " = " + GetValueSQLString(Values[i]);
            }
            return GetInteger(SQLString + DataString);
        }

        public string GetDescription(string TableName, string[] FieldNames, object[] Values)
        {
            string SQLString = "SELECT [Description] FROM " + TableName + " WHERE ";
            string DataString = "";
            for (int i = 0; i < FieldNames.Length; i++)
            {
                if (DataString != "")
                    DataString += " AND ";
                DataString += "[" + FieldNames[i] + "]";
                if (Values[i] == null)
                    DataString += " IS NULL";
                else
                    DataString += " = " + GetValueSQLString(Values[i]);
            }
            return GetString(SQLString + DataString);
        }

        #endregion
    }
}
