using System;
using System.Net;
using System.Net.Cache;
using System.IO;
using System.Text;

namespace voivode
{
    public class UserAgent
    {
        public string Accept;
        public string Agent;
        public string AcceptLanguage;
        public string AcceptCharset;

        public static void Apply(UserAgent that, HttpWebRequest request)
        {
            if (that != null)
                that.Apply(request);
        }
        
        public void Apply(HttpWebRequest request)
        {
            request.UserAgent = Agent;
            request.Accept = Accept;
            if (AcceptLanguage != null)
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, AcceptLanguage);
            if (AcceptCharset != null)
                request.Headers.Add(HttpRequestHeader.AcceptCharset, AcceptCharset);
        }

        // Opera 11.51
        public static readonly UserAgent Opera = new UserAgent
        {
            Agent =
            "Opera/9.80 (Windows NT 5.1; U; en) Presto/2.9.168 Version/11.51",
            Accept =
            "text/html, application/xml;q=0.9, application/xhtml+xml, image/png, image/webp, image/jpeg, image/gif, image/x-xbitmap, */*;q=0.1",
            AcceptLanguage =
            "en,ru-RU;q=0.9,ru;q=0.8",
            AcceptCharset = null
        };

        // Firefox 3.6.15
        public static readonly UserAgent Firefox = new UserAgent
        {
            Agent =
            "Mozilla/5.0 (Windows; U; Windows NT 5.1; ru; rv:1.9.2.15) Gecko/20110303 Firefox/3.6.15 (.NET CLR 3.5.30729)",
            Accept =
            "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
            AcceptLanguage =
            "ru-ru,ru;q=0.8,en-us;q=0.5,en;q=0.3",
            AcceptCharset =
            "windows-1251,utf-8;q=0.7,*;q=0.7"
        };

        // Internet Explorer 8
        public static readonly UserAgent InternetExplorer8 = new UserAgent
        {
            Agent =
            "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E)",
            Accept =
            "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, */*",
            AcceptLanguage =
            "ru-RU",
            AcceptCharset = null
        };
    };

    public class ProxyDef
    {
        public string address;
        public string country;

        public static void Apply(ProxyDef that, HttpWebRequest request)
        {
            if (that != null)
                that.Apply(request);
        }

        public void Apply(HttpWebRequest request)
        {
            request.Proxy = new WebProxy(address);
        }
    }

    public class Web
    {
        public ProxyDef Proxy;
        public UserAgent Agent;
        //public CookieContainer Cookies;

        HttpWebRequest CreateWebRequest(string address, string method, string referer, CookieContainer cookies)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(address);
            ProxyDef.Apply(Proxy, request);
            UserAgent.Apply(Agent, request);
            request.CookieContainer = cookies;
            if (referer != null)
                request.Referer = referer;
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            request.Method = method;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return request;
        }

        static string GetResponse(HttpWebRequest request)
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Encoding enc = null;
            string charset = response.CharacterSet;
            if (string.IsNullOrEmpty(charset) || charset.StartsWith("ISO", StringComparison.OrdinalIgnoreCase))
                enc = Encoding.UTF8;
            else
                enc = Encoding.GetEncoding(charset);
            using (Stream answer = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(answer, enc))
                return reader.ReadToEnd();
        }

        public string Get(string address)
        {
            return Get(address, null, null);
        }

        public string Get(string address, CookieContainer cookies)
        {
            return Get(address, null, cookies);
        }
        public string Get(string address, string referer, CookieContainer cookies)
        {
            HttpWebRequest request = CreateWebRequest(address, "GET", referer, cookies);;
            return GetResponse(request);
        }

        public string Post(string address, string contents, string referer)
        {
            return Post(address, contents, referer, null, null);
        }

        public string Post(string address, string contents, string referer, CookieContainer cookies)
        {
            return Post(address, contents, referer, cookies, null);
        }

        public string Post(string address, string contents, string referer, CookieContainer cookies,
            System.Collections.Specialized.NameValueCollection headers)
        {
            HttpWebRequest request = CreateWebRequest(address, "POST", referer, cookies);
            if (headers != null)
                request.Headers.Add(headers);
            byte[] data = Encoding.ASCII.GetBytes(contents);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream())
                stream.Write(data, 0, data.Length);
            return GetResponse(request);
        }
    }
}
