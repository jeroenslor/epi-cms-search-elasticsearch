namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public interface IIndexablePageData
    {
        IPageDataIndexModel CreateIndexModel();
        bool ShouldIndex();
    }
}