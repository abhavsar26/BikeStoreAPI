using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike_project.Models;
using Microsoft.AspNetCore.JsonPatch;
using static NuGet.Packaging.PackagingConstants;
using Microsoft.AspNetCore.Authorization;

namespace bike_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly BikeStores46Context _context;

        public OrdersController(BikeStores46Context context)
        {
            _context = context;
        }

        // GET: api/Orders
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
        {
            var orders = await _context.Orders
                .Select(o => new OrderDTO
                {
                    OrderId = o.OrderId,
                    CustomerId = o.CustomerId,
                    OrderStatus = o.OrderStatus,
                    OrderDate = o.OrderDate,
                    RequiredDate = o.RequiredDate,
                    ShippedDate = o.ShippedDate,
                    StoreId = o.StoreId,
                    StaffId = o.StaffId
                })
                .ToListAsync();
            var responseMessage = $"Collection of Order";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Orders = orders });
        }

        // GET: api/Orders/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDTO>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            var orderDTO = new OrderDTO
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                OrderStatus = order.OrderStatus,
                OrderDate = order.OrderDate,
                RequiredDate = order.RequiredDate,
                ShippedDate = order.ShippedDate,
                StoreId = order.StoreId,
                StaffId = order.StaffId
            };
            var responseMessage = $"Collection of Order";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Order = orderDTO });
            
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, OrderDTO orderDTO)
        {
            if (id != orderDTO.OrderId)
            {
                return BadRequest();
            }

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Update properties from DTO to entity
            order.CustomerId = orderDTO.CustomerId;
            order.OrderStatus = orderDTO.OrderStatus;
            order.OrderDate = orderDTO.OrderDate;
            order.RequiredDate = orderDTO.RequiredDate;
            order.ShippedDate = orderDTO.ShippedDate;
            order.StoreId = orderDTO.StoreId;
            order.StaffId = orderDTO.StaffId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
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
        [Authorize]
        [HttpPost("add")]
        public async Task<ActionResult<OrderDTO>> PostOrder(OrderDTO orderDTO)
        {
            // Check if required fields are provided
            if (orderDTO.CustomerId == 0 || orderDTO.StoreId == 0)
            {
                // Constructing the error response indicating missing or invalid fields
                return BadRequest(new ErrorResponseDto { TimeStamp = DateTime.UtcNow, Message = "Invalid or missing Order details" });
            }

            try
            {
                var order = new Order
                {
                    CustomerId = orderDTO.CustomerId,
                    OrderStatus = orderDTO.OrderStatus,
                    OrderDate = orderDTO.OrderDate,
                    RequiredDate = orderDTO.RequiredDate,
                    ShippedDate = orderDTO.ShippedDate,
                    StoreId = orderDTO.StoreId,
                    StaffId = orderDTO.StaffId
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Map the generated OrderId back to the DTO
                orderDTO.OrderId = order.OrderId;

                // Return a success response with a custom message and the DTO data
                var successResponse = new SuccessResponseDto<OrderDTO>
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Order added successfully",
                    Data = orderDTO
                };

                // Return 201 Created status with the newly created resource location
                return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, successResponse);
            }
            catch (Exception)
            {
                // Catch any other unexpected exceptions and return a generic error response
                return BadRequest(new ErrorResponseDto { TimeStamp = DateTime.UtcNow, Message = "Failed to add Order" });
            }
        }

        // DELETE: api/Orders/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPatch("edit/{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] JsonPatchDocument<OrderDTO> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var orderDTO = new OrderDTO
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                OrderStatus = order.OrderStatus,
                OrderDate = order.OrderDate,
                RequiredDate = order.RequiredDate,
                ShippedDate = order.ShippedDate,
                StoreId = order.StoreId,
                StaffId = order.StaffId
            };

            // Apply the patch document to the DTO
            patchDoc.ApplyTo(orderDTO, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update the entity with values from the DTO
            order.CustomerId = orderDTO.CustomerId;
            order.OrderStatus = orderDTO.OrderStatus;
            order.OrderDate = orderDTO.OrderDate;
            order.RequiredDate = orderDTO.RequiredDate;
            order.ShippedDate = orderDTO.ShippedDate;
            order.StoreId = orderDTO.StoreId;
            order.StaffId = orderDTO.StaffId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
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

        [Authorize]
        [HttpGet("customerid/{customerId}")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> SearchOrderByCustomerId(int customerId)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Select(o => new OrderDTO
                {
                    OrderId = o.OrderId,
                    CustomerId = o.CustomerId,
                    OrderStatus = o.OrderStatus,
                    OrderDate = o.OrderDate,
                    RequiredDate = o.RequiredDate,
                    ShippedDate = o.ShippedDate,
                    StoreId = o.StoreId,
                    StaffId = o.StaffId
                })
                .ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                return NotFound();
            }
            var responseMessage = $"Collection of Order";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Orders = orders });
        }
        
        [Authorize]
        [HttpGet("customername/{customerName}")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByCustomerName(string customerName)
        {
            // Find the customer by name
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.FirstName == customerName);

            if (customer == null)
            {
                return NotFound($"Customer with name '{customerName}' not found.");
            }

            // Fetch orders associated with the found customer
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customer.CustomerId)
                .Select(o => new OrderDTO
                {
                    OrderId = o.OrderId,
                    CustomerId = o.CustomerId,
                    OrderStatus = o.OrderStatus,
                    OrderDate = o.OrderDate,
                    RequiredDate = o.RequiredDate,
                    ShippedDate = o.ShippedDate,
                    StoreId = o.StoreId,
                    StaffId = o.StaffId
                })
                .ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                return NotFound($"No orders found for customer '{customerName}'.");
            }
            var responseMessage = $"Collection of Order";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Orders = orders });
        }

        [Authorize]
        [HttpGet("orderdate/{orderdate}")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByOrderDate(DateTime orderdate)
        {
            var orderDateOnly = new DateTime(orderdate.Year, orderdate.Month, orderdate.Day);
            var orders = await _context.Orders
                .Where(o => o.OrderDate == orderDateOnly)
                .Select(o => new OrderDTO
                {
                    OrderId = o.OrderId,
                    CustomerId = o.CustomerId,
                    OrderStatus = o.OrderStatus,
                    OrderDate = o.OrderDate,
                    RequiredDate = o.RequiredDate,
                    ShippedDate = o.ShippedDate,
                    StoreId = o.StoreId,
                    StaffId = o.StaffId
                })
                .ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                return NotFound($"No orders found for OrderDate '{orderdate}'.");
            }
            var responseMessage = $"Collection of Order";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Orders = orders });
        }

        [Authorize]
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrdersByStatus(byte status)
        {
            var orders = await _context.Orders
                .Where(o => o.OrderStatus == status)
                .Select(o => new OrderDTO
                {
                    OrderId = o.OrderId,
                    CustomerId = o.CustomerId,
                    OrderStatus = o.OrderStatus,
                    OrderDate = o.OrderDate,
                    RequiredDate = o.RequiredDate,
                    ShippedDate = o.ShippedDate,
                    StoreId = o.StoreId,
                    StaffId = o.StaffId
                })
                .ToListAsync();

            if (orders == null || orders.Count == 0)
            {
                return NotFound($"No orders found with OrderStatus '{status}'.");
            }
            var responseMessage = $"Collection of Order";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Orders = orders });
            
        }

        [Authorize]
        [HttpGet("numberoforderbydate")]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetNumberOfOrdersByDate()
        {
            var orderCounts = await _context.Orders
                .GroupBy(o => o.OrderDate)
                .Select(g => new OrderDTO
                {
                    OrderDate = g.Key,
                    OrderId = g.Count()
                })
                .OrderBy(o => o.OrderDate)
                .ToListAsync();

            if (orderCounts == null || orderCounts.Count == 0)
            {
                return NotFound("No orders found.");
            }
            var responseMessage = $"Orderdate number of order";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Orders = orderCounts });
        }

        [Authorize]
        [HttpGet("maxiumorderplaceonparticulardate")]
        public async Task<ActionResult<OrderDTO>> GetMaxOrderDate()
        {
            var maxOrderDate = await _context.Orders
                .GroupBy(o => o.OrderDate)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            if (maxOrderDate == default)
            {
                return NotFound("No orders found.");
            }

            var maxOrderDateDTO = new OrderDTO { OrderDate = maxOrderDate };

            var responseMessage = $"Order date";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Orders = maxOrderDateDTO });
            
        }
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
