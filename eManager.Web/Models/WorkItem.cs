using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebScraper;

namespace eManager.Web.Models
{
    public class WorkItem
    {
        public string SiteName { get; set; }
        public List<RegexPattern> Regexes = new List<RegexPattern>();
        public ScrapeRequest Request = new ScrapeRequest();
    }
}