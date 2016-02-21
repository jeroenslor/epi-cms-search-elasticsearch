using System.Globalization;
using Nest;

namespace EPi.Cms.Search.Elasticsearch.Indexing.TypeMap
{
    public interface IIndexableTypeMapper
    {
        string TypeName { get; }
        ITypeMapping CreateTypeMapping(CultureInfo cultureInfo);
    }
}