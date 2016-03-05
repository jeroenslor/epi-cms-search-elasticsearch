using System;
using System.Globalization;
using System.Linq;
using EPi.Cms.Search.Elasticsearch.Indexing;
using EPi.Cms.Search.Elasticsearch.Indexing.TypeMap;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Moq;
using Nest;
using Xunit;

namespace EPi.Cms.Search.Elasticsearch.UnitTest
{
    public class TypeMapperTests
    {
        [Fact]
        public static void TestPage_Type_Mapping_Should_Create_Index_And_Return_IsValid()
        {            
            var elasticClient = new ElasticClient(new Uri("http://localhost:9200"));
            var helper = new TypeScanIndexableTypeMapperResolver();
            var indexableTypeMappers = helper.GetAll().ToList();

            var typeMapper = indexableTypeMappers.First();
            var typeMapping = typeMapper.CreateTypeMapping(new CultureInfo("en"));
            var typeName = typeMapper.TypeName;

            var mappingsDescriptor = new MappingsDescriptor();
            mappingsDescriptor.Map<IIndexablePageData>(typeName, x => typeMapping);
            var createIndexResponse = elasticClient.CreateIndex("site_en_1", x => x.Mappings(m => mappingsDescriptor));

            Assert.Equal(true, createIndexResponse.IsValid);
            Assert.Equal(true, createIndexResponse.Acknowledged);
        }

        [Fact]
        public static void InitializeIndex_Should_Not_Throw_An_Exception()
        {
            var serviceLocatorMock = new Mock<IServiceLocator>();
            serviceLocatorMock.Setup(x => x.GetInstance<IIndexableTypeMapperResolver>())
                .Returns(new TypeScanIndexableTypeMapperResolver());
            ServiceLocator.SetLocator(serviceLocatorMock.Object);

            var languageBranchRepositoryMock = new Mock<ILanguageBranchRepository>();
            languageBranchRepositoryMock.Setup(x => x.ListEnabled())
                .Returns(new[] {new LanguageBranch(new CultureInfo("en")), new LanguageBranch(new CultureInfo("nl"))});

            var pageDataIndexer = new PageDataIndexer(languageBranchRepositoryMock.Object, new ElasticClient(new Uri("http://localhost:9200")),
                new CmsElasticSearchOptions(), null, null);

            pageDataIndexer.InitializeIndex();
        }

        [Fact]
        public static void IndexPageTree_Should_Not_Throw_An_Exception()
        {
            var serviceLocatorMock = new Mock<IServiceLocator>();
            serviceLocatorMock.Setup(x => x.GetInstance<IIndexableTypeMapperResolver>())
                .Returns(new TypeScanIndexableTypeMapperResolver());
            ServiceLocator.SetLocator(serviceLocatorMock.Object);

            var languageBranchRepositoryMock = new Mock<ILanguageBranchRepository>();
            languageBranchRepositoryMock.Setup(x => x.ListEnabled())
                .Returns(new[] { new LanguageBranch(new CultureInfo("en")), new LanguageBranch(new CultureInfo("nl")) });

            var contentRepositoryMock = new Mock<IContentRepository>();
            contentRepositoryMock.Setup(x => x.GetDescendents(It.IsAny<PageReference>()))
                .Returns(new[] {new ContentReference(1)});
            contentRepositoryMock.Setup(x => x.Get<PageData>(It.IsAny<ContentReference>(), It.IsAny<CultureInfo>()))
                .Returns(new TestPage());

            var pageDataIndexer = new PageDataIndexer(languageBranchRepositoryMock.Object, new ElasticClient(new Uri("http://localhost:9200")),
                new CmsElasticSearchOptions(), contentRepositoryMock.Object, null);

            var indexPageTree = pageDataIndexer.IndexPageTree().ToArray();
        }
    }
}
