using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web.Script.Serialization;

namespace HP.Analytics.Service.Commons.Extensions
{
    public static class ExtDataTable
    {
        // Convert DataTable to json format
        public static string SerializeDataTable(this JavaScriptSerializer self, DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0) return string.Empty;

            var dataTableDictionary = dataTable.ToDictionaryList(); // Convert to list first

            return self.Serialize(dataTableDictionary);
        }

        // Convert arrary list json format to DataTable
        public static DataTable DeserializerTable(this JavaScriptSerializer self, string jsonString)
        {
            var returnValue = new DataTable();
            var arrayList = self.Deserialize<ArrayList>(jsonString);
            if (arrayList.Count > 0)
            {
                foreach (Dictionary<string, object> rowDictionary in arrayList)
                {
                    if (returnValue.Columns.Count == 0)
                    {
                        foreach (string key in rowDictionary.Keys)
                        {
                            returnValue.Columns.Add(key, rowDictionary[key].GetType());
                        }
                    }

                    var dataRow = returnValue.NewRow();
                    foreach (string key in rowDictionary.Keys)
                    {

                        dataRow[key] = rowDictionary[key];
                    }
                    returnValue.Rows.Add(dataRow);
                }
            }

            return returnValue;
        }

        // Convert DataTable to List of Dictionary
        public static List<Dictionary<string, object>> ToDictionaryList(this DataTable self)
        {
            var dataTableToDictionary = new List<Dictionary<string, object>>();
            foreach (DataRow dataRow in self.Rows)
            {
                var rowToDictionary = new Dictionary<string, object>();
                foreach (DataColumn dataColumn in self.Columns)
                {
                    rowToDictionary.Add(dataColumn.ColumnName, dataRow[dataColumn].ToString());
                }
                dataTableToDictionary.Add(rowToDictionary);
            }

            return dataTableToDictionary;
        }
    }
}