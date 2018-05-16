using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace dxc.utilities
{
    internal static class MsSql
    {
        internal static DataTable GetDataTable(string connectionString, string query,
            CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(query))
                {
                    command.CommandType = commandType;
                    command.Connection = connection;
                    command.CommandTimeout = 0;

                    #region Add Parameters

                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                    #endregion

                    var dataSet = new DataSet();

                    try
                    {
                        var dataAdapter = new SqlDataAdapter(command);
                        dataAdapter.Fill(dataSet);
                        return dataSet.Tables[0];
                    }
                    catch { }
                    finally
                    {
                        connection.Close();
                    }

                    return null;
                }
            }
        }

        internal static SqlDataReader GetDataReader(string query, ref SqlConnection sqlConnection)
        {
            SqlDataReader returnValue = null;
            SqlCommand command = null;

            try
            {
                if (sqlConnection != null)
                {
                    if (sqlConnection.State != ConnectionState.Open)
                        sqlConnection.Open();

                    command = sqlConnection.CreateCommand();
                    command.CommandTimeout = 0;
                    command.CommandText = query;

                    returnValue = command.ExecuteReader();
                }
            }
            catch { }
            finally
            {
                if (command != null)
                    command.Dispose();
            }

            return returnValue;
        }

        internal static int Execute(string connectionString, string query,
            CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            return ExecuteNonQuery(connectionString, query, commandType, parameters);
        }

        internal static bool Insert(string connectionString, string tableName, string[] fieldNames, object[] values, bool lockTable = false)
        {
            var fieldsString = "";
            var valuesString = "";

            for (int i = 0; i < fieldNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(fieldsString))
                    fieldsString += ", ";
                fieldsString += "`" + fieldNames[i] + "`";

                if (!string.IsNullOrEmpty(valuesString))
                    valuesString += ", ";
                valuesString += "@" + fieldNames[i];
            }

            var query = "INSERT INTO `" + tableName + "` (" +
                          fieldsString + ") VALUES (" + valuesString + ")";

            var sqlParameters = new List<SqlParameter>();

            for (int i = 0; i < fieldNames.Length; i++)
            {
                var thisField = "@" + fieldNames[i];

                if (values.Length >= i)
                {
                    var value = values[i];

                    if (value == null)
                        sqlParameters.Add(new SqlParameter(thisField, DBNull.Value));
                    else
                    {
                        var parameter = new SqlParameter(thisField, GetDbType(value));
                        parameter.Value = value;
                        sqlParameters.Add(parameter);
                    }
                }
                else
                {
                    sqlParameters.Add(new SqlParameter(thisField, DBNull.Value));
                }
            }

            return ExecuteNonQuery(connectionString, query, CommandType.Text, sqlParameters.ToArray()) > 0;
        }

        internal static bool Update(string connectionString, string tableName, string[] setFields,
            object[] setValues, string[] whereFields, object[] whereValues, bool lockTable = false)
        {
            var query = "UPDATE [" + tableName + "] " + (lockTable ? " WITH (TABLOCK) " : "") + " SET ";
            var dataString = "";
            for (int i = 0; i < setFields.Length; i++)
            {
                if (dataString != "")
                    dataString += ", ";
                dataString += "[" + setFields[i] + "] = ";

                dataString += " @" + setFields[i];
            }

            query += dataString;
            dataString = "";
            for (int i = 0; i < whereFields.Length; i++)
            {
                if (dataString != "")
                    dataString += " AND ";
                dataString += "[" + whereFields[i] + "]";

                dataString += " = @" + whereFields[i];
            }

            query += " WHERE " + dataString;

            var sqlParameters = new List<SqlParameter>();

            for (int i = 0; i < setFields.Length; i++)
            {
                var thisField = "@" + setFields[i];

                if (setValues.Length >= i)
                {
                    var value = setValues[i];

                    if (value == null)
                        sqlParameters.Add(new SqlParameter(thisField, DBNull.Value));
                    else
                    {
                        var parameter = new SqlParameter(thisField, GetDbType(value));
                        parameter.Value = value;
                        sqlParameters.Add(parameter);
                    }
                }
                else
                {
                    sqlParameters.Add(new SqlParameter(thisField, DBNull.Value));
                }
            }

            for (int i = 0; i < whereFields.Length; i++)
            {
                var thisField = "@" + whereFields[i];

                if (whereValues.Length >= i)
                {
                    var value = whereValues[i];

                    if (value == null)
                        sqlParameters.Add(new SqlParameter(thisField, DBNull.Value));
                    else
                    {
                        var parameter = new SqlParameter(thisField, GetDbType(value));
                        parameter.Value = value;
                        sqlParameters.Add(parameter);
                    }
                }
                else
                {
                    sqlParameters.Add(new SqlParameter(thisField, DBNull.Value));
                }
            }

            return ExecuteNonQuery(connectionString, query, CommandType.Text, sqlParameters.ToArray()) > 0;
        }

        internal static bool DoBulkCopy(string connectionString, string tableName, DataTable dataTable)
        {
            try
            {
                using (SqlBulkCopy bulkCopy =
                    new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction | SqlBulkCopyOptions.TableLock))
                {
                    bulkCopy.BulkCopyTimeout = 0;
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.WriteToServer(dataTable);
                }

                return true;
            }
            catch { }
            return false;
        }

        internal static string GetValue(string connectionString, string query, string defaultValue,
            CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            var returnValue = ExecuteScalar(connectionString, query, commandType, parameters);

            if (returnValue == null) return defaultValue;
            return ToString(returnValue);
        }

        internal static int GetValue(string connectionString, string query, int defaultValue,
            CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            var returnValue = ExecuteScalar(connectionString, query, commandType, parameters);

            if (returnValue == null) return defaultValue;
            return ToInt32(returnValue);
        }

        internal static bool GetValue(string connectionString, string query, bool defaultValue,
            CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            var returnValue = ExecuteScalar(connectionString, query, commandType, parameters);

            if (returnValue == null) return defaultValue;
            return ToBoolean(returnValue);
        }

        internal static bool Test(string connectionString)
        {
            var connection = new SqlConnection(connectionString);

            using (connection)
            {
                connection.Open();
            }

            return true;
        }

        #region Basic Functions

        private static SqlDbType GetDbType(object value)
        {
            var returnType = SqlDbType.NVarChar;
            switch (value.GetType().Name)
            {
                case "Boolean":
                    returnType = SqlDbType.Bit;
                    break;

                case "Byte":
                    returnType = SqlDbType.TinyInt;
                    break;

                case "Int16":
                    returnType = SqlDbType.SmallInt;
                    break;

                case "Int32":
                    returnType = SqlDbType.Int;
                    break;

                case "Single":
                    returnType = SqlDbType.Real;
                    break;

                case "Double":
                    returnType = SqlDbType.Float;
                    break;

                case "TimeSpan":
                    returnType = SqlDbType.Time;
                    break;

                case "DateTime":
                    returnType = SqlDbType.DateTime;
                    break;

                case "Guid":
                    returnType = SqlDbType.UniqueIdentifier;
                    break;

                case "Byte[]":
                    returnType = SqlDbType.Image;
                    break;

                case "String":
                    if (value.ToString().Length > 4000)
                        returnType = SqlDbType.NText;
                    break;
            }

            return returnType;
        }


        private static int ExecuteNonQuery(string connectionString, string query,
            CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(query))
                {
                    command.CommandType = commandType;
                    command.Connection = connection;
                    command.CommandTimeout = 0;

                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        private static object ExecuteScalar(string connectionString, string commandText,
            CommandType commandType, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(commandText))
                {
                    command.CommandType = commandType;
                    command.Connection = connection;
                    command.CommandTimeout = 0;

                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
        }

        private static int ToInt32(object value)
        {
            try
            {
                return Convert.ToInt32(Convert.ToDouble(value));
            }
            catch
            {
                return 0;
            }
        }

        private static string ToString(object value)
        {
            return value == null ? string.Empty : value.ToString();
        }

        private static bool ToBoolean(object value)
        {
            try
            {
                if (ToInt32(value) == 1) return true;

                return Convert.ToBoolean(value);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
