namespace BlazorClient.Pages.Catalog;
    public class ItemDto
{
    public Guid id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public decimal price { get; set; }
    public DateTimeOffset createdDate { get; set; }
};

 public class CreateItemDto
{
   
    public string name { get; set; }
    public string description { get; set; }
    public decimal price { get; set; }
  
};

 public class UpdateItemDto
{
   
    public string name { get; set; }
    public string description { get; set; }
    public decimal price { get; set; }
  
};

public class GrantItemsDto
{
   
    public Guid userId { get; set; }
    public Guid CatalogItemId { get; set; }
    public int quantity { get; set; }
 

};
