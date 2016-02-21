using EPiServer.ServiceLocation;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    [ServiceConfiguration(typeof(ICmsElasticSearchOptions), Lifecycle = ServiceInstanceScope.Singleton)]
    public class DefaultCmsElasticSearchOptions : ICmsElasticSearchOptions
    {
        public string IndexName => "site";
        public int BulkSize => 100;
    }
}