using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike_project.Models;
using System.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace bike_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemsController : ControllerBase
    {
        private readonly BikeStores46Context _context;

        public OrderItemsController(BikeStores46Context context)
        {
            _context = context;
        }

        // GET: api/OrderItems
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetOrderItems()
        {
            var orderItems = await _context.OrderItems
                .Select(oi => new OrderItemDTO
                {
                    OrderId = oi.OrderId,
                    ItemId = oi.ItemId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    ListPrice = oi.ListPrice,
                    Discount = oi.Discount
                })
                .ToListAsync();

            // Customize the success response message
            var responseMessage = $"Collection of OrderDetails";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, OrderItems = orderItems });
        }

        // GET: api/OrderItems/{orderid}/billamount
        // Get the bill amount for an order including discount
        [Authorize]
        [HttpGet("{orderid}/billamount")]
        public async Task<ActionResult<decimal>> GetOrderBillAmount(int orderid)
        {
            try
            {
                // Find the order items for the given order id
                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == orderid)
                    .ToListAsync();

                if (orderItems == null || orderItems.Count == 0)
                {
                    return NotFound();
                }

                // Calculate the total bill amount including discounts
               // decimal billAmount = orderItems.Sum(oi => oi.Quantity * (oi.ListPrice + oi.Discount));
                decimal billAmount = orderItems.Sum(oi => oi.Quantity * oi.ListPrice);
                decimal bill = orderItems.Sum(oi=>oi.Discount);
                decimal amount = billAmount * bill;
                decimal final = amount + billAmount;
                var responseMessage = $"Bill Amount";

                // Return both the custom message and the collection of categories
                return Ok(new { Message = responseMessage, Amount = final });
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }


        // PUT: api/OrderItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderItem(int id, OrderItemDTO orderItemDTO)
        {
            if (id != orderItemDTO.OrderId)
            {
                return BadRequest();
            }
            var order= new OrderItem
            {
                OrderId = orderItemDTO.OrderId,
                ItemId = orderItemDTO.ItemId,
                ProductId = orderItemDTO.ProductId,
                Quantity = orderItemDTO.Quantity,
                ListPrice = orderItemDTO.ListPrice,
                Discount = orderItemDTO.Discount
            };


            _context.Entry(order).State = EntityState.Modified;
            
           
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/OrderItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost("add")]
        public async Task<ActionResult<OrderItemDTO>> PostOrderItem(OrderItemDTO orderItemDTO)
        {
            // Check if required fields are provided
            if (orderItemDTO.OrderId == 0 || orderItemDTO.ItemId == 0 || orderItemDTO.ProductId == 0 || orderItemDTO.Quantity <= 0)
            {
                // Constructing the error response indicating missing or invalid fields
                return BadRequest(new ErrorResponseDto { TimeStamp = DateTime.UtcNow, Message = "Invalid or missing OrderItem details" });
            }

            try
            {
                var orderItem = new OrderItem
                {
                    OrderId = orderItemDTO.OrderId,
                    ItemId = orderItemDTO.ItemId,
                    ProductId = orderItemDTO.ProductId,
                    Quantity = orderItemDTO.Quantity,
                    ListPrice = orderItemDTO.ListPrice,
                    Discount = orderItemDTO.Discount
                };

                _context.OrderItems.Add(orderItem);
                await _context.SaveChangesAsync();

                // Map the generated OrderItemId back to the DTO
                orderItemDTO.ItemId = orderItem.ItemId;

                // Return a success response with a custom message and the DTO data
                var successResponse = new SuccessResponseDto<OrderItemDTO>
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "OrderItem added successfully",
                    Data = orderItemDTO
                };

                return Ok(successResponse);
            }
            catch (Exception)
            {
                // Catch any other unexpected exceptions and return a generic error response
                return BadRequest(new ErrorResponseDto { TimeStamp = DateTime.UtcNow, Message = "Failed to add OrderItem" });
            }
        }



        // DELETE: api/OrderItems/5
        [Authorize]
        [HttpDelete("{orderId}/{itemId}")]
        public async Task<IActionResult> DeleteOrderItem(int orderId, int itemId)
        {
            var orderItem = await _context.OrderItems.FindAsync(orderId, itemId);
            if (orderItem == null)
            {
                return NotFound();
            }

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/orderitems/orderDetails/{orderid}
        // Display the bill for an order without discount
        [Authorize]
        [HttpGet("orderDetails/{orderid}")]
        public async Task<ActionResult<object>> GetOrderBillWithoutDiscount(int orderid)
        {
            try
            {
                // Find the order items for the given order id
                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == orderid)
                    .ToListAsync();

                if (orderItems == null || orderItems.Count == 0)
                {
                    return NotFound();
                }

                // Calculate the bill amount without considering discounts
                decimal billAmount = orderItems.Sum(oi => oi.Quantity * oi.ListPrice);

                // Construct the response object
                var response = new
                {
                    Purpose = "Display the bill for an order without discount",
                    BillAmount = billAmount
                };
                var responseMessage = $"Bill Amount";

                // Return both the custom message and the collection of categories
                return Ok(new { Message = responseMessage, BillAmount = billAmount });
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        private bool OrderItemExists(int id)
        {
            return _context.OrderItems.Any(e => e.OrderId == id);
        }
    }
}
