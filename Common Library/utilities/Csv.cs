using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace hp.utilities
{
    public class Csv
    {
        private const string FileExtensions = ".csv";
        private const char Delimiter = ',';

        public static string ExportToCsv(DataTable iDataTable, string iCsvFilePath = null, bool iOverwrite = true,
            bool iWithHeader = true)
        {
            try
            {
                if (iDataTable == null || iDataTable.Rows.Count == 0)
                    throw new Exception("No data for exporting!");

                var mCsvFilename = CheckFilename(iCsvFilePath, iOverwrite);

                // Create file before adding data
                if (!File.Exists(mCsvFilename))
                {
                    File.Create(mCsvFilename).Close();
                }

                var mAppendText = new StringBuilder();

                if (iWithHeader)
                {
                    // column headings
                    for (int i = 0; i < iDataTable.Columns.Count; i++)
                    {
                        mAppendText.Append(iDataTable.Columns[i].ColumnName + Delimiter);
                    }
                    File.AppendAllText(mCsvFilename, mAppendText.ToString());
                }

                // rows
                for (int i = 0; i < iDataTable.Rows.Count; i++)
                {
                    mAppendText = new StringBuilder("\n");

                    // to do: format datetime values before printing
                    for (int j = 0; j < iDataTable.Columns.Count; j++)
                    {
                        var mValue = Truncate(jAdapter.ToString(iDataTable.Rows[i][j]), 1024)
                            .Replace('\r', ' ')
                            .Replace('\n', ' ');

                        mAppendText.Append((mValue.Contains(Delimiter.ToString())
                            ? string.Format("\"{0}\"", mValue)
                            : mValue) + Delimiter);
                    }

                    File.AppendAllText(mCsvFilename, mAppendText.ToString());
                }

                return mCsvFilename;
            }
            catch (Exception ex)
            {
                throw new Exception("ExportToCSV: " + ex.Message);
            }
        }

        public delegate void OnRowExported(int iIndex);

        public static OnRowExported TableRowExported;
        // DataReader
        public static string ExportToCsv(string iConnectionString, string iQuery, string iCsvFilePath = null,
            bool iOverwrite = true, bool iWithHeader = true)
        {
            try
            {
                var mCsvFilename = CheckFilename(iCsvFilePath, iOverwrite);

                // Create file before adding data
                if (!File.Exists(mCsvFilename))
                    File.Create(mCsvFilename).Close();

                SqlConnection mConnection = null;
                SqlDataReader mDataReader = null;

                try
                {
                    mConnection = new SqlConnection(iConnectionString);
                    mDataReader = MsSql.GetDataReader(iQuery, ref mConnection);

                    var mAppendText = new StringBuilder();

                    if (iWithHeader)
                    {
                        // column headings
                        for (int i = 0; i < mDataReader.FieldCount; i++)
                        {
                            mAppendText.Append(mDataReader.GetName(i) + Delimiter);
                        }

                        File.AppendAllText(mCsvFilename, mAppendText.ToString());
                    }

                    int mRowIndex = 1;

                    if (mDataReader.HasRows)
                    {
                        while (mDataReader.Read())
                        {
                            mRowIndex++;

                            mAppendText = new StringBuilder("\n");

                            for (int i = 0; i < mDataReader.FieldCount; i++)
                            {
                                mAppendText.Append(jAdapter.ToString(mDataReader[i]) + Delimiter);
                            }

                            File.AppendAllText(mCsvFilename, mAppendText.ToString());

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

                return mCsvFilename;
            }
            catch (Exception ex)
            {
                throw new Exception("ExportToExcel: " + ex.Message);
            }
        }

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
                    iFilePath = iFilePath.TrimEnd('\\') + "\\" + "[" + DateTime.UtcNow.ToString("yyyy-MM-dd") +
                                "]_" + Guid.NewGuid() + FileExtensions;

                if (!iFilePath.EndsWith(FileExtensions))
                    iFilePath += FileExtensions;

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

        public static DataTable ImportToDataTable(string iCsvFilePath, string iTableName = "Table")
        {
            try
            {
                if (!File.Exists(iCsvFilePath)) return null;

                DataTable mReturnValue = null;

                using (var mCsvReader = new StreamReader(iCsvFilePath))
                {
                    mReturnValue = ImportToDataTable(mCsvReader, iTableName);
                }

                return mReturnValue;
            }
            catch
            {
                return null;
            }
        }

        public static DataTable ImportToDataTable(StreamReader iReader, string iTableName = "Table")
        {
            try
            {
                DataTable mReturnValue = null;

                if (iReader == null) return mReturnValue;

                while (!iReader.EndOfStream)
                {
                    var mLine = iReader.ReadLine();

                    if (string.IsNullOrEmpty(mLine)) continue;

                    if (mReturnValue == null)
                    {
                        mReturnValue = new DataTable(iTableName);

                        var mColumns = mLine.Split(Delimiter);

                        for (var i = 0; i < mColumns.Length; i++)
                        {
                            if (!mReturnValue.Columns.Contains(mColumns[i]))
                            {
                                mReturnValue.Columns.Add(mColumns[i]);
                            }
                        }
                    }
                    else
                    {
                        var mNewRow = mReturnValue.NewRow();

                        var mValues = mLine.Split(Delimiter);
                        for (var i = 0; i < mValues.Length; i++)
                        {
                            if (i < mReturnValue.Columns.Count)
                            {
                                mNewRow[i] = mValues[i];
                            }
                        }

                        mReturnValue.Rows.Add(mNewRow);
                    }
                }


                return mReturnValue;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (iReader != null)
                    iReader.Dispose();
            }
        }

        private static string Truncate(string iStr, int iLength)
        {
            if (iStr == null)
                return null;

            if (iStr.Length > iLength)
            {
                if (iLength == 0)
                    return string.Empty;
                if (iLength == 1)
                    return ".";
                if (iLength == 2)
                    return "..";
                if (iLength == 3)
                    return "...";

                return iStr.Substring(0, iLength - 3) + "...";
            }

            return iStr;
        }
    }
}
