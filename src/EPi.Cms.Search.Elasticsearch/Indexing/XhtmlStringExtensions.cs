using EPiServer.Core;
using EPiServer.Core.Html;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public static class XhtmlStringExtensions
    {
        public static string StripHtml(this XhtmlString xhtmlString)
        {
            return TextIndexer.StripHtml(xhtmlString.ToHtmlString(), 0);
        }
    }
}
