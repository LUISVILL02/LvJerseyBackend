using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Commands.CreateOrder;
using Orders.Application.Dtos;
using Shared.Application.Abstractions;

namespace LvJerseyApi.Controllers;

[ApiController]
[Route("api/v0.0.1/[controller]")]
[Authorize]
public class OrderController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderConfirmationDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var response = await sender.SendCommandAsync<CreateOrderCommand, OrderConfirmationDto>(command);
        return Created($"/api/v0.0.1/order/{response.OrderNumber}", response);
    }
}
