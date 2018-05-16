using System;
using System.Data;

namespace dxc.utilities
{
    public static class ExtDataRow
    {
        public static string GetColumnValue(this DataRow self, string columnName, string defaultValue = "")
        {
            if (self == null) return defaultValue;

            try
            {
                return ToString(self[columnName]);
            }
            catch { return defaultValue; }
        }

        public static bool GetColumnValue(this DataRow self, string columnName, bool defaultValue = false)
        {
            if (self == null) return defaultValue;

            try
            {
                return ToBoolean(self[columnName]);
            }
            catch { return defaultValue; }
        }

        public static DateTime GetColumnValue(this DataRow self, string columnName, DateTime defaultValue)
        {
            if (self == null) return defaultValue;

            try
            {
                return ToDateTime(self[columnName], defaultValue);
            }
            catch { return defaultValue; }
        }

        public static double GetColumnValue(this DataRow self, string columnName, double defaultValue = 0)
        {
            if (self == null) return defaultValue;

            try
            {
                return ToDouble(self[columnName]);
            }
            catch { return defaultValue; }
        }

        public static decimal GetColumnValue(this DataRow self, string columnName, decimal defaultValue = 0)
        {
            if (self == null) return defaultValue;

            try
            {
                return ToDecimal(self[columnName]);
            }
            catch { return defaultValue; }
        }

        public static long GetColumnValue(this DataRow self, string columnName, long defaultValue = 0)
        {
            if (self == null) return defaultValue;

            try
            {
                return ToLong(self[columnName]);
            }
            catch { return defaultValue; }
        }

        public static short GetColumnValue(this DataRow self, string columnName, short defaultValue = 0)
        {
            if (self == null) return defaultValue;

            try
            {
                return ToShort(self[columnName]);
            }
            catch { return defaultValue; }
        }

        public static int GetColumnValue(this DataRow self, string columnName, Int32 defaultValue = 0)
        {
            if (self == null) return defaultValue;

            try
            {
                return ToInt32(self[columnName]);
            }
            catch { return defaultValue; }
        }


        public static UInt32 GetColumnValue(this DataRow self, string columnName, uint defaultValue = 0)
        {
            if (self == null) return defaultValue;

            try
            {
                return ToUInt32(self[columnName]);
            }
            catch { return defaultValue; }
        }

        public static Guid GetColumnValue(this DataRow self, string columnName, Guid defaultValue = new Guid())
        {
            defaultValue = Guid.Empty;

            if (self == null) return defaultValue;

            try
            {
                return ToGuid(self[columnName]);
            }
            catch { return defaultValue; }
        }

        #region Basic Utilities

        static string ToString(object value)
        {
            return value == null ? string.Empty : value.ToString();
        }

        static bool ToBoolean(object value)
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

        static int ToInt32(object value)
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

        static long ToInt64(object value)
        {
            try
            {
                return Convert.ToInt64(Convert.ToDouble(value));
            }
            catch
            {
                return 0;
            }
        }

        static DateTime ToDateTime(object value, DateTime defalutValue)
        {
            try
            {
                if (string.IsNullOrEmpty(value.ToString()))
                    return defalutValue;

                return Convert.ToDateTime(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return defalutValue;
            }
        }

        static double ToDouble(object value)
        {
            try
            {
                return Convert.ToDouble(value);
            }
            catch
            {
                return 0;
            }
        }

        static decimal ToDecimal(object value)
        {
            try
            {
                return Convert.ToDecimal(value);
            }
            catch
            {
                return 0;
            }
        }

        static long ToLong(object value)
        {
            try
            {
                return long.Parse(value.ToString());
            }
            catch
            {
                return 0;
            }
        }

        static short ToShort(object value)
        {
            try
            {
                return short.Parse(value.ToString());
            }
            catch
            {
                return 0;
            }
        }

        static UInt32 ToUInt32(object value)
        {
            try
            {
                return Convert.ToUInt32(value.ToString(), 16);
            }
            catch
            {
                return (UInt32)0;
            }
        }

        static Guid ToGuid(object value)
        {
            try
            {
                return new Guid(value.ToString());
            }
            catch
            {
                return Guid.Empty;
            }
        }

        #endregion
    }
}