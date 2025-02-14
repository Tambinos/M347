using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Contracts;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private const string AdminRole = "Admin";

        private readonly IRepository<InventoryItem> inventoryItemsRepository;
        private readonly IRepository<CatalogItem> catalogItemsRepository;
        private readonly IPublishEndpoint publishEndpoint;

        public ItemsController(
            IRepository<InventoryItem> inventoryItemsRepository,
            IRepository<CatalogItem> catalogItemsRepository,
            IPublishEndpoint publishEndpoint)
        {
            this.inventoryItemsRepository = inventoryItemsRepository;
            this.catalogItemsRepository = catalogItemsRepository;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var currentUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub); //We are using the User property from ControllerBase to get the current user’s id from the JWT token

            if (Guid.Parse(currentUserId) != userId) //We are checking if the current user’s id matches the requested user’s id
            {
                if (!User.IsInRole(AdminRole)) //We are checking if the current user is an admin
                {
                    return Forbid();
                }
            }

            var inventoryItemEntities = await inventoryItemsRepository.GetAllAsync(item => item.UserId == userId); //We are using the IRepository<T> interface to access the database see Play.Common/src/Play.Common/IRepository.cs
            var itemIds = inventoryItemEntities.Select(item => item.CatalogItemId); //We are getting the item ids from the inventory items entities to get the catalog items from the catalog service
            var catalogItemEntities = await catalogItemsRepository.GetAllAsync(item => itemIds.Contains(item.Id)); //We are using the IRepository<T> interface to access the database see Play.Common/src/Play.Common/IRepository.cs

            var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem => //We are converting the inventory items entities into inventory item dtos
            {
                var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId); //We are getting the catalog item from the catalog items entities that matches the inventory item’s catalog item id
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description); //We are using the AsDto extension method to convert the InventoryItem entity into an InventoryItemDto
            });

            return Ok(inventoryItemDtos);
        }

        [HttpPost]
        [Authorize(Roles = AdminRole)]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await inventoryItemsRepository.GetAsync(
                item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    UserId = grantItemsDto.UserId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };

                await inventoryItemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemsDto.Quantity;
                await inventoryItemsRepository.UpdateAsync(inventoryItem);
            }

            await publishEndpoint.Publish(new InventoryItemUpdated(
                inventoryItem.UserId,
                inventoryItem.CatalogItemId,
                inventoryItem.Quantity
            ));

            return Ok();
        }
    }
}