using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;


namespace WebScraper
{
    public class RegexPattern
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int ParentId { get; set; }

        public RegexType Type { get; set; }

        public Regex RegularExpression { get; set; }

        public List<Dictionary<string, string>> ProcessHtml(string html, List<RegexPattern> regexPatterns)
        {
            var results = new List<Dictionary<string, string>>();

            RegexInfo pageTypeMatch = GetPageType(html, regexPatterns, 0);
            if(pageTypeMatch== null) return null;

            // can use reflection 
            if (pageTypeMatch.RegexType.Equals(RegexType.List))
            {
                var selectionRegexs = regexPatterns.Where(r => r.Type.Equals(RegexType.Selection) && r.ParentId.Equals(pageTypeMatch.RegexId)).ToList();
                var selections = GetMatchedResults(html, selectionRegexs);

                foreach (var s in selections)
                {
                    var rows = ListPageProcessor(s.Value, regexPatterns, pageTypeMatch);
                    results.AddRange(rows);
                }
            }
            return results;
        }

        public List<Dictionary<string, string>> ListPageProcessor(string html, List<RegexPattern> regexPatterns, RegexInfo pageTypeMatch)
        {
            var results = new List<Dictionary<string, string>>();
            var globalResults = new List<KeyValuePair<string, string>>();

            var globalRegexes = regexPatterns.Where(r => r.Type.Equals(RegexType.Global) && r.ParentId.Equals(pageTypeMatch.RegexId)).ToList();
            if (globalRegexes != null && globalRegexes.Count > 0)
                globalResults = GetMatchedResults(html, globalRegexes);

            var regexes = regexPatterns.Where(r => r.ParentId.Equals(pageTypeMatch.RegexId) && r.Type.Equals(RegexType.Item)).ToList();
            if (regexes == null) return null;

            var items = GetMatchedResults(html, regexes);

            foreach (var regex in regexes)
            {                
                var detailRegexes = regexPatterns.Where(r => r.Type.Equals(RegexType.Detail) && r.ParentId.Equals(regex.Id)).ToList();

                foreach (var item in items)
                {
                    var itemDetails = GetMatchedResults(item.Value, detailRegexes);
                    if (itemDetails != null && itemDetails.Count > 0)
                    {
                        if (globalResults.Count > 0) itemDetails.AddRange(globalResults);
                        results.Add(itemDetails.GroupBy(f => f.Key).Select(g => g.First()).ToList().ToDictionary(x => x.Key, x => x.Value));
                    }
                }
            }
            return results;
        }

        public List<KeyValuePair<string, string>> GetMatchedResults(string html, List<RegexPattern> regexPatterns)
        {
            List<KeyValuePair<string, string>> results = new List<KeyValuePair<string, string>>();

            foreach (var regex in regexPatterns)
            {
                string groupName = string.Empty;
                try
                {
                    groupName = regex.RegularExpression.GetGroupNames().ToList().Where(gn => Regex.Match(gn, "\\d+").Success != true).FirstOrDefault();

                    foreach (Match m in regex.RegularExpression.Matches(html))
                        results.Add(new KeyValuePair<string, string>(groupName, m.Groups[groupName].Value));
                }
                catch (Exception)
                {
                }
            }

            return results;
        }



        /// <summary>
        /// returns regex type matched information
        /// </summary>
        /// <param name="html"></param>
        /// <param name="regexPatterns"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public RegexInfo GetPageType(string html, List<RegexPattern> regexPatterns, int parentId)
        {
            var regexes = regexPatterns.Where(r => r.ParentId.Equals(parentId)).ToList();

            foreach (var r in regexes)
                if (r.RegularExpression.Match(html).Success)
                    return new RegexInfo { RegexType = r.Type, ParentId = r.ParentId, RegexId = r.Id };

            return null;
        }

        public Regex GetRegex(string pattern)
        {
            return new Regex(WebUtility.HtmlDecode(pattern), RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }

    public class RegexInfo
    {
        public RegexType RegexType { get; set; }
        public int RegexId { get; set; }
        public int ParentId { get; set; }
    }

    public enum RegexType
    {
        List,
        Selection,
        Item,
        Global,
        Detail,
        None
    }
}
