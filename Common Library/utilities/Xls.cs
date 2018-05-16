using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

#if OFFICE_INSTALLED
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using Excel = Microsoft.Office.Interop.Excel;
#endif

using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace hp.utilities
{
    /// <summary>
    /// Reference : http://www.codeproject.com/Articles/33850/Generate-Excel-files-without-using-Microsoft-Excel
    /// </summary>
    internal class Xls
    {
        private static readonly string[] FileExtensions = {".xls"};

#if OFFICE_INSTALLED

        public static Dictionary<string, string> GetSheetList(string iExcelFilePath)
        {
            if (string.IsNullOrEmpty(iExcelFilePath))
                throw new Exception("No excel is specified");

            if (!iExcelFilePath.EndsWith(".xls") && !iExcelFilePath.EndsWith(".xlsx"))
                throw new Exception("File type is unsupported");

            if (!File.Exists(iExcelFilePath))
                throw new Exception("File does not exist.");

            string mConnectionStr = iExcelFilePath.EndsWith(".xlsx") ? ConnectionString2 : ConnectionString;
            mConnectionStr = mConnectionStr.Replace("%Path%", iExcelFilePath);

            var mReturnValue = new Dictionary<string, string>();

            using (var mConnection = new OleDbConnection(mConnectionStr))
            {
                OleDbCommand mCommand = new OleDbCommand();
                mCommand.Connection = mConnection;

                mConnection.Open();
                DataTable mSheetTable = mConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (mSheetTable != null && mSheetTable.Rows.Count > 0)
                {
                    foreach (DataRow mDataRow in mSheetTable.Rows)
                    {
                        string mSheetName = Convert.ToString(mDataRow["TABLE_NAME"]);

                        if (mSheetName.EndsWith("$"))
                            mReturnValue.Add(mSheetName, mSheetName.TrimStart('\'').TrimEnd('\'').Replace("$", ""));
                    }
                }
            }
            return mReturnValue;
        }

        public static List<string> GetSheetNames(string iExcelFilePath)
        {
            if (string.IsNullOrEmpty(iExcelFilePath))
                throw new Exception("No excel is specified");

            if (!iExcelFilePath.EndsWith(".xls") && !iExcelFilePath.EndsWith(".xlsx"))
                throw new Exception("File type is unsupported");

            if (!File.Exists(iExcelFilePath))
                throw new Exception("File does not exist.");

            var mReturnValue = new List<string>();

            Excel.Application mExcelApp = new Excel.Application();
            Excel.Workbook mWorkbook = null;
            try
            {
                mWorkbook = mExcelApp.Workbooks.Open(iExcelFilePath);
            }
            catch
            {
                mWorkbook = mExcelApp.Workbooks.Add();
            }

            foreach (Excel.Worksheet mWorkSheet in mWorkbook.Worksheets)
            {
                mReturnValue.Add(mWorkSheet.Name);
            }

            return mReturnValue;
        }

        private const string ConnectionString =
            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=%Path%;Extended Properties='Excel 8.0;HDR=YES;'";

        private const string ConnectionString2 =
            "Provider=Microsoft.ACE.OLEDB.12.0; Data Source=%Path%;Extended Properties='Excel 12.0;HDR=YES;'";

        public static DataTable GetDataTable(string iExcelFilePath, string iSheetName)
        {
            if (string.IsNullOrEmpty(iExcelFilePath))
                throw new Exception("No excel is specified");

            if (!iExcelFilePath.EndsWith(".xls") && !iExcelFilePath.EndsWith(".xlsx"))
                throw new Exception("File type is unsupported");

            if (!File.Exists(iExcelFilePath))
                throw new Exception("File does not exist.");

            string mConnectionStr = iExcelFilePath.EndsWith(".xlsx") ? ConnectionString2 : ConnectionString;
            mConnectionStr = mConnectionStr.Replace("%Path%", iExcelFilePath);
            var mReturnValue = new DataTable();

            using (var mConnection = new OleDbConnection(mConnectionStr))
            {
                OleDbCommand mCommand = new OleDbCommand();
                OleDbDataAdapter mDataAdapter = new OleDbDataAdapter();

                mCommand.Connection = mConnection;
                mCommand.CommandText = "SELECT * FROM [" + iSheetName + "]";

                mConnection.Open();
                mDataAdapter.SelectCommand = mCommand;
                mDataAdapter.Fill(mReturnValue);
                mConnection.Close();
            }

            return mReturnValue;
        }

        public static string ExportToExcel(DataTable iDataTable, string iExcelFilePath = null)
        {
            try
            {
                if (iDataTable == null || iDataTable.Rows.Count == 0)
                    throw new Exception("Null or empty input table!");

                // load excel, and create a new workbook
                Excel.Application mExcelApp = new Excel.Application();
                mExcelApp.Workbooks.Add();

                // single worksheet
                Excel._Worksheet mWorkSheet = mExcelApp.ActiveSheet;

                // column headings
                for (int i = 0; i < iDataTable.Columns.Count; i++)
                {
                    mWorkSheet.Cells[1, (i + 1)] = iDataTable.Columns[i].ColumnName;
                }

                // rows
                for (int i = 0; i < iDataTable.Rows.Count; i++)
                {
                    // to do: format datetime values before printing
                    for (int j = 0; j < iDataTable.Columns.Count; j++)
                    {
                        mWorkSheet.Cells[(i + 2), (j + 1)] = iDataTable.Rows[i][j];
                    }
                }

                // check fielpath
                if (!string.IsNullOrEmpty(iExcelFilePath))
                {
                    try
                    {
                        var mExcelFilename = CheckFilename(iExcelFilePath);

                        mWorkSheet.SaveAs(mExcelFilename);
                        mExcelApp.Quit();

                        return mExcelFilename;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Excel file could not be saved! " + ex.Message);
                    }
                }
                else // no filepath is given
                {
                    mExcelApp.Visible = true;
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ExportToExcel: " + ex.Message);
            }
        }

        public delegate void OnRowExported(int iIndex);

        public static OnRowExported TableRowExported;
        // DataReader
        public static string ExportToExcel(string iConnectionString, string iQuery, string iExcelFilePath = null)
        {
            try
            {
                // load excel, and create a new workbook
                Excel.Application mExcelApp = new Excel.Application();
                mExcelApp.Workbooks.Add();

                // single worksheet
                Excel._Worksheet mWorkSheet = mExcelApp.ActiveSheet;

                SqlConnection mConnection = null;
                SqlDataReader mDataReader = null;

                try
                {
                    mConnection = new SqlConnection(iConnectionString);
                    mDataReader = MsSql.GetDataReader(iQuery, ref mConnection);

                    // column headings
                    for (int i = 0; i < mDataReader.FieldCount; i++)
                    {
                        mWorkSheet.Cells[1, (i + 1)] = mDataReader.GetName(i);
                    }

                    int mRowIndex = 1;

                    if (mDataReader.HasRows)
                    {
                        while (mDataReader.Read())
                        {
                            mRowIndex++;

                            for (int i = 0; i < mDataReader.FieldCount; i++)
                            {
                                mWorkSheet.Cells[mRowIndex, (i + 1)] = jAdapter.ToString(mDataReader[i]);
                            }

                            if (TableRowExported != null)
                                TableRowExported(mRowIndex);
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    if (mDataReader != null)
                        mDataReader.Close();
                    if (mConnection != null)
                    {
                        mConnection.Close();
                        mConnection.Dispose();
                    }
                }


                // check fielpath
                if (!string.IsNullOrEmpty(iExcelFilePath))
                {
                    try
                    {
                        var mExcelFilename = CheckFilename(iExcelFilePath);

                        mWorkSheet.SaveAs(mExcelFilename);
                        mExcelApp.Quit();

                        return mExcelFilename;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Excel file could not be saved! " + ex.Message);
                    }
                }
                else // no filepath is given
                {
                    mExcelApp.Visible = true;
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ExportToExcel: " + ex.Message);
            }
        }
#else

        private static readonly ushort[] CellBegin = {0x0809, 8, 0, 0x10, 0, 0};
        private static readonly ushort[] CellEnd = {0x0A, 00};
        private static Dictionary<string, int> FileRowCount = new Dictionary<string, int>(); 

        public static string ExportToExcel(DataTable iDataTable, string iExcelFilePath = null, bool iOverwrite = true,
            bool iWithHeader = true)
        {
            try
            {
                if (iDataTable == null || iDataTable.Rows.Count == 0)
                    throw new Exception("No data for exporting!");

                var mExportFilename = CheckFilename(iExcelFilePath, iOverwrite);

                if (!FileRowCount.ContainsKey(mExportFilename)) FileRowCount.Add(mExportFilename, 0);

                using (var mStream = new FileStream(mExportFilename, FileMode.OpenOrCreate))
                {
                    using (var mWriter = new BinaryWriter(mStream))
                    {
                        WriteUshortArray(mWriter, CellBegin);

                        if (iWithHeader)
                        {
                            // column headings
                            for (int i = 0; i < iDataTable.Columns.Count; i++)
                            {
                                WriteCell(mWriter, FileRowCount[mExportFilename], i, iDataTable.Columns[i].ColumnName);
                            }

                            FileRowCount[mExportFilename] ++;
                        }

                        // rows
                        for (int i = 0; i < iDataTable.Rows.Count; i++)
                        {
                            // to do: format datetime values before printing
                            for (int j = 0; j < iDataTable.Columns.Count; j++)
                            {
                                WriteCell(mWriter, FileRowCount[mExportFilename] + i, j,
                                    jAdapter.ToString(iDataTable.Rows[i][j]).Truncate(1024));
                            }
                        }

                        FileRowCount[mExportFilename] += iDataTable.Rows.Count;

                        WriteUshortArray(mWriter, CellEnd);
                    }
                }

                return mExportFilename;
            }
            catch (Exception ex)
            {
                throw new Exception("ExportToExcel: " + ex.Message);
            }
        }

        private static void WriteUshortArray(BinaryWriter iWriter, ushort[] iValues)
        {
            foreach (ushort mValue in iValues)
                iWriter.Write(mValue);
        }

        private static void WriteCell(BinaryWriter iWriter, int iRow, int iColumn, object iValue)
        {
            var mValue = iValue == null ? "" : iValue.ToString().Truncate(1000);

            ushort[] mCellData = {0x0204, 0, 0, 0, 0, 0};

            byte[] mPlainText = Encoding.ASCII.GetBytes(mValue);
            mCellData[1] = (ushort) (8 + mValue.Length);
            mCellData[2] = (ushort) iRow;
            mCellData[3] = (ushort) iColumn;
            mCellData[5] = (ushort) mValue.Length;
            WriteUshortArray(iWriter, mCellData);

            iWriter.Write(mPlainText);
        }

#endif

        private static string CheckFilename(string iFilePath, bool iOverwrite = false)
        {
            const string mPathPattern = @"^(?:[a-zA-Z]\:|\\\\[\w\.]+\\[\w.$]+)\\(?:[\w]+\\)*\w([\w.])+$";

            if (string.IsNullOrEmpty(iFilePath))
                iFilePath = Path.Combine(Environment.CurrentDirectory,
                    "[" + DateTime.UtcNow.ToString("yyyy-MM-dd") + "]_" + Guid.NewGuid() + FileExtensions);

            if (!new Regex(mPathPattern).IsMatch(iFilePath))
            {
                try
                {
                    iFilePath = Path.Combine(Environment.CurrentDirectory, iFilePath);
                }
                catch
                {
                    iFilePath = Path.Combine(Environment.CurrentDirectory,
                        "[" + DateTime.UtcNow.ToString("yyyy-MM-dd") + "]_" + Guid.NewGuid() + FileExtensions);
                }
            }

            try
            {
                if (!iFilePath.Substring(iFilePath.LastIndexOf('\\')).Contains("."))
                    iFilePath = iFilePath.TrimEnd('\\') + "\\" + "[" + DateTime.UtcNow.ToString("yyyy-MM-dd") + "]_" +
                                Guid.NewGuid() + FileExtensions;

                var mValidType = FileExtensions.Any(mExtension => iFilePath.EndsWith(mExtension));

                if (!mValidType) iFilePath += FileExtensions[0];

                var mDirectory = iFilePath.Substring(0, iFilePath.LastIndexOf('\\'));

                if (!Directory.Exists(mDirectory)) Directory.CreateDirectory(mDirectory);

                if (iOverwrite && File.Exists(iFilePath)) File.Delete(iFilePath);
            }
            catch
            {
                iFilePath = Path.Combine(Environment.CurrentDirectory,
                    "[" + DateTime.UtcNow.ToString("yyyy-MM-dd") + "]_" + Guid.NewGuid() + FileExtensions);
            }

            return iFilePath;
        }
    }
}
