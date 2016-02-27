using System.Linq;
using EPi.Cms.Search.Elasticsearch.Indexing;
using EPi.Cms.Search.Elasticsearch.Indexing.TypeMap;
using Xunit;

namespace EPi.Cms.Search.Elasticsearch.UnitTest
{
    public class TypeScanIndexableTypeMapperHelperTests
    {
        [Fact]
        public static void GetAll_Should_Return_New_Instance_Of_TestPage()
        {
            var helper = new TypeScanIndexableTypeMapperResolver();
            var indexableTypeMappers = helper.GetAll().ToList();

            Assert.Equal(1, indexableTypeMappers.Count);
            Assert.Equal(typeof(TestPage), indexableTypeMappers[0].GetType());
        }
    }
}
