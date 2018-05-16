using System;
using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Generic;

namespace dxc.utilities
{
    internal static class MySql
    {
        internal static DataTable GetDataTable(string connectionString, string query,
            CommandType commandType = CommandType.Text, params MySqlParameter[] parameters)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var command = new MySqlCommand(query))
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
                        var dataAdapter = new MySqlDataAdapter(command);
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

        internal static MySqlDataReader GetDataReader(string query, ref MySqlConnection sqlConnection)
        {
            MySqlDataReader returnValue = null;
            MySqlCommand command = null;

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
            CommandType commandType = CommandType.Text, params MySqlParameter[] parameters)
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

            var sqlParameters = new List<MySqlParameter>();

            for (int i = 0; i < fieldNames.Length; i++)
            {
                var thisField = "@" + fieldNames[i];

                if (values.Length >= i)
                {
                    var value = values[i];

                    if (value == null)
                        sqlParameters.Add(new MySqlParameter(thisField, DBNull.Value));
                    else
                    {
                        var parameter = new MySqlParameter(thisField, GetDbType(value));
                        parameter.Value = value;
                        sqlParameters.Add(parameter);
                    }
                }
                else
                {
                    sqlParameters.Add(new MySqlParameter(thisField, DBNull.Value));
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

            var sqlParameters = new List<MySqlParameter>();

            for (int i = 0; i < setFields.Length; i++)
            {
                var thisField = "@" + setFields[i];

                if (setValues.Length >= i)
                {
                    var value = setValues[i];

                    if (value == null)
                        sqlParameters.Add(new MySqlParameter(thisField, DBNull.Value));
                    else
                    {
                        var parameter = new MySqlParameter(thisField, GetDbType(value));
                        parameter.Value = value;
                        sqlParameters.Add(parameter);
                    }
                }
                else
                {
                    sqlParameters.Add(new MySqlParameter(thisField, DBNull.Value));
                }
            }

            for (int i = 0; i < whereFields.Length; i++)
            {
                var thisField = "@" + whereFields[i];

                if (whereValues.Length >= i)
                {
                    var value = whereValues[i];

                    if (value == null)
                        sqlParameters.Add(new MySqlParameter(thisField, DBNull.Value));
                    else
                    {
                        var parameter = new MySqlParameter(thisField, GetDbType(value));
                        parameter.Value = value;
                        sqlParameters.Add(parameter);
                    }
                }
                else
                {
                    sqlParameters.Add(new MySqlParameter(thisField, DBNull.Value));
                }
            }

            return ExecuteNonQuery(connectionString, query, CommandType.Text, sqlParameters.ToArray()) > 0;
        }

        internal static string GetValue(string connectionString, string query, string defaultValue,
            CommandType commandType = CommandType.Text, params MySqlParameter[] parameters)
        {
            var returnValue = ExecuteScalar(connectionString, query, commandType, parameters);

            if (returnValue == null) return defaultValue;
            return ToString(returnValue);
        }

        internal static int GetValue(string connectionString, string query, int defaultValue,
            CommandType commandType = CommandType.Text, params MySqlParameter[] parameters)
        {
            var returnValue = ExecuteScalar(connectionString, query, commandType, parameters);

            if (returnValue == null) return defaultValue;
            return ToInt32(returnValue);
        }

        internal static bool GetValue(string connectionString, string query, bool defaultValue,
            CommandType commandType = CommandType.Text, params MySqlParameter[] parameters)
        {
            var returnValue = ExecuteScalar(connectionString, query, commandType, parameters);

            if (returnValue == null) return defaultValue;
            return ToBoolean(returnValue);
        }

        //internal static bool DoBulkCopy(string iConnectionString, string iTableName, DataTable iDataTable)
        //{
        //    MySqlBulkCopy mBulkCopy = null;
        //    try
        //    {
        //        mBulkCopy = new SqlBulkCopy(iConnectionString, SqlBulkCopyOptions.UseInternalTransaction | MySqlBulkCopyOptions.TableLock)
        //        {
        //            BulkCopyTimeout = 0,
        //            DestinationTableName = iTableName
        //        };
        //        mBulkCopy.WriteToServer(iDataTable);

        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //    finally
        //    {
        //        try
        //        {
        //            mBulkCopy.Close();
        //        }
        //        catch
        //        {
        //        }
        //    }
        //}

        internal static bool Test(string connectionString)
        {
            var connection = new MySqlConnection(connectionString);

            using (connection)
            {
                connection.Open();
            }

            return true;
        }

        #region Basic Functions

        private static MySqlDbType GetDbType(object value)
        {
            var returnType = MySqlDbType.VarChar;
            switch (value.GetType().Name)
            {
                case "Boolean":
                    returnType = MySqlDbType.Bit;
                    break;

                case "Byte":
                    returnType = MySqlDbType.Byte;
                    break;

                case "Int16":
                    returnType = MySqlDbType.Int16;
                    break;

                case "Int32":
                    returnType = MySqlDbType.Int32;
                    break;

                case "Single":
                    returnType = MySqlDbType.Float;
                    break;

                case "Double":
                    returnType = MySqlDbType.Double;
                    break;

                case "TimeSpan":
                    returnType = MySqlDbType.Time;
                    break;

                case "DateTime":
                    returnType = MySqlDbType.DateTime;
                    break;

                case "Guid":
                    returnType = MySqlDbType.Guid;
                    break;

                case "Byte[]":
                    returnType = MySqlDbType.Byte; // TBD
                    break;

                case "String":
                    if (value.ToString().Length > 4000)
                        returnType = MySqlDbType.Text;
                    break;
            }

            return returnType;
        }

        private static int ExecuteNonQuery(string connectionString, string query,
            CommandType commandType = CommandType.Text, params MySqlParameter[] parameters)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var command = new MySqlCommand(query))
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
            CommandType commandType, params MySqlParameter[] parameters)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var command = new MySqlCommand(commandText))
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
