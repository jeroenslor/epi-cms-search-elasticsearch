﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPi.Cms.Search.Elasticsearch.Indexing.TypeMap;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Nest;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    [ServiceConfiguration(typeof(IPageDataIndexer), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PageDataIndexer : IPageDataIndexer
    {
        protected readonly ILanguageBranchRepository LanguageBranchRepository;
        protected readonly IElasticClient ElasticClient;
        protected readonly CmsElasticSearchOptions Options;
        private readonly IContentRepository _contentRepository;
        private readonly IIndexableTypeMapperResolver _indexableTypeMapperResolver;
        private readonly ILogger _logger;

        public PageDataIndexer(ILanguageBranchRepository languageBranchRepository,IElasticClient elasticClient,
            CmsElasticSearchOptions options, IContentRepository contentRepository, IIndexableTypeMapperResolver indexableTypeMapperResolver, ILogger logger)
        {
            LanguageBranchRepository = languageBranchRepository;
            ElasticClient = elasticClient;
            Options = options;
            _contentRepository = contentRepository;
            _indexableTypeMapperResolver = indexableTypeMapperResolver;
            _logger = logger;            
        }

        /// <summary>
        /// (Re)Indexes the complete page tree for pagedata that inherits from <see cref="IIndexablePageData"/>
        /// </summary>
        /// <param name="swapWithErrors">if set to <c>true</c> and if an error occured during the index process the new index is swapped to live</param>
        /// <param name="onStatusChanges">the action that is executed on status changed during the index process</param>
        /// <returns></returns>
        public IEnumerable<IBulkResponse> IndexPageTree(bool swapWithErrors = false, Action<string> onStatusChanges = null)
        {
            foreach (var languageBranch in LanguageBranchRepository.ListEnabled())
            {
                var language = languageBranch.Culture;
                InitializeIndex(language);

                var aliasName = GetAliasName(language);
                var indexName1 = GetIndexName(language, 1);
                var indexName2 = GetIndexName(language, 2);

                var aliasIndices = ElasticClient.GetAlias(x => x.Name(aliasName)).Indices;
                var liveIndexName = aliasIndices.First().Key;
                var reIndexName = liveIndexName.Equals(indexName1) ? indexName2 : indexName1;

                // clear the reIndex indice since we are re-building it
                Clear(reIndexName, language);

                var indexablePages = GetIndexablePages(language).ToArray();
                var bulkOperations = new List<IBulkOperation>();
                bool hasErrors = false;

                for (var i = 0; i < indexablePages.Length; i++)
                {
                    var indexablePageData = indexablePages[i];
                    if (!indexablePageData.ShouldIndex())
                        continue;

                    var indexModel = CreateIndexModel(indexablePageData);

                    bulkOperations.Add(CreateBulkOperation(indexModel));

                    if (bulkOperations.Count == Options.BulkSize || i == indexablePages.Length - 1)
                    {
                        var bulkRequest = new BulkRequest(reIndexName) {Operations = new List<IBulkOperation>(bulkOperations)};

                        var bulkResponse = ElasticClient.Bulk(bulkRequest);
                        if (!bulkResponse.IsValid || bulkResponse.Errors)
                            hasErrors = true;

                        bulkOperations.Clear();

                        yield return bulkResponse;
                    }

                    onStatusChanges?.Invoke($"Insterted bulk into {reIndexName} currentIndex: {i}");
                }

                if (!hasErrors || swapWithErrors)
                    SwapIndex(liveIndexName, aliasName, reIndexName);
            }
        }

        public IIndexResponse Index(IIndexablePageData indexablePageData)
        {
            var pageData = indexablePageData as PageData;
            if (pageData == null)
                throw new ArgumentException("Should inherit from PageData", nameof(indexablePageData));

            var indexModel = CreateIndexModel(indexablePageData);

            return ElasticClient.Index(indexModel, x => x.Index(GetAliasName(pageData.Language)));
        }

        public IDeleteResponse Delete(IIndexablePageData indexablePageData)
        {
            var pageData = indexablePageData as PageData;
            if (pageData == null)
                throw new ArgumentException("Should inherit from PageData", nameof(indexablePageData));

            return
                ElasticClient.Delete(new DeleteRequest(GetAliasName(pageData.Language),
                    TypeName.Create(indexablePageData.GetType()), pageData.ContentGuid));
        }

        public void InitializeIndex()
        {
            foreach (var languageBranch in LanguageBranchRepository.ListEnabled())
            {
                var language = languageBranch.Culture;
                InitializeIndex(language);
            }
        }

        private void InitializeIndex(CultureInfo language)
        {
            var aliasName = GetAliasName(language);

            // create 2 indexes one for live and one for re-indexing
            var indexName1 = GetIndexName(language, 1);
            var indexName2 = GetIndexName(language, 2);

            if (!ElasticClient.IndexExists(indexName1).Exists)
                CreateIndex(indexName1, language);

            if (!ElasticClient.IndexExists(indexName2).Exists)
                CreateIndex(indexName2, language);

            if (!ElasticClient.AliasExists(x => x.Name(aliasName)).Exists)
                CreateAlias(aliasName, indexName1);
        }

        private IPageDataIndexModel CreateIndexModel(IIndexablePageData indexablePageData)
        {
            IPageDataIndexModel indexModel;
            try
            {
                indexModel = indexablePageData.CreateIndexModel();
            }
            catch (Exception)
            {
                _logger.Error(
                    $"Failed to create index model for page with name: {((PageData)indexablePageData).Name} and contentlink id: {((PageData)indexablePageData).ContentLink.ID}");
                throw;
            }
            return indexModel;
        }               

        protected virtual IEnumerable<IIndexablePageData> GetIndexablePages(CultureInfo language)
        {
            foreach (var pageReference in _contentRepository.GetDescendents(ContentReference.RootPage))
            {
                var indexablePageData = _contentRepository.Get<IContent>(pageReference, language) as IIndexablePageData;
                if (indexablePageData == null)
                    continue;

                yield return indexablePageData;
            }
        }                

        protected virtual MappingsDescriptor CreateMappingsDescriptor(CultureInfo language)
        {
            var mappingsDescriptor = new MappingsDescriptor();

            foreach (var indexableTypeMapper in _indexableTypeMapperResolver.GetAll())
            {
                var typeMapping = indexableTypeMapper.CreateTypeMapping(language);
                mappingsDescriptor.Map<IIndexablePageData>(indexableTypeMapper.TypeName, x => typeMapping);
            }

            return mappingsDescriptor;
        }        

        protected virtual void DeleteIndex(string name)
        {
            var deleteIndexResponse = ElasticClient.DeleteIndex(name);
            if (!deleteIndexResponse.IsValid)
                throw new InvalidOperationException(
                    $"Unable to delete index with name: {name}, server error: {deleteIndexResponse.ServerError}, debug information: {deleteIndexResponse.DebugInformation}");
        }

        protected virtual void CreateIndex(string name, CultureInfo language)
        {
            var createIndexResponse = ElasticClient.CreateIndex(name, x => x.Mappings(m => CreateMappingsDescriptor(language)));
            if (!createIndexResponse.IsValid)
                throw new InvalidOperationException(
                    $"Unable to create index with name: {name}, server error: {createIndexResponse.ServerError}, debug information: {createIndexResponse.DebugInformation}");
        }

        protected virtual string GetAliasName(CultureInfo language)
        {
            return $"{Options.IndexName}_{language.Name.ToLower()}";
        }

        protected virtual string GetIndexName(CultureInfo language, int slotNumber)
        {
            return $"{GetAliasName(language)}_{slotNumber}";
        }        

        private void SwapIndex(string liveIndexName, string alias, string reIndexName)
        {
            var aliasResult =
                ElasticClient.Alias(
                    x =>
                        x.Remove(a => a.Index(liveIndexName).Alias(alias))
                            .Add(a => a.Index(reIndexName).Alias(alias)));
            if (!aliasResult.IsValid || !aliasResult.Acknowledged)
                throw new InvalidOperationException(
                    $"Unable to swap index alias with name {alias}, from {liveIndexName}, to {reIndexName}  server error: {aliasResult.ServerError}, debug information: {aliasResult.DebugInformation}");
        }

        private void CreateAlias(string alias, string indexName1)
        {
            var createAliasResponse = ElasticClient.Alias(x => x.Add(a => a.Alias(alias).Index(indexName1)));
            if (!createAliasResponse.IsValid)
                throw new InvalidOperationException(
                    $"Unable to create alias with name: {alias}, server error: {createAliasResponse.ServerError}, request information: {createAliasResponse.DebugInformation}");
        }

        private void Clear(string name, CultureInfo language)
        {
            DeleteIndex(name);
            CreateIndex(name, language);
        }

        private static BulkCreateOperation<IPageDataIndexModel> CreateBulkOperation(IPageDataIndexModel indexModel)
        {
            var bulkCreateOperation = new BulkCreateOperation<IPageDataIndexModel>(indexModel)
            {
                Type = TypeName.Create(indexModel.GetType())
            };

            return bulkCreateOperation;
        }        
    }
}
