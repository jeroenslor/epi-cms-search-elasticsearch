using System;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public static class IndexablePageDataExtensions
    {
        private static Injected<SiteDefinitionResolver> SiteDefinitionResolver { get; } 

        public static void SetIndexablePageDataProperties(this IIndexablePageData indexablePageData, IPageDataIndexModel indexModel)
        {
            var pageData = indexablePageData as PageData;
            if (pageData == null)
                throw new ArgumentException("Should inherit from PageData", nameof(indexablePageData));

            indexModel.Id = pageData.ContentGuid;
            indexModel.ContentReference = pageData.ContentLink.ToString();

            var siteDefinition = SiteDefinitionResolver.Service.GetDefinitionForContent(pageData.ContentLink, false, false);
            indexModel.SiteDefinitionId = siteDefinition?.Id;
        }
    }
}
