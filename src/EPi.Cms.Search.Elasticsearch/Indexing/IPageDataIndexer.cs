using System;
using System.Globalization;
using EPiServer.Core;

namespace EPi.Cms.Search.Elasticsearch.Indexing
{
    public interface IPageDataIndexer<in T>
    {
        IPageDataIndexModel CreateIndexModel(T pageData, CultureInfo cultureInfo);
    }

    public interface IPageDataIndexer
    {
        IPageDataIndexModel CreateIndexModel(PageData pageData, CultureInfo cultureInfo);
    }

    public interface IPageDataIndexModel
    {
        Guid Id { get; set; }
        ContentReference ContentReference { get; set; }
    }
}
