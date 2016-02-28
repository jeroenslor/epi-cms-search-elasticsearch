using EPi.Cms.Search.Elasticsearch.Indexing.TypeMap;
using Nest;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public interface IIndexablePageData : IIndexableTypeMapper
    {
        IPageDataIndexModel CreateIndexModel();
        bool ShouldIndex();
        Id Id { get; }
    }
}