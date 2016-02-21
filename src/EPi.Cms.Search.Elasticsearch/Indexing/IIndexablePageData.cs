using System.Globalization;
using EPi.Cms.Search.Elasticsearch.Indexing.TypeMap;
using EPiServer.Core;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public interface IIndexablePageData : IContent, IIndexableTypeMapper
    {
        IPageDataIndexModel CreateIndexModel(CultureInfo cultureInfo);
        bool ShouldIndex(CultureInfo cultureInfo);
    }
}