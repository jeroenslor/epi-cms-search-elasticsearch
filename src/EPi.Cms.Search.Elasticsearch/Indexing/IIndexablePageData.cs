using System.Globalization;
using EPi.Cms.Search.Elasticsearch.Indexing.TypeMap;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public interface IIndexablePageData : IIndexableTypeMapper
    {
        IPageDataIndexModel CreateIndexModel(CultureInfo cultureInfo);
        bool ShouldIndex(CultureInfo cultureInfo);
    }
}