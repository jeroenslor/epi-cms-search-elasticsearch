using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public static class IndexablePageDataExtensions
    {
        private static Injected<SiteDefinitionResolver> SiteDefinitionResolver { get; } 

        public static void SetBaseProperties(this IIndexablePageData indexablePageDate, IPageDataIndexModel indexModel)
        {
            indexModel.Id = indexablePageDate.ContentGuid;
            indexModel.ContentReference = indexablePageDate.ContentLink.ToString();

            var siteDefinition = SiteDefinitionResolver.Service.GetDefinitionForContent(indexablePageDate.ContentLink, false, false);
            indexModel.SiteDefinitionId = siteDefinition?.Id;
        }
    }
}
