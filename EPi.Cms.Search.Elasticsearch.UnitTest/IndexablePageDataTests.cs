using System;
using System.Globalization;
using EPi.Cms.Search.Elasticsearch.Indexing;
using EPiServer.Core;
using Nest;
using Xunit;

namespace EPi.Cms.Search.Elasticsearch.UnitTest
{
    public class IndexablePageDataTests
    {
        [Fact]
        public static void CreateIndexModel_Should_Return_TestPageIndexModel()
        {
            var testPage = new TestPage();
            var indexModel = testPage.CreateIndexModel(new CultureInfo("en"));

            Assert.Equal(typeof(TestPageIndexModel), indexModel.GetType());
        }

        public static void ShouldIndex_Should_Return_True()
        {
            var testPage = new TestPage();
            var shouldIndex = testPage.ShouldIndex(new CultureInfo("en"));

            Assert.Equal(true, shouldIndex);
        }

        [Fact]
        public static void CreateTypeMapping_Should_Return_A_Valid_PutMappingRequest()
        {
            var testPage = new TestPage();
            var typeMapping = testPage.CreateTypeMapping(new CultureInfo("en"));

            Assert.NotNull(typeMapping);
            Assert.IsType<TypeMappingDescriptor<TestPageIndexModel>>(typeMapping);
        }
    }

    public class TestPage : PageData, IIndexablePageData
    {
        public IPageDataIndexModel CreateIndexModel(CultureInfo cultureInfo)
        {
            return new TestPageIndexModel();
        }

        public bool ShouldIndex(CultureInfo cultureInfo)
        {
            return true;
        }

        public string TypeName => "test_page";

        public ITypeMapping CreateTypeMapping(CultureInfo cultureInfo)
        {
            return new TypeMappingDescriptor<TestPageIndexModel>().AutoMap();
        }
    }

    public class TestPageIndexModel : IPageDataIndexModel
    {
        public Guid Id { get; set; }
        public ContentReference ContentReference { get; set; }
    }

}
