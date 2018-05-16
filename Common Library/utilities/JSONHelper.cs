using System.Web.Script.Serialization;

namespace jy.utilities
{
    public static class JSONHelper
    {
        public static string ToJSON(this object obj)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            return javaScriptSerializer.Serialize(obj);
        }

        public static string ToJSON(this object obj, int recursionDepth)
        {
            return new JavaScriptSerializer
            {
                RecursionLimit = recursionDepth
            }.Serialize(obj);
        }
    }
}
