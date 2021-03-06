using System.Globalization;
using Nest;

namespace EPi.Cms.Search.Elasticsearch.Indexing.TypeMap
{
    public interface IIndexableTypeMapper
    {
        TypeName TypeName { get; }
        ITypeMapping CreateTypeMapping(CultureInfo cultureInfo);
    }
}