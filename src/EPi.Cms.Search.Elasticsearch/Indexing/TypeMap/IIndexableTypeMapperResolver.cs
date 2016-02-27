using System.Collections.Generic;

namespace EPi.Cms.Search.Elasticsearch.Indexing.TypeMap
{
    public interface IIndexableTypeMapperResolver
    {
        IEnumerable<IIndexableTypeMapper> GetAll();
    }
}