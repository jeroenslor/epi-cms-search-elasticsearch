using System.Globalization;
using System.Linq;
using EPi.Cms.Search.Elasticsearch.Indexing;
using EPiServer.Core;
using StructureMap;
using Xunit;

namespace EPi.Cms.Search.Elasticsearch.UnitTest
{
    public class ContainerTests
    {
        [Fact]
        public void GetAllPageDataIndexers_Should_Return_All_Indexers()
        {
            var container = new Container(_ =>
            {
                _.For<IPageDataIndexer<TestPage>>().Add<TestPageIndexer>();
            });

            var testPage = new TestPage();
            var foo = new TestPageIndexer();
            var bar = foo as IPageDataIndexer<PageData>; // this is not allowed since in parameters are not allowed to downcast
            Assert.NotNull(bar);

            var instances = container.GetAllInstances<IPageDataIndexer<PageData>>();

            Assert.True(instances.Count() == 1);
        }

        public class TestPageIndexer : IPageDataIndexer<TestPage>
        {
            public IPageDataIndexModel CreateIndexModel(TestPage pageData, CultureInfo cultureInfo)
            {
                return null;
            }
        }

        public class TestPage : PageData
        {
        }
    }
}
