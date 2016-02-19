using System;
using System.Collections.Generic;
using System.Globalization;
using EPi.Cms.Search.Elasticsearch.Indexing.TypeMap;
using EPiServer.DataAbstraction;
using Nest;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public class PageDataIndexer
    {
        private readonly ILanguageBranchRepository _languageBranchRepository;
        private readonly IIndexableTypeMapperHelper _typeMapperHelper;
        private readonly IElasticClient _elasticClient;
        private static string _indexName = "site";

        public PageDataIndexer(ILanguageBranchRepository languageBranchRepository, IIndexableTypeMapperHelper typeMapperHelper, IElasticClient elasticClient)
        {
            _languageBranchRepository = languageBranchRepository;
            _typeMapperHelper = typeMapperHelper;
            _elasticClient = elasticClient;
        }

        /// <summary>
        /// (Re)Indexes the complete page tree for pagedata that inherits from <see cref="IIndexablePageData"/>
        /// </summary>
        /// <param name="swapWithErrors">if set to <c>true</c> and if an error occured during the index process the new index is swapped to live</param>
        /// <param name="onStatusChanges">the action that is executed on status changed during the index process</param>
        /// <returns></returns>
        IEnumerable<IBulkResponse> IndexPageTree(bool swapWithErrors = false, Action<string> onStatusChanges = null)
        {
            var mappers = _typeMapperHelper.GetAll();
            foreach (var languageBranch in _languageBranchRepository.ListEnabled())
            {
                var language = languageBranch.Culture;
                var index = GetIndex(language);
            }

            throw new NotImplementedException();
        }

        protected virtual void CreateIndex(string name)
        {
            var createIndexResponse = _elasticClient.CreateIndex(name);
            if (!createIndexResponse.IsValid)
                throw new InvalidOperationException(
                    $"Unable to create index with name: {name}, server error: {createIndexResponse.ServerError}, debug information: {createIndexResponse.DebugInformation}");
        }

        protected virtual string GetIndex(CultureInfo language)
        {
            return $"{_indexName}_{language.Name.ToLower()}";
        }
    }
}
