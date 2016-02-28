using System;
using System.Collections.Generic;
using Nest;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public interface IPageDataIndexer
    {
        /// <summary>
        /// (Re)Indexes the complete page tree for pagedata that inherits from <see cref="IIndexablePageData"/>
        /// </summary>
        /// <param name="swapWithErrors">if set to <c>true</c> and if an error occured during the index process the new index is swapped to live</param>
        /// <param name="onStatusChanges">the action that is executed on status changed during the index process</param>
        /// <returns></returns>
        IEnumerable<IBulkResponse> IndexPageTree(bool swapWithErrors = false, Action<string> onStatusChanges = null);

        IIndexResponse Index(IIndexablePageData indexablePageData);
        IDeleteResponse Delete(IIndexablePageData indexablePageData);
    }
}