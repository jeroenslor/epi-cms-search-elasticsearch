using System.Linq;
using EPi.Cms.Search.Elasticsearch.Indexing;
using Xunit;

namespace EPi.Cms.Search.Elasticsearch.UnitTest
{
    public class AssemblyScanIndexableTypeMapperHelperTests
    {
        [Fact]
        public static void GetAll_Should_Return_New_Instance_Of_TestPage()
        {
            var helper = new AssemblyScanIndexableTypeMapperHelper();
            var indexableTypeMappers = helper.GetAll().ToList();

            Assert.Equal(1, indexableTypeMappers.Count);
            Assert.Equal(typeof(TestPage), indexableTypeMappers[0].GetType());
        }
    }
}
