using System;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class PageDataIndexerInitializationModule : IInitializableModule
    {
        private IPageDataIndexer _pageDataIndexer;

        public void Initialize(InitializationEngine context)
        {
            _pageDataIndexer = context.Locate.Advanced.GetInstance<IPageDataIndexer>();

            // make sure the indexes are correctly configured
            _pageDataIndexer.InitializeIndex();

            var options = context.Locate.Advanced.GetInstance<CmsElasticSearchOptions>();
            if (!options.EnableContentEvents)
                return;            

            // register content events
            var contentEvents = context.Locate.ContentEvents();
            contentEvents.SavingContent += ContentEventsOnSavingContent;
            contentEvents.DeletingContent += ContentEventsOnDeletingContent;
        }

        private void ContentEventsOnDeletingContent(object sender, DeleteContentEventArgs deleteContentEventArgs)
        {
            var indexablePageData = deleteContentEventArgs.Content as IIndexablePageData;
            if (indexablePageData == null)
                return;

            var deleteResponse = _pageDataIndexer.Delete(indexablePageData);
            if (!deleteResponse.IsValid)
                throw new InvalidOperationException(
                    $"Failed to delete pagedata server error {deleteResponse.ServerError}, requestInfo {deleteResponse.DebugInformation}");
        }

        private void ContentEventsOnSavingContent(object sender, ContentEventArgs contentEventArgs)
        {
            var indexablePageData = contentEventArgs.Content as IIndexablePageData;
            if (indexablePageData == null)
                return;

            if (!indexablePageData.ShouldIndex())
                return;

            var indexResponse = _pageDataIndexer.Index(indexablePageData);
            if (!indexResponse.IsValid)
                throw new InvalidOperationException(
                    $"Failed to index pagedata server error {indexResponse.ServerError}, requestInfo {indexResponse.DebugInformation}");
        }

        public void Uninitialize(InitializationEngine context)
        {
            var contentEvents = context.Locate.ContentEvents();
            contentEvents.SavingContent -= ContentEventsOnSavingContent;
            contentEvents.DeletedContent -= ContentEventsOnDeletingContent;
        }
    }
}