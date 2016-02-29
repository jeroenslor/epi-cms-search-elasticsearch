using EPiServer.ServiceLocation;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    [ServiceConfiguration(typeof(CmsElasticSearchOptions), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CmsElasticSearchOptions
    {
        public string IndexName { get; set; } = "site";
        public int BulkSize { get; set; } = 100;
        public bool EnableContentEvents { get; set; } = true;
    }
}