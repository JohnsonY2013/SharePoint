using System;
using System.Data;
using System.Globalization;

namespace hp.utilities
{
    internal class jGadgets
    {
        internal const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Get the value according to the type
        /// </summary>
        /// <param name="iType"></param>
        /// <param name="iValue"></param>
        /// <returns></returns>
        internal static object GetValueByType(string iType, string iValue)
        {
            object mReturnValue = null;
            try
            {
                switch (iType)
                {
                    case "System.Int16":
                        mReturnValue = jAdapter.ToInt16(iValue);
                        break;
                    case "System.Int32":
                        mReturnValue = jAdapter.ToInt32(iValue);
                        break;
                    case "System.Int64":
                        mReturnValue = jAdapter.ToInt64(iValue);
                        break;
                    case "System.Double":
                        mReturnValue = jAdapter.ToDouble(iValue);
                        break;
                    case "System.Decimal":
                        mReturnValue = jAdapter.ToDecimal(iValue);
                        break;
                    case "System.DateTime":
                        mReturnValue = jAdapter.ToDateTime(iValue, new DateTime(1800, 1, 1));
                        break;
                    case "System.Boolean":
                        mReturnValue = jAdapter.ToBoolean(iValue);
                        break;
                    case "System.Guid":
                        mReturnValue = jAdapter.ToGuid(iValue);
                        break;
                    default:
                        mReturnValue = jAdapter.ToString(iValue);
                        break;
                }
            }
            catch
            {
            }
            return mReturnValue;
        }

        #region IO.Helper

        /// <summary>
        /// Get filename from the filepath
        /// Return string.Empty if exception
        /// </summary>
        /// <param name="FullPath"></param>
        /// <param name="IsVerified"></param>
        /// <returns></returns>
        internal static string GetFilename(string FullPath, bool IsVerified = false)
        {
            try
            {
                if (IsVerified && !System.IO.File.Exists(FullPath))
                    return string.Empty;

                return FullPath.Substring(FullPath.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get filename with out extension
        /// Return string.Empty if excetpion
        /// </summary>
        /// <param name="FullPath"></param>
        /// <param name="IsVerified"></param>
        /// <returns></returns>
        internal static string GetFilenameWithoutExtension(string FullPath, bool IsVerified = false)
        {
            try
            {
                string Filename = GetFilename(FullPath, IsVerified);

                if (string.IsNullOrEmpty(Filename))
                    return string.Empty;
                return Filename.Substring(0, Filename.LastIndexOf(".", StringComparison.Ordinal));
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static string GetDirectoryPath(string FullPath)
        {
            try
            {
                if (IsDirectory(FullPath))
                    return FullPath;

                if (IsFile(FullPath))
                {
                    string FolderPath = FullPath.Substring(0, FullPath.LastIndexOf('\\'));
                    
                    if (IsDirectory(FolderPath))
                        return FolderPath;
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        internal static bool IsFile(string FullPath)
        {
            try
            {
                if (System.IO.File.Exists(FullPath))
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        internal static bool IsDirectory(string FullPath)
        {
            try
            {
                if (System.IO.Directory.Exists(FullPath))
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        // It could be same in muilt threads 
        private static Random _Random = new Random(Guid.NewGuid().GetHashCode());
        /// <summary>
        /// Get a random number
        /// </summary>
        /// <returns></returns>
        internal static int RandomNumber()
        {
            return _Random.Next();
        }
    }
    
    /// <summary>
    /// Toolset - Helper for type convert
    /// </summary>
    internal class jAdapter
    {
        /// <summary>
        /// Convert input string to boolean
        /// Return false if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static bool ToBoolean(object Value)
        {
            try
            {
                if (ToInt32(Value) == 1) return true;

                return Convert.ToBoolean(Value);
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Convert input string to datetime
        /// Return DateTime.MinValue if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static DateTime ToDateTime(object Value, DateTime DefalutValue)
        {
            try
            {
                if (string.IsNullOrEmpty(Value.ToString()))
                    return DefalutValue;

                return Convert.ToDateTime(Value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return DefalutValue;
            }
        }

        /// <summary>
        /// Convert input string to timespan
        /// Return DateTime.Zero if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static TimeSpan ToTimeSpan(object Value)
        {
            try
            {
                return TimeSpan.Parse(Value.ToString());
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Convert input string to double
        /// Return 0 if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static double ToDouble(object Value)
        {
            try
            {
                return Convert.ToDouble(Value);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Convert input string to long
        /// Return 0 if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static long ToLong(object Value)
        {
            try
            {
                return long.Parse(Value.ToString());
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Convert input string to decimal
        /// Return 0 if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static decimal ToDecimal(object Value)
        {
            try
            {
                return Convert.ToDecimal(Value);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Convert input string to short
        /// Return 0 if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static short ToShort(object Value)
        {
            try
            {
                return short.Parse(Value.ToString());
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Convert input string to int32
        /// Return 0 if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static int ToInt32(object Value)
        {
            try
            {
                return Convert.ToInt32(Convert.ToDouble(Value));
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Convert input string to Int16
        /// Return 0 if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static Int16 ToInt16(object Value)
        {
            try
            {
                return Convert.ToInt16(Convert.ToDouble(Value));
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Convert input string to Int64
        /// Return 0 if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static Int64 ToInt64(object Value)
        {
            try
            {
                return Convert.ToInt64(Convert.ToDouble(Value));
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Convert input string to UInt16
        /// Return 0 if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static UInt16 ToUInt16(object Value)
        {
            try
            {
                return Convert.ToUInt16(Value);
            }
            catch
            {
                return (UInt16) 0;
            }
        }

        /// <summary>
        /// Convert input string to UInt32
        /// Return 0 if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static UInt32 ToUInt32(object Value)
        {
            try
            {
                return Convert.ToUInt32(Value.ToString(), 16);
            }
            catch
            {
                return (UInt32) 0;
            }
        }

        /// <summary>
        /// Convert input string to UInt64
        /// Return 0 if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static UInt64 ToUInt64(object Value)
        {
            try
            {
                return Convert.ToUInt16(Value);
            }
            catch
            {
                return (UInt64) 0;
            }
        }

        /// <summary>
        /// Convert input string to Guid
        /// Return Guid.Empty if exception
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static Guid ToGuid(object Value)
        {
            try
            {
                return new Guid(Value.ToString());
            }
            catch
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Convert input string to string
        /// Return string.Empty if null
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        internal static string ToString(object Value)
        {
            return Value == null ? string.Empty : Value.ToString();
        }
    }
    
    //internal static class jDbAdapter
    //{
    //    internal static bool ToBoolean(DataRow datarow, string columnName)
    //    {
    //        try
    //        {
    //            return jAdapter.ToBoolean(datarow[columnName]);
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }

    //    internal static DateTime ToDateTime(DataRow datarow, string columnName, DateTime DefalutValue)
    //    {
    //        try
    //        {
    //            return jAdapter.ToDateTime(datarow[columnName], DefalutValue);
    //        }
    //        catch
    //        {
    //            return DefalutValue;
    //        }
    //    }

    //    internal static double ToDouble(DataRow datarow, string columnName)
    //    {
    //        try
    //        {
    //            return jAdapter.ToDouble(datarow[columnName]);
    //        }
    //        catch
    //        {
    //            return 0;
    //        }
    //    }

    //    internal static decimal ToDecimal(DataRow datarow, string columnName)
    //    {
    //        try
    //        {
    //            return jAdapter.ToDecimal(datarow[columnName]);
    //        }
    //        catch
    //        {
    //            return 0;
    //        }
    //    }

    //    internal static double ToLong(DataRow datarow, string columnName)
    //    {
    //        try
    //        {
    //            return jAdapter.ToLong(datarow[columnName]);
    //        }
    //        catch
    //        {
    //            return 0;
    //        }
    //    }

    //    internal static short ToShort(DataRow datarow, string columnName)
    //    {
    //        try
    //        {
    //            return jAdapter.ToShort(datarow[columnName]);
    //        }
    //        catch
    //        {
    //            return 0;
    //        }
    //    }

    //    internal static Int32 ToInt32(DataRow datarow, string columnName)
    //    {
    //        try
    //        {
    //            return jAdapter.ToInt32(datarow[columnName]);
    //        }
    //        catch
    //        {
    //            return 0;
    //        }
    //    }

    //    internal static Int64 ToInt64(DataRow datarow, string columnName)
    //    {
    //        try
    //        {
    //            return jAdapter.ToInt64(datarow[columnName]);
    //        }
    //        catch
    //        {
    //            return 0;
    //        }
    //    }

    //    internal static string ToString(DataRow datarow, string columnName)
    //    {
    //        try
    //        {
    //            return jAdapter.ToString(datarow[columnName]);
    //        }
    //        catch
    //        {
    //            return string.Empty;
    //        }
    //    }

    //    internal static Guid ToGuid(DataRow datarow, string columnName)
    //    {
    //        try
    //        {
    //            return jAdapter.ToGuid(datarow[columnName]);
    //        }
    //        catch
    //        {
    //            return Guid.Empty;
    //        }
    //    }

    //}

}
