using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPi.Cms.Search.Elasticsearch.Indexing;
using EPi.Cms.Search.Elasticsearch.Indexing.TypeMap;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Logging;
using Moq;
using Nest;
using Xunit;

namespace EPi.Cms.Search.Elasticsearch.UnitTest
{
    public class PageDataIndexerTests
    {
        [Fact]
        public static void IndexPageTree_Should_Swap_Correct_Indices()
        {
            var contentRepositoryMock = new Mock<IContentRepository>();
            contentRepositoryMock.Setup(x => x.GetDescendents(ContentReference.RootPage))
                .Returns(new[] { new ContentReference(1)});
            contentRepositoryMock.Setup(x => x.Get<PageData>(new ContentReference(1), new CultureInfo("en")))
                .Returns(new TestPage());

            var elasticClientMock = CreateElasticClientMock();

            var pageDataIndexer = new PageDataIndexer(CreateLanguageBranchRepositoryMock().Object,
                CreateTypeMapperResolverMock().Object,
                elasticClientMock.Object, new CmsElasticSearchOptions(), contentRepositoryMock.Object,
                new Mock<ILogger>().Object);

            var indexPageTree = pageDataIndexer.IndexPageTree();
            indexPageTree.ToList();

            // verify create index is not called when re-creating the re-indexed indice
            elasticClientMock.Verify(
                x =>
                    x.CreateIndex(It.Is<IndexName>(s => s.Name.Equals("site_en_1")),
                        It.IsAny<Func<CreateIndexDescriptor, ICreateIndexRequest>>()), Times.Once);            

            elasticClientMock.Verify(x=>x.Alias(It.Is<Func<BulkAliasDescriptor, IBulkAliasRequest>>(f=> f(new BulkAliasDescriptor()).Actions.Any(a=> a.GetType() == typeof(AliasAddDescriptor)))));

            elasticClientMock.Verify(
                x =>
                    x.Alias(
                        It.Is<Func<BulkAliasDescriptor, IBulkAliasRequest>>(
                            f =>
                                f(new BulkAliasDescriptor())
                                    .Actions.OfType<AliasAddDescriptor>()
                                    .Any(a => ((IAliasAddAction)a).Add.Index.Name.Equals("site_en_1")) &&
                                    f(new BulkAliasDescriptor())
                                    .Actions.OfType<AliasRemoveDescriptor>()
                                    .Any(a => ((IAliasRemoveAction)a).Remove.Index.Name.Equals("site_en_2")))), Times.Once);
        }

        [Fact]
        public static void IndexPageTree_With_BulkSize_2_And_3_Records_Should_BulkIndex_Twice()
        {
            var contentRepositoryMock = new Mock<IContentRepository>();
            contentRepositoryMock.Setup(x => x.GetDescendents(ContentReference.RootPage))
                .Returns(new[] { new ContentReference(1), new ContentReference(1), new ContentReference(1) });
            contentRepositoryMock.Setup(x => x.Get<PageData>(new ContentReference(1), new CultureInfo("en")))
                .Returns(new TestPage());

            var elasticClientMock = CreateElasticClientMock();            

            var pageDataIndexer = new PageDataIndexer(CreateLanguageBranchRepositoryMock().Object,
                CreateTypeMapperResolverMock().Object,
                elasticClientMock.Object, new CmsElasticSearchOptions {BulkSize = 2}, contentRepositoryMock.Object,
                new Mock<ILogger>().Object);

            pageDataIndexer.IndexPageTree().ToList();

            elasticClientMock.Verify(x => x.Bulk(It.IsAny<IBulkRequest>()), Times.Exactly(2));
            elasticClientMock.Verify(x => x.Bulk(It.Is<IBulkRequest>(b => b.Operations.Count == 2)), Times.Exactly(1));
            elasticClientMock.Verify(x => x.Bulk(It.Is<IBulkRequest>(b => b.Operations.Count == 1)), Times.Exactly(1));
        }

        private static Mock<IIndexableTypeMapperResolver> CreateTypeMapperResolverMock()
        {
            var typeMapperResolverMock = new Mock<IIndexableTypeMapperResolver>();
            typeMapperResolverMock.Setup(x => x.GetAll()).Returns(new[] {new TestPage()});
            return typeMapperResolverMock;
        }

        private static Mock<ILanguageBranchRepository> CreateLanguageBranchRepositoryMock()
        {
            var languageBranchRepositoryMock = new Mock<ILanguageBranchRepository>();
            languageBranchRepositoryMock.Setup(x => x.ListEnabled())
                .Returns(new[] {new LanguageBranch(new CultureInfo("en"))});
            return languageBranchRepositoryMock;
        }        

        private static Mock<IElasticClient> CreateElasticClientMock()
        {
            var elasticClientMock = new Mock<IElasticClient>();

            // site indexes exists and alias exists
            elasticClientMock.Setup(
                x => x.IndexExists("site_en_1", It.IsAny<Func<IndexExistsDescriptor, IIndexExistsRequest>>()))
                .Returns(() =>
                {
                    var existsResponseMock = new Mock<IExistsResponse>();
                    existsResponseMock.SetupGet(x => x.IsValid).Returns(true);
                    existsResponseMock.SetupGet(x => x.Exists).Returns(true);
                    return existsResponseMock.Object;
                });
            elasticClientMock.Setup(
                x => x.IndexExists("site_en_2", It.IsAny<Func<IndexExistsDescriptor, IIndexExistsRequest>>()))
                .Returns(() =>
                {
                    var existsResponseMock = new Mock<IExistsResponse>();
                    existsResponseMock.SetupGet(x => x.IsValid).Returns(true);
                    existsResponseMock.SetupGet(x => x.Exists).Returns(true);
                    return existsResponseMock.Object;
                });
            elasticClientMock.Setup(x => x.AliasExists(It.IsAny<Func<AliasExistsDescriptor, IAliasExistsRequest>>()))
                .Returns(
                    () =>
                    {
                        var existsResponseMock = new Mock<IExistsResponse>();
                        existsResponseMock.SetupGet(x => x.IsValid).Returns(true);
                        existsResponseMock.SetupGet(x => x.Exists).Returns(true);
                        return existsResponseMock.Object;
                    });
            elasticClientMock.Setup(x => x.GetAlias(It.IsAny<Func<GetAliasDescriptor, IGetAliasRequest>>())).Returns(
                () =>
                {
                    var getAliasResponseMock = new Mock<IGetAliasesResponse>();
                    getAliasResponseMock.SetupGet(x => x.IsValid).Returns(true);
                    getAliasResponseMock.SetupGet(x => x.Indices)
                        .Returns(new Dictionary<string, IList<AliasDefinition>>
                        {
                            {"site_en_2", new List<AliasDefinition>()}
                        });
                    return getAliasResponseMock.Object;
                });
            elasticClientMock.Setup(
                x => x.CreateIndex(It.IsAny<IndexName>(), It.IsAny<Func<CreateIndexDescriptor, ICreateIndexRequest>>()))
                .Returns(
                    () =>
                    {
                        var createIndexResponseMock = new Mock<ICreateIndexResponse>();
                        createIndexResponseMock.SetupGet(x => x.IsValid).Returns(true);
                        return createIndexResponseMock.Object;
                    });
            elasticClientMock.Setup(x => x.DeleteIndex(It.IsAny<Indices>(), null)).Returns(() =>
            {
                var deleteIndexResponse = new Mock<IDeleteIndexResponse>();
                deleteIndexResponse.SetupGet(x => x.IsValid).Returns(true);
                return deleteIndexResponse.Object;
            });
            elasticClientMock.Setup(x => x.Alias(It.IsAny<Func<BulkAliasDescriptor, IBulkAliasRequest>>())).Returns(
                () =>
                {
                    var bulkAliasResponseMock = new Mock<IBulkAliasResponse>();
                    bulkAliasResponseMock.SetupGet(x => x.IsValid).Returns(true);
                    bulkAliasResponseMock.SetupGet(x => x.Acknowledged).Returns(true);
                    return bulkAliasResponseMock.Object;
                });
            elasticClientMock.Setup(x => x.Bulk(It.IsAny<IBulkRequest>())).Returns(
                () =>
                {
                    var bulkResponbseMock = new Mock<IBulkResponse>();
                    bulkResponbseMock.SetupGet(x => x.IsValid).Returns(true);
                    return bulkResponbseMock.Object;
                });

            return elasticClientMock;
        }
    }
}
