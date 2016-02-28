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
            var indexModel = testPage.CreateIndexModel();

            Assert.Equal(typeof(TestPageIndexModel), indexModel.GetType());
        }

        public static void ShouldIndex_Should_Return_True()
        {
            var testPage = new TestPage();
            var shouldIndex = testPage.ShouldIndex();

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
        public virtual ContentArea TestContentArea { get; set; }
                
        public IPageDataIndexModel CreateIndexModel()
        {
            var indexModel = new TestPageIndexModel();
            this.SetBaseProperties(indexModel);

            indexModel.TestContentArea = TestContentArea.ToIndexableString(Language);

            return indexModel;
        }

        public bool ShouldIndex()
        {
            return true;
        }

        public Id Id => ContentGuid.ToString();

        public TypeName TypeName => TypeName.From<TestPageIndexModel>();

        public ITypeMapping CreateTypeMapping(CultureInfo cultureInfo)
        {
            return new TypeMappingDescriptor<TestPageIndexModel>().AutoMap();
        }
    }

    public class TestBlock : BlockData, IIndexableBlockData
    {

        public virtual string Title { get; set; }
        public virtual string Summary { get; set; }

        public string ToIndexableString()
        {
            return $"{Title} {Summary}";
        }
    }

    public class TestPageIndexModel : IPageDataIndexModel
    {
        public Guid Id { get; set; }
        public string ContentReference { get; set; }
        public Guid? SiteDefinitionId { get; set; }
        public string TestContentArea { get; set; }
    }

}
