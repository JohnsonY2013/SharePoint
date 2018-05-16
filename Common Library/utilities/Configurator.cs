using System;
using System.Configuration;

namespace hpe.utilities
{
    public static class Configurator
    {
        /// <summary>
        /// utility class with configuration
        /// </summary>
        public static class AppSettings
        {
            #region Get Methods

            /// <summary>
            /// read config value as string
            /// </summary>
            /// <param name="key"></param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static string GetValue(string key, string defaultValue = "")
            {
                var returnValue = string.Empty;

                try
                {
                    if (ConfigurationManager.AppSettings.AllKeys.Contains(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (var settingKey in ConfigurationManager.AppSettings.AllKeys)
                        {
                            if (settingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                                return ConfigurationManager.AppSettings.Get(settingKey);
                        }
                    }
                }
                catch { }

                return defaultValue;
            }

            /// <summary>
            /// read config value as int
            /// </summary>
            /// <param name="key"></param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static int GetValue(string key, int defaultValue = 0)
            {
                try
                {
                    return int.Parse(GetValue(key, defaultValue.ToString()));
                }
                catch
                {
                    return defaultValue;
                }
            }

            /// <summary>
            /// read config value as bool
            /// </summary>
            /// <param name="key"></param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static bool GetValue(string key, bool defaultValue = false)
            {
                try
                {
                    var value = GetValue(key, defaultValue.ToString());

                    if (value.Equals("Yes", StringComparison.InvariantCultureIgnoreCase)
                        || value.Equals("True", StringComparison.InvariantCultureIgnoreCase)
                        || value.Equals("1", StringComparison.InvariantCultureIgnoreCase))
                        return true;

                    return bool.Parse(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            /// <summary>
            /// read config value as datetime
            /// </summary>
            /// <param name="key"></param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static DateTime GetValue(string key, DateTime? defaultValue = null)
            {
                try
                {
                    return Convert.ToDateTime(GetValue(key, defaultValue.ToString()));
                }
                catch
                {
                    return defaultValue.HasValue ? defaultValue.Value : DateTime.MinValue;
                }
            }

            #endregion
        }
    }
}
