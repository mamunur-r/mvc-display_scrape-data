
using eManager.Web.Helper;
using System.Collections.Generic;
using System.Web.Mvc;

namespace eManager.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ScrapeData()
        {
            ViewBag.Message = "Here are the scrape data";

            Scrape scrape = new Scrape();
            var scrapeResults =  scrape.GetScrapeData();
            //var scrapeResults = new List<Dictionary<string, string>>();
            //scrapeResults.Add(new Dictionary<string, string>() {
            //    { "Key1", "Value1" },
            //    { "Key2", "Value2" },
            //    { "Key3", "Value3" }
            //});
            //scrapeResults.Add(new Dictionary<string, string>() {
            //    { "Key1", "Value4" },
            //    { "Key2", "Value5" },
            //    { "Key3", "Value6" }
            //});

            return View(scrapeResults);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}