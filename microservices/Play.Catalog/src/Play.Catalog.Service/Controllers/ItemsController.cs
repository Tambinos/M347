using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers
{
    [ApiController] //The ApiController attribute enables a series of features that improve your REST api developer experience like having model validation errors automatically return a 400 Bad Request error or how to bind incoming requests into our method parameters.
    [Route("items")] //The Route attribute specifies the URL pattern that this controller will map to. For instance, if we use “items” here, it means that this controller will handle routes that start with /items, like https://localhost:5001/items
    public class ItemsController : ControllerBase  //ControllerBase provides many properties and methods useful when handling HTTP requests, like the BadRequest, NotFound and CreatedAtAction methods
    {
        private const string AdminRole = "Admin";

        private readonly IRepository<Item> itemsRepository; //We are using the IRepository<T> interface to access the database see Play.Common/src/Play.Common/IRepository.cs
        private readonly IPublishEndpoint publishEndpoint; //We are using the IPublishEndpoint interface to publish messages to the message broker see Play.Common/src/Play.Common/MassTransit/IMassTransitPublisher.cs

        public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint) //We are using dependency injection to get the repository and the publish endpoint 
        {
            this.itemsRepository = itemsRepository; //We are using the IRepository<T> interface to access the database see Play.Common/src/Play.Common/IRepository.cs
            this.publishEndpoint = publishEndpoint; //We are using the IPublishEndpoint interface to publish messages to the message broker see Play.Common/src/Play.Common/MassTransit/IMassTransitPublisher.cs
        }

        [HttpGet]
        [Authorize(Policies.Read)]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync() //The ActionResult<T> type is a wrapper around the HTTP response that allows us to return a 200 OK with the items as the response body
        {
            var items = (await itemsRepository.GetAllAsync())
                        .Select(item => item.AsDto()); //We are using the AsDto extension method to convert the Item entity into an ItemDto

            return Ok(items); // Ok is a method from ControllerBase that returns a 200 OK with the items as the response body
        }

        // GET /items/{id}
        [HttpGet("{id}")]
        [Authorize(Policies.Read)] //
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id) //Because we are using the ApiController attribute, we can use the ActionResult<T> type, which will automatically return a 404 Not Found if the item is not found
        {
            var item = await itemsRepository.GetAsync(id);

            if (item == null) //If we don’t use the ApiController attribute, we would have to manually check if the item is null and return a 404 Not Found
            {
                return NotFound(); // NotFound is a method from ControllerBase that returns a 404 Not Found
            }

            return item.AsDto(); //We are using the AsDto extension method to convert the Item entity into an ItemDto
        }

        // POST /items
        [HttpPost]
        [Authorize(Policies.Write)] //We are using the Authorize attribute to specify that only Authenticated users can access this endpoint
        public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
        {
            var item = new Item  //We are creating a new Item entity from the CreateItemDto
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await itemsRepository.CreateAsync(item); //We are using the repository to create the item

            await publishEndpoint.Publish(new CatalogItemCreated(
                item.Id,
                item.Name,
                item.Description,
                item.Price)); //We are publishing the CatalogItemCreated event

            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item); //We are returning a 201 Created with the item as the response body
        }

        // PUT /items/{id}
        [HttpPut("{id}")]
        [Authorize(Policies.Write)]
        public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = await itemsRepository.GetAsync(id); //We are using the repository to get the item by id

            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;

            await itemsRepository.UpdateAsync(existingItem); //

            await publishEndpoint.Publish(new CatalogItemUpdated(
                existingItem.Id,
                existingItem.Name,
                existingItem.Description,
                existingItem.Price));

            return NoContent(); //We are returning a 204 No Content
        }

        // DELETE /items/{id}
        [HttpDelete("{id}")]
        [Authorize(Policies.Write)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            await itemsRepository.RemoveAsync(item.Id);

            await publishEndpoint.Publish(new CatalogItemDeleted(id));

            return NoContent();
        }
    }
}