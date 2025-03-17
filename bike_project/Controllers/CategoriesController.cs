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
    public class CategoriesController : ControllerBase
    {
        private readonly BikeStores46Context _context;

        public CategoriesController(BikeStores46Context context)
        {
            _context = context;
        }

        // GET: api/Categories
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories
                                .Select(c => new CategoryDto
                                {
                                    CategoryId = c.CategoryId,
                                    CategoryName = c.CategoryName
                                })
                                .ToListAsync();

            
            var responseMessage = $"Collection of Category";

            
            return Ok(new { Message = responseMessage, Categories = categories });
        }

        // GET: api/Categories/5
        [Authorize]
        [HttpGet("{name}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(string name)
        {
            var category = await _context.Categories
                                .Where(c => c.CategoryName == name)
                                .Select(c => new CategoryDto
                                {
                                    CategoryId = c.CategoryId,
                                    CategoryName = c.CategoryName
                                })
                                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }
           
            var responseMessage = $"Category";

           
            return Ok(new { Message = responseMessage, Categories = category });
        }

        // PUT: api/Categories/5
     
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, CategoryDto categoryDto)
        {
            if (id != categoryDto.CategoryId)
            {
                return BadRequest();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.CategoryName = categoryDto.CategoryName;
            

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
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

        // POST: api/Categories
       
        [Authorize]
        [HttpPost("add")]  
        public async Task<ActionResult<CategoryDto>> PostCategory(CategoryDto categoryDto)
        {
            
            if (string.IsNullOrEmpty(categoryDto.CategoryName))
            {
                var errorResponse = new ErrorResponseDto
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "CategoryName is required"
                };
                return BadRequest(errorResponse);
            }

            try
            {
                
                var category = new Category
                {
                    CategoryName = categoryDto.CategoryName
                   
                };

               
                _context.Categories.Add(category);

              
                await _context.SaveChangesAsync();

                
                categoryDto.CategoryId = category.CategoryId;

                
                var successResponse = new SuccessResponseDto<CategoryDto>
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Record Added Successfully!!",
                    Data = categoryDto
                };
                return Ok(successResponse);
            }
            catch (Exception)
            {
               
                var errorResponse = new ErrorResponseDto
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Failed to add Category"
                };
                return BadRequest(errorResponse);
            }
        }



        // DELETE: api/Categories/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
