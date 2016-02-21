using EPi.Cms.Search.Elasticsearch.Indexing;
using Xunit;

namespace EPi.Cms.Search.Elasticsearch.UnitTest
{
    public class DefaultCmsElasticSearchOptionsTests
    {
        [Fact]
        public static void IndexName_Should_Return_Site()
        {
            var options = new CmsElasticSearchOptions();
            Assert.Equal("site" , options.IndexName);
        }
    }
}
