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
    public class BrandsController : ControllerBase
    {
        private readonly BikeStores46Context _context;

        public BrandsController(BikeStores46Context context)
        {
            _context = context;
        }

        // GET: api/Brands
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
        {
            var brands = await _context.Brands
                             .Select(b => new BrandDto
                             {
                                 BrandId = b.BrandId,
                                 BrandName = b.BrandName
                             })
                             .ToListAsync();
            var responseMessage = $"Collection of Brand";

            return Ok(new { Message = responseMessage, Brands = brands });
           
        }

        // GET: api/Brands/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<BrandDto>> GetBrand(int id)
        {
            var brand = await _context.Brands
                            .Where(b => b.BrandId == id)
                            .Select(b => new BrandDto
                            {
                                BrandId = b.BrandId,
                                BrandName = b.BrandName
                            })
                            .FirstOrDefaultAsync();

            if (brand == null)
            {
                return NotFound();
            }

            var responseMessage = $"Brand";

           
            return Ok(new { Message = responseMessage, Brands = brand });
           
        }

        // PUT: api/Brands/5
        
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBrand(int id, BrandDto brandDto)
        {
            if (id != brandDto.BrandId)
            {
                return BadRequest();
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            brand.BrandName = brandDto.BrandName;
            

            _context.Entry(brand).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BrandExists(id))
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

        // POST: api/Brands
        
        [Authorize]
        [HttpPost("add")]  
        public async Task<ActionResult<BrandDto>> PostBrand(BrandDto brandDto)
        {
            
            if (string.IsNullOrEmpty(brandDto.BrandName))
            {
                var errorResponse = new ErrorResponseDto
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "BrandName is required"
                };
                return BadRequest(errorResponse);
            }

            try
            {
               
                var brand = new Brand
                {
                    BrandName = brandDto.BrandName

                };

              
                _context.Brands.Add(brand);

               
                await _context.SaveChangesAsync();

                
                brandDto.BrandId = brand.BrandId;

                var successResponse = new SuccessResponseDto<BrandDto>
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Record Added Successfully!!",
                    Data = brandDto
                };
                return Ok(successResponse);
            }
            catch (Exception)
            {
                
                var errorResponse = new ErrorResponseDto
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Failed to add Brand"
                };
                return BadRequest(errorResponse);
            }
        }


        // DELETE: api/Brands/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BrandExists(int id)
        {
            return _context.Brands.Any(e => e.BrandId == id);
        }
    }
}
