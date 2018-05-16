//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.OleDb;
//using System.Data.SqlClient;
//using System.IO;
//using Excel = Microsoft.Office.Interop.Excel;

//namespace hp.utilities
//{
//    internal class XlUtility
//    {
//        public static Dictionary<string, string> GetSheetList(string iExcelFilePath)
//        {
//            if (string.IsNullOrEmpty(iExcelFilePath))
//                throw new Exception("No excel is specified");

//            if (!iExcelFilePath.EndsWith(".xls") && !iExcelFilePath.EndsWith(".xlsx"))
//                throw new Exception("File type is unsupported");

//            if (!File.Exists(iExcelFilePath))
//                throw new Exception("File does not exist.");

//            string mConnectionStr = iExcelFilePath.EndsWith(".xlsx") ? ConnectionString2 : ConnectionString;
//            mConnectionStr = mConnectionStr.Replace("%Path%", iExcelFilePath);

//            var mReturnValue = new Dictionary<string, string>();

//            using (var mConnection = new OleDbConnection(mConnectionStr))
//            {
//                OleDbCommand mCommand = new OleDbCommand();
//                mCommand.Connection = mConnection;

//                mConnection.Open();
//                DataTable mSheetTable = mConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

//                if (mSheetTable != null && mSheetTable.Rows.Count > 0)
//                {
//                    foreach (DataRow mDataRow in mSheetTable.Rows)
//                    {
//                        string mSheetName = Convert.ToString(mDataRow["TABLE_NAME"]);

//                        if (mSheetName.EndsWith("$"))
//                            mReturnValue.Add(mSheetName, mSheetName.TrimStart('\'').TrimEnd('\'').Replace("$", ""));
//                    }
//                }
//            }
//            return mReturnValue;
//        }

//        public static List<string> GetSheetNames(string iExcelFilePath)
//        {
//            if (string.IsNullOrEmpty(iExcelFilePath))
//                throw new Exception("No excel is specified");

//            if (!iExcelFilePath.EndsWith(".xls") && !iExcelFilePath.EndsWith(".xlsx"))
//                throw new Exception("File type is unsupported");

//            if (!File.Exists(iExcelFilePath))
//                throw new Exception("File does not exist.");

//            var mReturnValue = new List<string>();

//            Excel.Application mExcelApp = new Excel.Application();
//            Excel.Workbook mWorkbook = null;
//            try
//            {
//                mWorkbook = mExcelApp.Workbooks.Open(iExcelFilePath);
//            }
//            catch
//            {
//                mWorkbook = mExcelApp.Workbooks.Add();
//            }

//            foreach (Excel.Worksheet mWorkSheet in mWorkbook.Worksheets)
//            {
//                mReturnValue.Add(mWorkSheet.Name);
//            }

//            return mReturnValue;
//        }

//        private const string ConnectionString =
//            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=%Path%;Extended Properties='Excel 8.0;HDR=YES;'";

//        private const string ConnectionString2 =
//            "Provider=Microsoft.ACE.OLEDB.12.0; Data Source=%Path%;Extended Properties='Excel 12.0;HDR=YES;'";

//        public static DataTable GetDataTable(string iExcelFilePath, string iSheetName)
//        {
//            if (string.IsNullOrEmpty(iExcelFilePath))
//                throw new Exception("No excel is specified");

//            if (!iExcelFilePath.EndsWith(".xls") && !iExcelFilePath.EndsWith(".xlsx"))
//                throw new Exception("File type is unsupported");

//            if (!File.Exists(iExcelFilePath))
//                throw new Exception("File does not exist.");

//            string mConnectionStr = iExcelFilePath.EndsWith(".xlsx") ? ConnectionString2 : ConnectionString;
//            mConnectionStr = mConnectionStr.Replace("%Path%", iExcelFilePath);
//            var mReturnValue = new DataTable();

//            using (var mConnection = new OleDbConnection(mConnectionStr))
//            {
//                OleDbCommand mCommand = new OleDbCommand();
//                OleDbDataAdapter mDataAdapter = new OleDbDataAdapter();

//                mCommand.Connection = mConnection;
//                mCommand.CommandText = "SELECT * FROM [" + iSheetName + "]";

//                mConnection.Open();
//                mDataAdapter.SelectCommand = mCommand;
//                mDataAdapter.Fill(mReturnValue);
//                mConnection.Close();
//            }

//            return mReturnValue;
//        }

//        public static void ExportToExcel(DataTable iDataTable, string iExcelFilePath = null)
//        {
//            try
//            {
//                if (iDataTable == null || iDataTable.Rows.Count == 0)
//                    throw new Exception("Null or empty input table!");

//                // load excel, and create a new workbook
//                Excel.Application mExcelApp = new Excel.Application();
//                mExcelApp.Workbooks.Add();

//                // single worksheet
//                Excel._Worksheet mWorkSheet = mExcelApp.ActiveSheet;

//                // column headings
//                for (int i = 0; i < iDataTable.Columns.Count; i++)
//                {
//                    mWorkSheet.Cells[1, (i + 1)] = iDataTable.Columns[i].ColumnName;
//                }

//                // rows
//                for (int i = 0; i < iDataTable.Rows.Count; i++)
//                {
//                    // to do: format datetime values before printing
//                    for (int j = 0; j < iDataTable.Columns.Count; j++)
//                    {
//                        mWorkSheet.Cells[(i + 2), (j + 1)] = iDataTable.Rows[i][j];
//                    }
//                }

//                // check fielpath
//                if (!string.IsNullOrEmpty(iExcelFilePath))
//                {
//                    try
//                    {
//                        mWorkSheet.SaveAs(iExcelFilePath);
//                        mExcelApp.Quit();
//                    }
//                    catch (Exception ex)
//                    {
//                        throw new Exception("Excel file could not be saved! " + ex.Message);
//                    }
//                }
//                else // no filepath is given
//                {
//                    mExcelApp.Visible = true;
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("ExportToExcel: " + ex.Message);
//            }
//        }

//        public delegate void OnRowExported(int iIndex);

//        public static OnRowExported TableRowExported;
//        // DataReader
//        public static void ExportToExcel(string iConnectionString, string iQuery, string iExcelFilePath = null)
//        {
//            try
//            {
//                // load excel, and create a new workbook
//                Excel.Application mExcelApp = new Excel.Application();
//                mExcelApp.Workbooks.Add();

//                // single worksheet
//                Excel._Worksheet mWorkSheet = mExcelApp.ActiveSheet;

//                SqlConnection mConnection = null;
//                SqlDataReader mDataReader = null;

//                try
//                {
//                    mConnection = new SqlConnection(iConnectionString);
//                    mDataReader = MsSql.GetDataReader(iQuery, ref mConnection);

//                    // column headings
//                    for (int i = 0; i < mDataReader.FieldCount; i++)
//                    {
//                        mWorkSheet.Cells[1, (i + 1)] = mDataReader.GetName(i);
//                    }

//                    int mRowIndex = 1;

//                    if (mDataReader.HasRows)
//                    {
//                        while (mDataReader.Read())
//                        {
//                            mRowIndex++;

//                            for (int i = 0; i < mDataReader.FieldCount; i++)
//                            {
//                                mWorkSheet.Cells[mRowIndex, (i + 1)] = jAdapter.ToString(mDataReader[i]);
//                            }

//                            if (TableRowExported != null)
//                                TableRowExported(mRowIndex);
//                        }
//                    }
//                }
//                catch
//                {
//                }
//                finally
//                {
//                    if (mDataReader != null)
//                        mDataReader.Close();
//                    if (mConnection != null)
//                    {
//                        mConnection.Close();
//                        mConnection.Dispose();
//                    }
//                }


//                // check fielpath
//                if (!string.IsNullOrEmpty(iExcelFilePath))
//                {
//                    try
//                    {
//                        mWorkSheet.SaveAs(iExcelFilePath);
//                        mExcelApp.Quit();
//                    }
//                    catch (Exception ex)
//                    {
//                        throw new Exception("Excel file could not be saved! " + ex.Message);
//                    }
//                }
//                else // no filepath is given
//                {
//                    mExcelApp.Visible = true;
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("ExportToExcel: " + ex.Message);
//            }
//        }
//    }
//}
