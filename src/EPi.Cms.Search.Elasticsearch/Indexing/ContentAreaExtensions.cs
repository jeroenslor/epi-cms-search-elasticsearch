using System.Globalization;
using System.Text;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public static class ContentAreaExtensions
    {
        private static Injected<IContentRepository> ContentRepository { get; } 

        public static string ToIndexableString(this ContentArea contentArea, CultureInfo cultureInfo)
        {
            var stringBuilder = new StringBuilder();
            foreach (var contentAreaItem in contentArea.Items)
            {
                var indexableBlockData =
                    ContentRepository.Service.Get<IContent>(contentAreaItem.ContentLink, cultureInfo) as
                        IIndexableBlockData;

                if (indexableBlockData == null)
                    continue;

                stringBuilder.Append($" {indexableBlockData.ToIndexableString()}");
            }

            return stringBuilder.ToString();
        }
    }
}
