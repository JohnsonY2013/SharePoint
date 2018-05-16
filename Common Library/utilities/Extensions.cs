using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Script.Serialization;

namespace hp.utilities
{
    public static class Extensions
    {
        //internal static bool UpdateWithNonNullValues(this Database iDatabase, string iTableName, string[] iSetFieldNames,
        //    object[] iSetValues, string[] iWhereFieldNames, object[] iWhereValues)
        //{
        //    var mSetFieldNameList = new List<string>();
        //    var mSetValueList = new List<object>();

        //    for (int i = 0; i < iSetValues.Length; i++)
        //    {
        //        if (iSetValues[i] != null && !string.IsNullOrEmpty(iSetValues[i].ToString()))
        //        {
        //            mSetFieldNameList.Add(iSetFieldNames[i]);
        //            mSetValueList.Add(iSetValues[i]);
        //        }
        //    }

        //    return iDatabase.Update(iTableName, mSetFieldNameList.ToArray(), mSetValueList.ToArray(), iWhereFieldNames,
        //        iWhereValues);
        //}

        public static string Truncate(this string iStr, int iLength)
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

        // Convert DataTable to json format
        public static string SerializeDataTable(this JavaScriptSerializer iSerializer, DataTable iDataTable)
        {
            if (iDataTable == null || iDataTable.Rows.Count == 0) return string.Empty;

            var mDicList = iDataTable.ToDictionaryList(); // Convert to list first

            return iSerializer.Serialize(mDicList);
        }

        // Convert arrary list json format to DataTable
        public static DataTable DeserializerTable(this JavaScriptSerializer iSerializer, string iJsonString)
        {
            var mReturnValue = new DataTable();
            var mArralList = iSerializer.Deserialize<ArrayList>(iJsonString);
            if (mArralList.Count > 0)
            {
                foreach (Dictionary<string, object> mRowDic in mArralList)
                {
                    if (mReturnValue.Columns.Count == 0)
                    {
                        foreach (string mKey in mRowDic.Keys)
                        {
                            mReturnValue.Columns.Add(mKey, mRowDic[mKey].GetType());
                        }
                    }

                    var mDataRow = mReturnValue.NewRow();
                    foreach (string mKey in mRowDic.Keys)
                    {

                        mDataRow[mKey] = mDataRow[mKey];
                    }
                    mReturnValue.Rows.Add(mDataRow);
                }
            }

            return mReturnValue;
        }

        // Convert DataTable to List of Dictionary
        public static List<Dictionary<string, object>> ToDictionaryList(this DataTable iDataTable)
        {
            var mDataTableList = new List<Dictionary<string, object>>();
            foreach (DataRow mDataRow in iDataTable.Rows)
            {
                var mRowDic = new Dictionary<string, object>();
                foreach (DataColumn mDataColumn in iDataTable.Columns)
                {
                    mRowDic.Add(mDataColumn.ColumnName, mDataRow[mDataColumn].ToString());
                }
                mDataTableList.Add(mRowDic);
            }

            return mDataTableList;
        }
    }

    public static class jUtilities
    {
        /// <summary>
        /// Deep copy, by Serialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iSource"></param>
        /// <returns></returns>
        public static T Clone_Serialization<T>(T iSource)
        {
            T mReturnValue;

            var mFileStream = new FileStream("Temp.dat", FileMode.Create);
            var mFormatter = new BinaryFormatter();
            try
            {
                mFormatter.Serialize(mFileStream, iSource);
            }
            catch (SerializationException ex)
            {
                throw new Exception(string.Format("Failed to serialize - {0}", ex.Message));
            }
            finally
            {
                mFileStream.Close();
            }

            mFileStream = new FileStream("Temp.dat", FileMode.Open);
            try
            {
                mReturnValue = (T)mFormatter.Deserialize(mFileStream);
            }
            catch (SerializationException ex)
            {
                throw new Exception(string.Format("Failed to deserialize - {0}", ex.Message));
            }
            finally
            {
                mFileStream.Close();

                try
                {
                    File.Delete("Temp.dat");
                }
                catch
                {
                }
            }

            return mReturnValue;
        }

        /// <summary>
        /// Deep copy, by Reflection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iSource"></param>
        /// <returns></returns>
        public static T Clone_Reflection<T>(T iSource)
        {
            T mReturnValue;

            var mTargetType = iSource.GetType();

            if (mTargetType.IsValueType)
            {
                mReturnValue = iSource;
                return mReturnValue;
            }

            mReturnValue = (T) Activator.CreateInstance(mTargetType);

            foreach (var mMember in mTargetType.GetMembers())
            {
                switch (mMember.MemberType)
                {
                    case MemberTypes.Field:
                    {
                        var mField = (FieldInfo) mMember;
                        var mFieldValue = mField.GetValue(iSource);

                        if (mFieldValue is ICloneable)
                            mField.SetValue(mReturnValue, (mFieldValue as ICloneable).Clone());
                        else
                            mField.SetValue(mReturnValue, Clone_Reflection(mFieldValue));
                    }
                        break;
                    case MemberTypes.Property:
                    {
                        var mProperty = (PropertyInfo) mMember;

                        if (mProperty.GetSetMethod(false) != null)
                        {
                            var mPropertyValue = mProperty.GetValue(iSource, null);

                            if (mPropertyValue is ICloneable)
                                mProperty.SetValue(mReturnValue, (mPropertyValue as ICloneable).Clone(), null);
                            else mProperty.SetValue(mReturnValue, Clone_Reflection(mPropertyValue), null);
                        }
                    }
                        break;
                }
            }

            return mReturnValue;
        }

    }

    public static class MemberHelper
    {
        public static string GetMemberName<T>(Expression<Func<T>> iMemberExpression)
        {
            var mExpressionBody = (MemberExpression)iMemberExpression.Body;
            return mExpressionBody.Member.Name;
        }
    }
}
