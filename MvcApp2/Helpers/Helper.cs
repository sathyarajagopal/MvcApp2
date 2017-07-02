using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Xml.Linq;

namespace MvcApp2.Helpers
{
    public static class JavascriptExtension
    {
        private static readonly StringDictionary allowedFileTypes = new StringDictionary() { { "js", "application/javascript" } };
        public static MvcHtmlString IncludeVersionedJs(this HtmlHelper helper, string relativeFilePathQuery)
        {
            string actualPathToLoad = string.Empty;
            string version = GetVersion(helper, relativeFilePathQuery, out actualPathToLoad);
            return MvcHtmlString.Create("<script type='text/javascript' src='" + actualPathToLoad + version + "'></script>");
        }

        private static string GetVersion(this HtmlHelper helper, string relativeFilePathQuery, out string actualPathToLoad)
        {
            actualPathToLoad = relativeFilePathQuery;
            var context = helper.ViewContext.RequestContext.HttpContext;
            string relativeFilePath = relativeFilePathQuery.Split('?')[0];
            int idx = relativeFilePathQuery.IndexOf('?');
            string query = idx >= 0 ? relativeFilePathQuery.Substring(idx) : "";
            string referringSourceApp = string.IsNullOrWhiteSpace(HttpUtility.ParseQueryString(query).Get("s")) ? "en" : HttpUtility.ParseQueryString(query).Get("s");
            List<FileReferenceEntity> staticFiles = FileReferenceEntity.GetFileReferences();

            foreach (FileReferenceEntity file in staticFiles)
            {
                if (string.Equals(file.Key, referringSourceApp, StringComparison.OrdinalIgnoreCase))
                {
                    actualPathToLoad = file.Value + "?s=" + referringSourceApp;
                    break;
                }
            }

            if (context.Cache[relativeFilePathQuery] == null)
            {
                var physicalPath = context.Server.MapPath(relativeFilePath);
                var version = $"&v={new System.IO.FileInfo(physicalPath).LastWriteTime.ToString("yyyyMMddHHmmss")}";
                string dependencyFile1 = physicalPath;
                string dependencyFile2 = @"C:\inetpub\wwwroot\Publish\MvcApp2\FileReferenceMapper.config";
                string[] dependencies = new string[] { dependencyFile1, dependencyFile2 };
                context.Cache.Insert(relativeFilePathQuery, version, new CacheDependency(dependencies));
                return version;
            }
            else
            {
                return context.Cache[relativeFilePathQuery] as string;
            }
        }
    }
    public class FileReferenceEntity
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public static List<FileReferenceEntity> GetFileReferences()
        {
            XDocument xDoc = XDocument.Load(@"C:\inetpub\wwwroot\Publish\MvcApp2\FileReferenceMapper.config");
            List<FileReferenceEntity> items = (from element in xDoc.Descendants("configuration").Descendants("appSettings").Elements("add")
                                               select new FileReferenceEntity
                                               {
                                                   Key = element.Attribute("key").Value,
                                                   Value = element.Attribute("value").Value
                                               }).ToList();
            return items;
        }
    }
}