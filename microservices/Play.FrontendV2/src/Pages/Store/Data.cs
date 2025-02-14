namespace BlazorClient.Pages.Store;
    
    public record StoreDto(
       List<StoreItemDto> Items,
        decimal UserGil
    );

    public record StoreItemDto(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        int OwnedQuantity
    );

