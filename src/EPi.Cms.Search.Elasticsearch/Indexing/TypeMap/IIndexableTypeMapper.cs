using System.Globalization;
using Nest;

namespace EPi.Cms.Search.Elasticsearch.Indexing.TypeMap
{
    public interface IIndexableTypeMapper
    {
        IPutMappingRequest CreateTypeMapping(CultureInfo cultureInfo);
    }
}