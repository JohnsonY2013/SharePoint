using System;
using System.ServiceModel.Description;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Web;
using System.IdentityModel.Claims;

namespace SPService2016.Helpers
{
    public class AspNetMembershipAuthorizationPolicy : IAuthorizationPolicy
    {
        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            var context = HttpContext.Current;

            evaluationContext.Properties["Principal"] = context.User;

            if (!context.Request.IsAuthenticated)
                return false;

            //check loginName to make sure it's not empty or invalid (windows login)
            var loginName = context.User.Identity.Name;
            if (string.IsNullOrEmpty(loginName)
            || loginName.IndexOf('\\') == -1)
            {
                return false;
            }
            return true;
        }

        public ClaimSet Issuer
        {
            get { return ClaimSet.System; }
        }

        string id = Guid.NewGuid().ToString();
        public string Id
        {
            get { return this.id; }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class AspNetMembershipAuthorizationBehaviorAttribute : Attribute, IServiceBehavior
    {
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            var policies = new List<IAuthorizationPolicy>();
            policies.Add(new AspNetMembershipAuthorizationPolicy());
            serviceHostBase.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();

            var bh = serviceDescription.Behaviors.Find<ServiceAuthorizationBehavior>();
            if (bh != null)
            {
                bh.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            }
            else
                throw new NotSupportedException();
        }

        public void AddBindingParameters(ServiceDescription serviceDescription,
            System.ServiceModel.ServiceHostBase serviceHostBase,
            System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,
            System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        { }

        public void Validate(ServiceDescription serviceDescription,
            System.ServiceModel.ServiceHostBase serviceHostBase)
        { }
    }

}