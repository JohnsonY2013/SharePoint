using System;
using System.Net;
using System.Security.Principal;
using System.Xml.Linq;

namespace SetSiteLock
{
    public class HPSiteProxy
    {
        public static HPIT.EUE.SPSite.HPSiteSoapClient CreateCAProxy(string iSiteUrl, NetworkCredential iCredential)
        {
            var mProxy = CreateProxy(iSiteUrl, iCredential);

            string mErrorMessage;

            string mCaUrl;

            var mXml = XElement.Parse(mProxy.GetCentralAdminUrl());

            if (!IsError(mXml, out mErrorMessage))
            {
                mCaUrl = mXml.Element("Result").Element("Sites").Element("Site").Attribute("URL").Value;
            }
            else
            {
                throw new Exception(mErrorMessage);
            }

            CloseProxy(mProxy);

            return CreateProxy(mCaUrl, iCredential);
        }

        private static HPIT.EUE.SPSite.HPSiteSoapClient CreateProxy(string iSiteUrl, NetworkCredential iCredential)
        {
            var mServieUrl = new Uri(iSiteUrl.TrimEnd('/') + "/_vti_bin/hp/hpsite.asmx");

            var mRemoteAddress = new System.ServiceModel.EndpointAddress(mServieUrl);
            var mHttBinding = new System.ServiceModel.BasicHttpBinding();

            if (mServieUrl.Scheme == Uri.UriSchemeHttps)
            {
                mHttBinding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
                ServicePointManager.ServerCertificateValidationCallback = ((sender,
                    certificate, chain, sslPolicyErrors) => true);
            }

            if (mServieUrl.Scheme == Uri.UriSchemeHttp)
                mHttBinding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;

            mHttBinding.SendTimeout = new TimeSpan(24, 0, 0);
            mHttBinding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Ntlm;
            mHttBinding.Security.Message.ClientCredentialType =
                System.ServiceModel.BasicHttpMessageCredentialType.UserName;

            var mReturnProxy = new HPIT.EUE.SPSite.HPSiteSoapClient(mHttBinding, mRemoteAddress);

            if (mReturnProxy.ClientCredentials == null) return mReturnProxy;

            mReturnProxy.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Delegation;
            mReturnProxy.ClientCredentials.Windows.ClientCredential = iCredential;

            return mReturnProxy;
        }

        private static void CloseProxy(HPIT.EUE.SPSite.HPSiteSoapClient iProxy)
        {
            if (iProxy != null)
            {
                iProxy.Close();
                iProxy = null;
            }
        }

        private static bool IsError(XElement iXElement, out string oMessage)
        {
            var mReturnValue = bool.Parse(iXElement.Attribute("Error").Value);
            oMessage = string.Empty;

            if (mReturnValue)
            {
                oMessage = iXElement.Attribute("ErrorMessage").Value;
            }

            return mReturnValue;
        }
    }
}
