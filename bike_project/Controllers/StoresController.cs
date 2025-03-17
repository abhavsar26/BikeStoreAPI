using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike_project.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;

namespace bike_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly BikeStores46Context _context;

        public StoresController(BikeStores46Context context)
        {
            _context = context;
        }

        // GET: api/Stores
        //[Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetStores()
        {
            var stores = await _context.Stores
           .Select(s => new StoreDto
           {
               StoreId = s.StoreId,
               StoreName = s.StoreName,
               Phone = s.Phone ?? "", // Handle nullability if needed
               Email = s.Email ?? "", // Handle nullability if needed
               Street = s.Street ?? "", // Handle nullability if needed
               City = s.City ?? "", // Handle nullability if needed
               State = s.State ?? "", // Handle nullability if needed
               ZipCode = s.ZipCode ?? "" // Handle nullability if needed
           })
           .ToListAsync();
            var responseMessage = $"Collection of Stores";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Stores = stores });

        }

        // GET: api/Stores/5.
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<StoreDto>> GetStore(int id)
        {
            var store = await _context.Stores.FindAsync(id);

            if (store == null)
            {
                return NotFound();
            }

            var storeDto = new StoreDto
            {
                StoreId = store.StoreId,
                StoreName = store.StoreName,
                Phone = store.Phone ?? "", // Handle nullability if needed
                Email = store.Email ?? "", // Handle nullability if needed
                Street = store.Street ?? "", // Handle nullability if needed
                City = store.City ?? "", // Handle nullability if needed
                State = store.State ?? "", // Handle nullability if needed
                ZipCode = store.ZipCode ?? "" // Handle nullability if needed
            };
            var responseMessage = $"Store";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Stores = storeDto });

        }

        // PUT: api/Stores/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStore(int id, StoreDto storeDto)
        {
            if (id != storeDto.StoreId)
            {
                return BadRequest();
            }

            var store = await _context.Stores.FindAsync(id);

            if (store == null)
            {
                return NotFound();
            }

            store.StoreName = storeDto.StoreName;
            store.Phone = storeDto.Phone;
            store.Email = storeDto.Email;
            store.Street = storeDto.Street;
            store.City = storeDto.City;
            store.State = storeDto.State;
            store.ZipCode = storeDto.ZipCode;

            _context.Entry(store).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StoreExists(id))
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

        // POST: api/Stores
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost("add")]  // Route attribute to specify the endpoint URL
        public async Task<ActionResult<StoreDto>> PostStore(StoreDto storeDto)
        {
            // Example: Check if StoreName is provided in the payload
            if (string.IsNullOrEmpty(storeDto.StoreName)|| string.IsNullOrEmpty(storeDto.Phone)|| string.IsNullOrEmpty(storeDto.Email)|| string.IsNullOrEmpty(storeDto.City)||string.IsNullOrEmpty(storeDto.State)|| string.IsNullOrEmpty(storeDto.ZipCode))
            {
                var errorResponse = new ErrorResponseDto
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "StoreName,Phone,Email,City,Zipcode is required"
                };
                return BadRequest(errorResponse);
            }

            try
            {
                // Create a new Store entity from the DTO
                var store = new Store
                {
                    StoreName = storeDto.StoreName,
                    Phone = storeDto.Phone,
                    Email = storeDto.Email,
                    Street = storeDto.Street,
                    City = storeDto.City,
                    State = storeDto.State,
                    ZipCode = storeDto.ZipCode
                };

                // Add the store to the DbContext
                _context.Stores.Add(store);

                // Save changes asynchronously
                await _context.SaveChangesAsync();

                // Map the generated StoreId back to the DTO
                storeDto.StoreId = store.StoreId;

                // Return a success response with a custom message and the DTO data
                var successResponse = new SuccessResponseDto<StoreDto>
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Record Added Successfully!!",
                    Data = storeDto
                };
                return Ok(successResponse);
            }
            catch (Exception)
            {
                // Catch any other unexpected exceptions and return a generic error response
                var errorResponse = new ErrorResponseDto
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Failed to add Store"
                };
                return BadRequest(errorResponse);
            }
        }

        // DELETE: api/Stores/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null)
            {
                return NotFound();
            }

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Stores/edit/{storeid}
        [Authorize]
        [HttpPatch("edit/{storeid}")]
        public async Task<IActionResult> PatchStore(int storeid, JsonPatchDocument<StoreDto> patchDocument)
        {
            var store = await _context.Stores.FindAsync(storeid);
            if (store == null)
            {
                return NotFound();
            }

            var storeDto = new StoreDto
            {
                StoreId = store.StoreId,
                StoreName = store.StoreName,
                Phone = store.Phone,
                Email = store.Email,
                Street = store.Street,
                City = store.City,
                State = store.State,
                ZipCode = store.ZipCode
            };

            patchDocument.ApplyTo(storeDto, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            store.StoreName = storeDto.StoreName;
            store.Phone = storeDto.Phone;
            store.Email = storeDto.Email;
            store.Street = storeDto.Street;
            store.City = storeDto.City;
            store.State = storeDto.State;
            store.ZipCode = storeDto.ZipCode;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StoreExists(storeid))
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
        [HttpGet("city/{city}")]
        public async Task<ActionResult<IEnumerable<Store>>> GetStoresByCity(string city)
        {
            var stores = await _context.Stores
                .Where(s => s.City.ToLower() == city.ToLower())
                .ToListAsync();

            if (stores == null || stores.Count == 0)
            {
                return NotFound();
            }
            var responseMessage = $"Collection of Stores";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Stores = stores });

        }
        // GET: api/Stores/maxiumcustomers
        [Authorize]
        [HttpGet("maxiumcustomers")]
        public async Task<ActionResult<string>> GetStoreWithMaxCustomers()
        {
            var storeWithMaxCustomers = await _context.Stores
                .OrderByDescending(s => s.Orders.Count) // Assuming Orders represent customers
                .Select(s => s.StoreName)
                .FirstOrDefaultAsync();

            if (storeWithMaxCustomers == null)
            {
                return NotFound();
            }

            var responseMessage = $"Storename";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Stores = storeWithMaxCustomers });
           
        }

        // GET: api/Stores/highestsale
        [Authorize]
        [HttpGet("highestsale")]
        public async Task<ActionResult<string>> GetStoreWithHighestSale()
        {
            var storeWithHighestSale = await _context.Stores
    .Include(s => s.Orders) // Ensure Orders collection is loaded
    .OrderByDescending(s => s.Orders.Sum(o => o.OrderStatus))
    .Select(s => s.StoreName)
    .FirstOrDefaultAsync();

            if (storeWithHighestSale == null)
            {
                return NotFound();
            }

            var responseMessage = $"Storename";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Stores = storeWithHighestSale });
           
        }



        // GET: api/Stores/totalstoreineachstate
        [Authorize]
        [HttpGet("totalstoreineachstate")]
        public async Task<ActionResult<IEnumerable<object>>> GetTotalStoresInEachState()
        {
            var stateStoreCounts = await _context.Stores
                .GroupBy(s => s.State)
                .Select(g => new
                {
                    State = g.Key,
                    NumberOfStores = g.Count()
                })
                .ToListAsync();

            var responseMessage = $"State,Number of store";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Store = stateStoreCounts });

        }
        private bool StoreExists(int id)
        {
            return _context.Stores.Any(e => e.StoreId == id);
        }
    }
}
