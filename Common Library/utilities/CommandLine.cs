using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace hp.utilities
{
    public class CommandLine
    {
        public static Dictionary<string, string> Arguments = null;

        private static Dictionary<string, string> ProcessArguments(string[] iArgs)
        {
            if (iArgs == null || iArgs.Length == 0) return null;

            Arguments = new Dictionary<string, string>();

            // Previous key 
            string mLastKey = string.Empty;

            foreach (string mArgumentItem in iArgs)
            {
                if (!string.IsNullOrEmpty(mArgumentItem) && mArgumentItem.TrimStart().TrimEnd().IndexOf('-') == 0)
                {
                    mLastKey = mArgumentItem.TrimStart().TrimEnd().ToLower();

                    if (!Arguments.ContainsKey(mLastKey))
                    {
                        Arguments.Add(mLastKey, string.Empty);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(mLastKey))
                    {
                        if (Arguments.ContainsKey(mLastKey))
                        {
                            Arguments[mLastKey] = mArgumentItem;
                            mLastKey = string.Empty;
                        }
                    }
                }
            }

            return Arguments;
        }

        public static void Instantiate<T>(T iEntity, string[] iArgs)
        {
            Arguments = ProcessArguments(iArgs);

            if (Arguments == null) return;

            var mFieldList = typeof (T).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (var mField in mFieldList)
            {
                string mFieldKey = "-" + mField.Name.ToLower();
                if (Arguments.ContainsKey(mFieldKey))
                {
                    object mSetValue = null;

                    if (mField.FieldType.ToString().Equals("System.String[]"))
                    {
                        mSetValue = string.IsNullOrEmpty(Arguments[mFieldKey])
                            ? new string[] {}
                            : Arguments[mFieldKey].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => p.Trim()).ToArray();
                    }
                    else
                    {
                        mSetValue = jGadgets.GetValueByType(mField.FieldType.ToString(), Arguments[mFieldKey]);
                    }

                    mField.SetValue(iEntity, mSetValue);
                }
            }
        }
    }
}
