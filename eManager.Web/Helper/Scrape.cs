using eManager.Web.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Web;
using WebScraper;

namespace eManager.Web.Helper
{
    public class Scrape
    {
        public List<Dictionary<string, string>> GetScrapeData()
        {
            WebSession ws = new WebSession();
            RegexPattern rp = new RegexPattern();
            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();
            string configPath = HttpContext.Current.Server.MapPath("~/App_Data/Config.json");
            var workItem = JsonConvert.DeserializeObject<WorkItem>(File.ReadAllText(configPath));

            var html = ws.GetWebClientResponse(workItem.Request);
            if (!string.IsNullOrEmpty(html))
                results = rp.ProcessHtml(html, workItem.Regexes);

            return results;
        }
    }
}