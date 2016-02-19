using System.Collections.Generic;

namespace EPi.Cms.Search.Elasticsearch.Indexing.TypeMap
{
    public interface IIndexableTypeMapperHelper
    {
        IEnumerable<IIndexableTypeMapper> GetAll();
    }
}