using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using hp.utilities;

namespace jIO
{
    public class CSVUtility
    {
        private const string _FILEEXTENSIONS = ".csv";
        private const string _DELIMITER = "\t";

        public static void ExportToCSV(DataTable iDataTable, string iExcelFilePath = null, bool iOverwrite = false)
        {
            try
            {
                if (iDataTable == null || iDataTable.Rows.Count == 0)
                    throw new Exception("Null or empty input table!");

                // Default file name
                if (string.IsNullOrEmpty(iExcelFilePath))
                    iExcelFilePath = DateTime.UtcNow.ToString("yyyy-MM-dd") + "_[" + Guid.NewGuid() + "]" +
                                     _FILEEXTENSIONS;

                // Check file format - extensions equals .csv
                if (!iExcelFilePath.EndsWith(_FILEEXTENSIONS, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new Exception("Specified file is not a valid format!");
                }

                // check folder path
                if (iExcelFilePath.IndexOf('\\') >= 0)
                {
                    string mParentFolder = iExcelFilePath.Substring(0, iExcelFilePath.LastIndexOf('\\')) + "\\";

                    if (!Directory.Exists(mParentFolder))
                        throw new Exception("Specified folder does not exist!");
                }
                else
                {
                    iExcelFilePath = Path.Combine(Environment.CurrentDirectory, iExcelFilePath);
                }

                if (File.Exists(iExcelFilePath))
                {
                    if (iOverwrite)
                        File.Delete(iExcelFilePath);
                    else
                    {
                        throw new Exception("Specified file already exits!");
                    }
                }
                else
                    File.Create(iExcelFilePath).Close();


                StringBuilder mAppendText = new StringBuilder();
                StringBuilder mLineText = new StringBuilder();

                // column headings
                for (int i = 0; i < iDataTable.Columns.Count; i++)
                {
                    mLineText.Append(iDataTable.Columns[i].ColumnName + _DELIMITER);
                }
                mAppendText.AppendLine(mLineText.ToString());

                // rows
                for (int i = 0; i < iDataTable.Rows.Count; i++)
                {
                    mLineText = new StringBuilder();

                    // to do: format datetime values before printing
                    for (int j = 0; j < iDataTable.Columns.Count; j++)
                    {
                        mLineText.Append(iDataTable.Rows[i][j] + _DELIMITER);
                    }
                    mAppendText.AppendLine(mLineText.ToString());
                }
                
                File.AppendAllText(iExcelFilePath, mAppendText.ToString());

            }
            catch (Exception ex)
            {
                throw new Exception("ExportToCSV: " + ex.Message);
            }
        }

        public static void AppendToCSV(DataTable iDataTable, string iExcelFilePath = null)
        {
            try
            {
                if (iDataTable == null || iDataTable.Rows.Count == 0)
                    throw new Exception("Null or empty input table!");

                // Default file name
                if (string.IsNullOrEmpty(iExcelFilePath))
                    iExcelFilePath = DateTime.UtcNow.ToString("yyyy-MM-dd") + "_[" + Guid.NewGuid() + "]" +
                                     _FILEEXTENSIONS;

                // Check file format - extensions equals .csv
                if (!iExcelFilePath.EndsWith(_FILEEXTENSIONS, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new Exception("Specified file is not a valid format!");
                }

                // check folder path
                if (iExcelFilePath.IndexOf('\\') >= 0)
                {
                    string mParentFolder = iExcelFilePath.Substring(0, iExcelFilePath.LastIndexOf('\\')) + "\\";

                    if (!Directory.Exists(mParentFolder))
                        throw new Exception("Specified folder does not exist!");
                }
                else
                {
                    iExcelFilePath = Path.Combine(Environment.CurrentDirectory, iExcelFilePath);
                }

                StringBuilder mAppendText = new StringBuilder();
                StringBuilder mLineText = new StringBuilder();

                // Create file before adding data
                if (!File.Exists(iExcelFilePath))
                {
                    File.Create(iExcelFilePath).Close();

                    // column headings
                    for (int i = 0; i < iDataTable.Columns.Count; i++)
                    {
                        mLineText.Append(iDataTable.Columns[i].ColumnName + _DELIMITER);
                    }

                    mAppendText.AppendLine(mLineText.ToString());
                }

                // rows
                for (int i = 0; i < iDataTable.Rows.Count; i++)
                {
                    mLineText = new StringBuilder();

                    // to do: format datetime values before printing
                    for (int j = 0; j < iDataTable.Columns.Count; j++)
                    {
                        mLineText.Append(jAdapter.ToString(iDataTable.Rows[i][j]) + _DELIMITER);
                    }

                    mAppendText.AppendLine(mLineText.ToString());
                }

                File.AppendAllText(iExcelFilePath, mAppendText.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("ExportToCSV: " + ex.Message);
            }
        }

        public delegate void OnRowExported(int iIndex);

        public static OnRowExported TableRowExported;
        // DataReader
        public static void ExportToCSV(string iConnectionString, string iQuery, string iExcelFilePath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(iConnectionString))
                    throw new Exception("Null or empty input table!");

                // Default file name
                if (string.IsNullOrEmpty(iExcelFilePath))
                    iExcelFilePath = DateTime.UtcNow.ToString("yyyy-MM-dd") + "_[" + Guid.NewGuid() + "]" +
                                     _FILEEXTENSIONS;

                // Check file format - extensions equals .csv
                if (!iExcelFilePath.EndsWith(_FILEEXTENSIONS, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new Exception("Specified file is not a valid format!");
                }

                // check folder path
                if (iExcelFilePath.IndexOf('\\') >= 0)
                {
                    string mParentFolder = iExcelFilePath.Substring(0, iExcelFilePath.LastIndexOf('\\')) + "\\";

                    if (!Directory.Exists(mParentFolder))
                        throw new Exception("Specified folder does not exist!");
                }
                else
                {
                    iExcelFilePath = Path.Combine(Environment.CurrentDirectory, iExcelFilePath);
                }

                // Create file before adding data
                if (!File.Exists(iExcelFilePath))
                    File.Create(iExcelFilePath).Close();

                SqlConnection mConnection = null;
                SqlDataReader mDataReader = null;

                try
                {
                    mConnection = new SqlConnection(iConnectionString);
                    mDataReader = MsSql.GetDataReader(iQuery, ref mConnection);

                    StringBuilder mAppendText = new StringBuilder();

                    // column headings
                    for (int i = 0; i < mDataReader.FieldCount; i++)
                    {
                        mAppendText.Append(mDataReader.GetName(i) + _DELIMITER);
                    }

                    File.AppendAllText(iExcelFilePath, mAppendText.ToString());

                    int mRowIndex = 1;

                    if (mDataReader.HasRows)
                    {
                        while (mDataReader.Read())
                        {
                            mRowIndex++;

                            mAppendText = new StringBuilder("\n");

                            for (int i = 0; i < mDataReader.FieldCount; i++)
                            {
                                mAppendText.Append(jAdapter.ToString(mDataReader[i]) + _DELIMITER);
                            }

                            File.AppendAllText(iExcelFilePath, mAppendText.ToString());

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
            }
            catch (Exception ex)
            {
                throw new Exception("ExportToExcel: " + ex.Message);
            }
        }
    }
}
