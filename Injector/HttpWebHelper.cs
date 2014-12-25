using System.IO;
using System.Net;

namespace Injector
{
    static class HttpWebHelper
    {
        public static HttpWebResponse GetResponse(string url)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request != null)
            {
                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                return request.GetResponse() as HttpWebResponse;
            }
            return null;
        }

        public static Stream GetResponseStream(string url)
        {
            var response = GetResponse(url);
            return response.GetResponseStream();
        }

    }
}