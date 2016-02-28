using System;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public interface IPageDataIndexModel
    {
        Guid Id { get; set; }
        string ContentReference { get; set; }
        Guid? SiteDefinitionId { get; set; }
    }
}