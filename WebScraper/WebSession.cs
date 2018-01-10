using System;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web;

namespace WebScraper
{
    public class WebSession
    {
        private CookieContainer _cookies = new CookieContainer();
        private string _userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";

        public WebSession()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            //ServicePointManager.Expect100Continue = false;
        }

        public string GetWebClientResponse(ScrapeRequest Request)
        {
            string result = string.Empty;
            NameValueCollection postContent = new NameValueCollection();            

            try
            {
                if (Request.WebConfig.PostParams != null && Request.WebConfig.PostParams.Count > 0)
                    foreach (var param in Request.WebConfig.PostParams)
                        postContent.Add(param.Key, HttpUtility.UrlEncode(param.Value));

                using (WebClient client = new WebClient())
                {
                    if (Request.WebConfig.Headers != null)
                        foreach (var h in Request.WebConfig.Headers)
                            client.Headers.Add(h.Key, h.Value);

                    if (!string.IsNullOrEmpty(Request.WebConfig.CustomCookies))
                        client.Headers.Add(HttpRequestHeader.Cookie, Request.WebConfig.CustomCookies);

                    if (Request.RequestType == "POST")
                    {
                        byte[] response = client.UploadValues(Request.Url, Request.RequestType, postContent);
                        result = Encoding.UTF8.GetString(response);
                    }
                    else
                    {
                        result = client.DownloadString(Request.Url);                        
                        result = Encoding.UTF8.GetString(Encoding.Default.GetBytes(result));
                    }

                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        public string GetWebRequestResponse(ScrapeRequest scrapeRequest)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(scrapeRequest.Url);

            SetRequestCookies(request, scrapeRequest);

            SetRequestHeaders(request, scrapeRequest);

            SetRequestMeta(request, scrapeRequest);

            if (scrapeRequest.RequestType.ToUpper() == "POST")
            {
                //scrapeRequest.PostContent = HttpUtility.UrlEncode(scrapeRequest.PostContent);
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] contentData = Encoding.ASCII.GetBytes(scrapeRequest.PostContent);
                request.ContentLength = contentData.Length;
                Stream contentWriter = request.GetRequestStream();
                contentWriter.Write(contentData, 0, contentData.Length);
                contentWriter.Close();
            }
            else
                request.ContentType = "text/html, charset=utf-8"; //application/json; charset=utf-8 

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response == null) return null;

            return readResponseStream(response);
        }

        private string readResponseStream(HttpWebResponse response)
        {
            Stream responseStream = null;
            StreamReader reader = null;
            try
            {
                responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    responseStream.ReadTimeout = 15000;

                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                        responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
                    reader = new StreamReader(responseStream);
                }

                if (reader != null) return reader.ReadToEnd();
            }
            finally
            {
                if (reader != null) reader.Close();
                if (responseStream != null) responseStream.Close();
            }
            return null;
        }

        void SetRequestHeaders(HttpWebRequest request, ScrapeRequest sr)
        {
            if (sr.WebConfig.Headers != null && sr.WebConfig.Headers.Count > 0)
            {
                WebHeaderCollection whc = request.Headers;
                foreach (var h in sr.WebConfig.Headers)
                {
                    if (WebHeaderCollection.IsRestricted(h.Key)) continue;
                    else if (!request.Headers.AllKeys.Equals(h.Key))
                        request.Headers.Add(h.Key, h.Value);
                    else
                        request.Headers[h.Key] = h.Value;
                }
            }
            else
            {
                request.Headers.Add("Accept-Language", "en-US, en;q=0.8");
                request.Headers.Add("Cache-Control", "no-chche");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
            }

        }

        void SetRequestCookies(HttpWebRequest request, ScrapeRequest sr)
        {
            request.CookieContainer = _cookies;

            if (sr.WebConfig.Cookies != null && sr.WebConfig.Cookies.Count > 0)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(sr.WebConfig.Cookies);
                _cookies = request.CookieContainer;
            }

        }

        void SetRequestMeta(HttpWebRequest request, ScrapeRequest sr)
        {
            request.Timeout = (sr.WebConfig.TimeOut != 0) ? sr.WebConfig.TimeOut : 90000;
            request.Method = sr.RequestType;
            if (!string.IsNullOrEmpty(sr.WebConfig.Accept))
                request.Accept = sr.WebConfig.Accept;

            request.UserAgent = (!string.IsNullOrEmpty(sr.WebConfig.UserAgent)) ? sr.WebConfig.UserAgent : _userAgent;
        }
    }
}
