using System.Globalization;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public interface IIndexableBlockData
    {
        string ToIndexableString();
    }
}