using System;

namespace Play.Catalog.Contracts  //We are creating a new project called Play.Catalog.Contracts and we are adding the CatalogItemCreated, CatalogItemUpdated, and CatalogItemDeleted records because we are going to use them in the Play.Catalog.Service project to publish events to the message broker
{
    public record CatalogItemCreated(
        Guid ItemId, 
        string Name, 
        string Description,
        decimal Price);

    public record CatalogItemUpdated(
        Guid ItemId, 
        string Name, 
        string Description,
        decimal Price);

    public record CatalogItemDeleted(Guid ItemId);
}