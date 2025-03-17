using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike_project.Models;
using Microsoft.AspNetCore.Authorization;

namespace bike_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly BikeStores46Context _context;

        public StocksController(BikeStores46Context context)
        {
            _context = context;
        }

        // GET: api/Stocks
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockDto>>> GetStocks()
        {
            var stocks = await _context.Stocks
                .Select(s => new StockDto
                {
                    StoreId = s.StoreId,
                    ProductId = s.ProductId,
                    Quantity = s.Quantity
                })
                .ToListAsync();
            var responseMessage = $"Collection of Stock";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Stocks = stocks });
           
        }

        // GET: api/Stocks/5,
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<StockDto>> GetStock(int id)
        {
            var stock = await _context.Stocks
                              .Where(s => s.StoreId == id)
                              .Select(s => new StockDto
                              {
                                  StoreId = s.StoreId,
                                  ProductId = s.ProductId,
                                  Quantity = s.Quantity
                              })
                              .FirstOrDefaultAsync();

            if (stock == null)
            {
                return NotFound();
            }
            var responseMessage = $"Stock";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Stocks = stock });

        }

        // PUT: api/Stocks/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStock(int id, StockDto stockDTO)
        {
            if (id != stockDTO.StoreId)
            {
                return BadRequest();
            }

            var stock = new Stock
            {
                StoreId = stockDTO.StoreId,
                ProductId = stockDTO.ProductId,
                Quantity = stockDTO.Quantity
            };

            _context.Entry(stock).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockExists(id))
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

        // POST: api/Stocks
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<StockDto>> PostStock(StockDto stockDTO)
        {
            // Check if stockDto or its required properties are null or invalid
            if (stockDTO.StoreId == 0 || stockDTO.ProductId == 0)
            {
                var errorResponse = new ErrorResponseDto
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Invalid stock data provided"
                };
                return BadRequest(errorResponse);
            }

            try
            {
                // Create a new Stock entity from the DTO
                var stock = new Stock
                {
                    StoreId = stockDTO.StoreId,
                    ProductId = stockDTO.ProductId,
                    Quantity = stockDTO.Quantity
                    // Set other properties as needed
                };

                // Add the stock to the DbContext
                _context.Stocks.Add(stock);

                // Save changes asynchronously
                await _context.SaveChangesAsync();

                // Map the generated StoreId (or any other ID) back to the DTO
                stockDTO.StoreId = stock.StoreId; // Assuming stockDto has a StoreId property

                // Return a success response with a custom message and the DTO data
                var successResponse = new SuccessResponseDto<StockDto>
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Stock record added successfully",
                    Data = stockDTO
                };
                return Ok(successResponse);
            }
            catch (Exception)
            {
                // Catch any other unexpected exceptions and return a generic error response
                var errorResponse = new ErrorResponseDto
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Failed to add stock"
                };
                return BadRequest(errorResponse);
            }

        }

        // DELETE: api/Stocks/5
        [Authorize]
        [HttpDelete("{storeId}/{productId}")]
        public async Task<IActionResult> DeleteStock(int storeId, int productId)
        {
            var stock = await _context.Stocks.FindAsync(storeId, productId);
            if (stock == null)
            {
                return NotFound();
            }

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StockExists(int id)
        {
            return _context.Stocks.Any(e => e.StoreId == id);
        }
}
}
