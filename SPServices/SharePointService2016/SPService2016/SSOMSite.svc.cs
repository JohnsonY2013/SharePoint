using SPService.Contract;
using System;
using SPService.Model;
using System.ServiceModel.Activation;
using SPService2016.Helpers;

namespace SPService2016
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SSite" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select SSite.svc or SSite.svc.cs at the Solution Explorer and start debugging.
    // For Authentication (Not Applicable for OnPremise SharePoint hosted WCF)
    // https://stackoverflow.com/questions/9300927/error-to-use-a-section-registered-as-allowdefinition-machinetoapplication-beyo
    //[AspNetMembershipAuthorizationBehavior()]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SSOMSite : ISSOMSite
    {
        public SSOMMock GetMock()
        {
            return new SSOMMock
            {
                ID = 1,
                Name = "Johnson"
            };
        }

        public SSOMSharePoint GetCentralAdminUrl()
        {
            var CTSSharePoint = InitWrapper("GetCentralAdminUrl");
            try
            {
                CTSSharePoint.Result = new Result
                {
                    Type = "",
                    Value = "http://iti-winsp2016.asiapacific.hpqcorp.net:8888",
                    DESC = "",
                    IsCompressed = false
                };

                CTSSharePoint.Flush();
            }
            catch (Exception ex)
            {
                CTSSharePoint.Flush(true, ex.Source + ":" + ex.Message);
            }

            return CTSSharePoint;
        }

        /// <summary>
        /// Generate a generic wrapper object
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static SSOMSharePoint InitWrapper(string method)
        {
            var wrapper = new SSOMSharePoint();
            wrapper.StartTimeUTC = DateTime.UtcNow;
            wrapper.URL = System.Web.HttpContext.Current.Request.Url.ToString();
            var userName = System.Web.HttpContext.Current.User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                wrapper.UserName = userName.Substring(userName.IndexOf('\\') + 1);
                wrapper.UserDomain = userName.Substring(0, userName.IndexOf('\\'));
            }
            wrapper.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            wrapper.Machine = System.Net.Dns.GetHostName();
            wrapper.Method = method;
            wrapper.Culture = System.Globalization.CultureInfo.CurrentCulture.ToString();
            wrapper.CultureName = System.Globalization.CultureInfo.CurrentCulture.DisplayName;
            return wrapper;
        }
    }

    public static class WrapperExtension
    {
        public static void Flush(this SSOMSharePoint wrapper, bool hasError = false, string errorMsg = "")
        {
            if (wrapper == null) return;

            var endTime = DateTime.UtcNow;
            var duration = endTime.Subtract(wrapper.StartTimeUTC);
            wrapper.EndTimeUTC = endTime;
            wrapper.DurationTicks = duration.Ticks;
            wrapper.DurationTicksPerSecond = (int)duration.TotalSeconds;
            wrapper.Error = hasError;
            wrapper.ErrorMessage = hasError ? errorMsg : string.Empty;
        }
    }
}
