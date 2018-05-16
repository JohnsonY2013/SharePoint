using SPService.Model;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SPService.Contract
{
    [ServiceContract(Namespace = "http://www.digitcyber.com/")]
    public interface ISSOMSite
    {
        [WebGet]
        [OperationContract]
        [Description("Get the Test response from Site service")]
        SSOMMock GetMock();

        [WebGet]
        [OperationContract]
        [Description("Get the Central Administration Url")] 
        SSOMSharePoint GetCentralAdminUrl();
    }
}
