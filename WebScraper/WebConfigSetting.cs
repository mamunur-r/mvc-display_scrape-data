using System.Collections.Generic;
using System.Net;

namespace WebScraper
{
    public class WebConfigSetting
    {
        public string Host { get; set; }
        public string Accept { get; set; }
        public string Referer { get; set; }
        public string UserAgent { get; set; }
        /// <summary>
        /// Request timeout period in ms
        /// Default is 90000
        /// </summary>
        public int TimeOut = 90000;
        public Dictionary<string, string> Headers;
        public Dictionary<string, string> PostParams = new Dictionary<string, string>();
        public CookieCollection Cookies;
        public string CustomCookies { get; set; }
    }
}
