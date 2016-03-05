using System;
using System.Linq;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    [ScheduledPlugIn(DisplayName = "Re-index page tree")]
    public class IndexPageTreeJob : ScheduledJobBase
    {
        private readonly IPageDataIndexer _pageDataIndexer;
        private bool _stopSignaled;

        public IndexPageTreeJob() : this(ServiceLocator.Current.GetInstance<IPageDataIndexer>()) { }

        public IndexPageTreeJob(IPageDataIndexer pageDataIndexer)
        {
            _pageDataIndexer = pageDataIndexer;
            IsStoppable = true; //TODO implement cancellation
        }

        /// <summary>
        /// Called when a user clicks on Stop for a manually started job, or when ASP.NET shuts down.
        /// </summary>
        public override void Stop()
        {
            _stopSignaled = true; //TODO implement cancellation
        }

        /// <summary>
        /// Called when a scheduled job executes
        /// </summary>
        /// <returns>A status message to be stored in the database log and visible from admin mode</returns>
        public override string Execute()
        {
            //Call OnStatusChanged to periodically notify progress of job for manually started jobs
            OnStatusChanged(String.Format("Starting execution of {0}", this.GetType()));

            var bulkIndexResult = _pageDataIndexer.IndexPageTree(false, OnStatusChanged);
            if (bulkIndexResult.Any(x => !x.IsValid || x.Errors))
            {
                throw new AggregateException(
                    bulkIndexResult.Where(x => !x.IsValid || x.Errors).Select(x => new InvalidOperationException(
                        $"Failed to reindex, server code {x.ServerError}, requestInfo {x.DebugInformation}")));
            }

            return "Finished re-indexing of the page tree";
        }
    }
}
