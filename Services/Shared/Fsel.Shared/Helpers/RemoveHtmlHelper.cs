// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Text.RegularExpressions;

    public static class RemoveHtmlHelper
    {
        private static Regex htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

        public static string RemoveHTMLTagsCompiled(string html)
        {
            return htmlRegex.Replace(html, string.Empty);
        }

        public static string RemoveHTMLTags(string html)
        {
            return Regex.Replace(html, "<.*?>", string.Empty);
        }

      /*  public static string StripHtmlTagsUsingHtmlAgilityPack(string htmlString)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(htmlString);
            return document.DocumentNode.InnerText;
        }*/
    }
}
