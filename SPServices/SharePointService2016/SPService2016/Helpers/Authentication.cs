using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPService2016.Helpers
{
    public class Authentication
    {
        /// <summary>
        /// ensure the request is authenticated,
        /// otherwise send 401 to client, the client should invoke it again;
        /// this method must be invoked at the beginning of a webmethod
        /// </summary>
        public static void EnsureAuthenticated()
        {
            var ctx = System.Web.HttpContext.Current;
            try
            {
                //not allow anonymous request
                if (ctx.Request.IsAuthenticated)
                {
                    //check loginName to make sure it's not empty or invalid (windows login)
                    var loginName = ctx.User.Identity.Name;
                    if (!string.IsNullOrEmpty(loginName)
                        && loginName.IndexOf('\\') != -1)
                    {
                        return;
                    }
                }

                //401 unauthoried
                ctx.Response.StatusCode = 401;
                ctx.Response.End();
            }
            catch (System.Threading.ThreadAbortException)
            {
                //raise exception to stop executing the rest code
                throw;
            }
            catch
            {
                //ignore error
            }
        }

    }
}