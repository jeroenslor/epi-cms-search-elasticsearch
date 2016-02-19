using System;
using EPiServer.Core;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public interface IPageDataIndexModel
    {
        Guid Id { get; set; }
        ContentReference ContentReference { get; set; }
    }
}