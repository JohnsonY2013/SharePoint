using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.DirectoryServices;

namespace hpe.utilities
{
    public class HPEmployee
    {
        public string Login { get; set; }
        public string Email { get; set; }
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string SpinCompany { get; set; }
        public string BusinessGroup { get; set; }
        public string BusinessUnit { get; set; }
        public string ManagerId { get; set; }
    }

    public class LDAP
    {
        private static readonly string LDAPPATH = ConfigurationManager.AppSettings["HPLDAP"] ?? "LDAP://hpe-pro-ods-ed.infra.hpecorp.net/ou=People,o=hp.com";

        public static bool EnsureUserByLogin(string login)
        {
            return EnsureUser("ntuserdomainid", ConvertLoginNameToNTUserDomainID(login));
        }

        public static bool EnsureUserByEmail(string email)
        {
            return EnsureUser("uid", email);
        }

        private static bool EnsureUser(string criterion, string value)
        {
            using (DirectoryEntry dirEntry = new DirectoryEntry(LDAPPATH, null, null, AuthenticationTypes.None))
            {
                using (DirectorySearcher searcher = new DirectorySearcher(dirEntry))
                {
                    searcher.SearchScope = SearchScope.Subtree;
                    searcher.Filter = "(" + criterion + "=" + value + ")";

                    using (SearchResultCollection results = searcher.FindAll())
                    {
                        return results.Count != 0;
                    }
                }
            }
        }

        public static HPEmployee GetHPUserByEmpId(string empId)
        {
            return GetUserBy("employeenumber", empId);
        }

        public static HPEmployee GetHPUserByLogin(string login)
        {
            return GetUserBy("ntuserdomainid", ConvertLoginNameToNTUserDomainID(login));
        }

        public static HPEmployee GetHPUserByEmail(string email)
        {
            return GetUserBy("uid", email);
        }

        private static HPEmployee GetUserBy(string criterion, string value)
        {
            HPEmployee returnValue = null;

            try
            {
                using (var dirEntry = new DirectoryEntry(LDAPPATH, null, null, AuthenticationTypes.None))
                {
                    using (var searcher = new DirectorySearcher(dirEntry))
                    {
                        searcher.SearchScope = SearchScope.Subtree;
                        searcher.Filter = "(" + criterion + "=" + value + ")";
                        searcher.PropertiesToLoad.Add("uid");
                        searcher.PropertiesToLoad.Add("ntuserdomainid");
                        searcher.PropertiesToLoad.Add("hpDisplayNameExtension");
                        searcher.PropertiesToLoad.Add("sn");
                        searcher.PropertiesToLoad.Add("givenName");
                        searcher.PropertiesToLoad.Add("employeenumber");
                        searcher.PropertiesToLoad.Add("hpeSpinCompany");
                        searcher.PropertiesToLoad.Add("hpbusinessgroup");
                        searcher.PropertiesToLoad.Add("hpbusinessunit");
                        searcher.PropertiesToLoad.Add("manageremployeenumber");

                        using (var resultcollection = searcher.FindAll())
                        {
                            if (resultcollection != null && resultcollection.Count > 0)
                            {
                                returnValue = new HPEmployee();

                                returnValue.Login = ConvertNTUserDomainIDToLoginName(GetPropertyValueInString(resultcollection[0].Properties["ntuserdomainid"].Count > 0 ? resultcollection[0].Properties["ntuserdomainid"][0] : ""));
                                returnValue.Email = GetPropertyValueInString(resultcollection[0].Properties["uid"].Count > 0 ? resultcollection[0].Properties["uid"][0] : "");
                                var extension = GetPropertyValueInString(resultcollection[0].Properties["hpDisplayNameExtension"].Count > 0 ? resultcollection[0].Properties["hpDisplayNameExtension"][0] : "");
                                returnValue.Name = GetPropertyValueInString(resultcollection[0].Properties["sn"].Count > 0 ? resultcollection[0].Properties["sn"][0] : "")
                                                    + ", "
                                                    + GetPropertyValueInString(resultcollection[0].Properties["givenName"].Count > 0 ? resultcollection[0].Properties["givenName"][0] : "")
                                                    + (string.IsNullOrEmpty(extension) ? "" : " (" + extension + ")");
                                returnValue.EmployeeId = GetPropertyValueInString(resultcollection[0].Properties["employeenumber"].Count > 0 ? resultcollection[0].Properties["employeenumber"][0] : "");
                                returnValue.SpinCompany = GetPropertyValueInString(resultcollection[0].Properties["hpeSpinCompany"].Count > 0 ? resultcollection[0].Properties["hpeSpinCompany"][0] : "");
                                returnValue.BusinessGroup = GetPropertyValueInString(resultcollection[0].Properties["hpbusinessgroup"].Count > 0 ? resultcollection[0].Properties["hpbusinessgroup"][0] : "");
                                returnValue.BusinessUnit = GetPropertyValueInString(resultcollection[0].Properties["hpbusinessunit"].Count > 0 ? resultcollection[0].Properties["hpbusinessunit"][0] : "");
                                returnValue.ManagerId = GetPropertyValueInString(resultcollection[0].Properties["manageremployeenumber"].Count > 0 ? resultcollection[0].Properties["manageremployeenumber"][0] : "");
                            }
                        }
                    }
                }
            }
            catch { }

            return returnValue;
        }

        private static Dictionary<string, string> GetUserDataBy(string criterion, string value)
        {
            string strLDAPPath = LDAPPATH;

            DirectoryEntry objDirEntry = new DirectoryEntry();
            objDirEntry.AuthenticationType = AuthenticationTypes.None;
            objDirEntry.Path = strLDAPPath;
            DirectorySearcher searcher = new DirectorySearcher(objDirEntry);
            searcher.SearchRoot = objDirEntry;
            searcher.SearchScope = SearchScope.Subtree;
            searcher.Filter = "(" + criterion + "=" + value + ")";

            searcher.PropertiesToLoad.Add("uid");
            searcher.PropertiesToLoad.Add("ntuserdomainid");
            searcher.PropertiesToLoad.Add("hpeSpinCompany");
            searcher.PropertiesToLoad.Add("hpbusinessgroup");
            searcher.PropertiesToLoad.Add("hpbusinessunit");
            searcher.PropertiesToLoad.Add("manageremployeenumber");

            SearchResultCollection resultcollection = null;
            //SearchResult sru = null;
            //DirectoryEntry group;
            try
            {
                resultcollection = searcher.FindAll();
                //sru = searcher.FindOne();
                //group = sru.GetDirectoryEntry();
            }
            catch
            {
                return new Dictionary<string, string>() {
                    { "email", null },
                    { "account", null },
                    { "company", null },
                    { "bg", null },
                    { "bu", null },
                    { "managerid", null }
                };
            }

            DataTable dtresult = new DataTable("Results");
            foreach (string colName in searcher.PropertiesToLoad)
            {
                dtresult.Columns.Add(colName, System.Type.GetType("System.String"));
            }

            string strtemp = "";
            try
            {
                foreach (SearchResult objresult in resultcollection)
                {
                    DataRow dr = dtresult.NewRow();
                    foreach (string colName in searcher.PropertiesToLoad)
                    {
                        if (objresult.Properties.Contains(colName))
                        {
                            strtemp = objresult.Properties[colName][0].ToString();
                            strtemp = strtemp.Replace("$", "");
                            if (colName == "ntuserdomainid")
                            {
                                strtemp = strtemp.Replace(":", "\\");
                            }
                            dr[colName] = strtemp;
                        }
                        else
                        {
                            dr[colName] = "";
                        }
                    }
                    dtresult.Rows.Add(dr);
                }
                dtresult.Columns.Remove("ADSPath");
                return new Dictionary<string, string>() {
                    { "email", (string)dtresult.Rows[0].ItemArray[0] },
                    { "account", (string)dtresult.Rows[0].ItemArray[1] },
                    { "company", (string)dtresult.Rows[0].ItemArray[2] },
                    { "bg", (string)dtresult.Rows[0].ItemArray[3] },
                    { "bu", (string)dtresult.Rows[0].ItemArray[4] },
                    { "managerid", (string)dtresult.Rows[0].ItemArray[5] },
                };
            }
            catch
            {
                return new Dictionary<string, string>() {
                    { "email", null },
                    { "account", null },
                    { "company", null },
                    { "bg", null },
                    { "bu", null },
                    { "managerid", null }
                };
            }
        }

        #region Helper

        private static string RemoveClaim(string pLoginName)
        {
            if (!string.IsNullOrEmpty(pLoginName))
            {
                if (pLoginName.IndexOf('|') > -1)
                    return pLoginName.Substring(pLoginName.IndexOf('|') + 1);
            }

            return pLoginName;
        }

        private static string ConvertLoginNameToNTUserDomainID(string loginName)
        {
            return RemoveClaim(loginName).Replace("\\", ":");
        }

        private static string ConvertNTUserDomainIDToLoginName(string ntUserDomainID)
        {
            return ntUserDomainID.Replace(":", "\\");
        }

        private static string GetPropertyValueInString(object value)
        {
            var type = value.GetType().ToString();

            if (type.Equals("System.Byte[]", StringComparison.InvariantCultureIgnoreCase))
            {
                return System.Text.Encoding.UTF8.GetString(value as byte[]);
            }
            else
            {
                return value.ToString();
            }
        }

        #endregion
    }
}
