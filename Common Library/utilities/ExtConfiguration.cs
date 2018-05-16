using System;
using System.Configuration;

namespace hpe.utilities
{
    public static partial class HPEExtensions
    {
        /// <summary>
        /// if config contains key
        /// </summary>
        /// <param name="config"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ContainsKey(this Configuration config, string key)
        {
            var setting = config.AppSettings.Settings[key];

            if (setting != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// set config value
        /// </summary>
        /// <param name="config"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>if add or update, return true;otherwise false</returns>
        public static bool SetValue(this Configuration config, string key, string value)
        {
            var setting = config.AppSettings.Settings[key];
            if (setting != null)
            {
                string oldValue = config.AppSettings.Settings[key].Value;
                if (oldValue != value)
                {
                    config.AppSettings.Settings[key].Value = value;
                    return true;
                }
                return false;
            }
            else
            {
                config.AppSettings.Settings.Add(key, value);
                return true;
            }
        }
    }
}
