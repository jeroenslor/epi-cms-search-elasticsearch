using System;
using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.DataAbstraction;
using Moq;
using Nest;
using Xunit;

namespace EPi.Cms.Search.Elasticsearch.UnitTest
{
    public class PageDataIndexerTests
    {
        [Fact]
        public static void IndexPageTree_Should_Create_New_Index1_With_Name_Site_en_1()
        {
            var languageBranchRepositoryMock = new Mock<ILanguageBranchRepository>();
            languageBranchRepositoryMock.Setup(x => x.ListEnabled())
                .Returns(new[] {new LanguageBranch(new CultureInfo("en"))});
            var elasticClientMock = new Mock<IElasticClient>();
            elasticClientMock.Setup(
                x => x.IndexExists("site_en_1", It.IsAny<Func<IndexExistsDescriptor, IIndexExistsRequest>>()))
                .Returns(() =>
                {
                    var existsResponseMock = new Mock<IExistsResponse>();
                    existsResponseMock.SetupGet(x => x.IsValid).Returns(true);
                    existsResponseMock.SetupGet(x => x.Exists).Returns(false);
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
            elasticClientMock.Setup(
                x => x.CreateIndex(It.IsAny<string>(), It.IsAny<Func<CreateIndexDescriptor, ICreateIndexRequest>>()))
                .Returns(
                    () =>
                    {
                        var createIndexResponseMock = new Mock<ICreateIndexResponse>();
                        createIndexResponseMock.SetupGet(x => x.IsValid).Returns(true);
                        return createIndexResponseMock.Object;
                    });
            elasticClientMock.Setup(x => x.Alias(It.IsAny<Func<BulkAliasDescriptor, IBulkAliasRequest>>())).Returns(
                () =>
                {
                    var bulkAliasResponseMock = new Mock<IBulkAliasResponse>();
                    bulkAliasResponseMock.SetupGet(x => x.IsValid).Returns(true);
                    return bulkAliasResponseMock.Object;
                });
            var contentRepositoryMock = new Mock<IContentRepository>();

            // verify create index call
            elasticClientMock.Verify(
                x =>
                    x.CreateIndex(It.Is<string>(indexName => indexName.Equals("site_en_1")),
                        It.IsAny<Func<CreateIndexDescriptor, ICreateIndexRequest>>()), Times.Once);

            // verify create alias call
            elasticClientMock.Verify(
                x =>
                    x.Alias(
                        It.Is<Func<BulkAliasDescriptor, IBulkAliasRequest>>(
                            f =>
                                f(new BulkAliasDescriptor())
                                    .Actions.OfType<AliasAddAction>()
                                    .Any(a => a.Add.Alias.Equals("site_en")) && 
                                    f(new BulkAliasDescriptor())
                                    .Actions.OfType<AliasAddAction>()
                                    .Any(a => a.Add.Index.Name.Equals("site_en_1")))), Times.Once);
        }
    }
}
