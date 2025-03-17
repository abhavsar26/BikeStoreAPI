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
    public class ProductsController : ControllerBase
    {
        private readonly BikeStores46Context _context;

        public ProductsController(BikeStores46Context context)
        {
            _context = context;
        }

        // GET: api/Products
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            var products = await _context.Products
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    BrandId = p.BrandId,
                    CategoryId = p.CategoryId,
                    ModelYear = p.ModelYear,
                    ListPrice = p.ListPrice
                })
                .ToListAsync();
            var responseMessage = $"Collection of Products";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Products = products });
        }

        // GET: api/Products/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var productDTO = new ProductDTO
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                BrandId = product.BrandId,
                CategoryId = product.CategoryId,
                ModelYear = product.ModelYear,
                ListPrice = product.ListPrice
            };
            var responseMessage = $"Product";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Product = productDTO });
        }
        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDTO productDTO)
        {
            if (id != productDTO.ProductId)
            {
                return BadRequest();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Update properties based on DTO
            product.ProductName = productDTO.ProductName;
            product.BrandId = productDTO.BrandId;
            product.CategoryId = productDTO.CategoryId;
            product.ModelYear = productDTO.ModelYear;
            product.ListPrice = productDTO.ListPrice;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost("add")]  // Route attribute to specify the endpoint URL
        public async Task<ActionResult<ProductDTO>> PostProduct(ProductDTO productDTO)
        {
            // Check if required fields are provided in the payload
            if (string.IsNullOrEmpty(productDTO.ProductName) || productDTO.BrandId == 0 || productDTO.CategoryId == 0||productDTO.ModelYear==0||productDTO.ListPrice==0)
            {
                var errorResponse = new ErrorResponseDto
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "ProductName, BrandId,ModelYear,ListPrice and CategoryId are required"
                };
                return BadRequest(errorResponse);
            }

            try
            {
                // Create a new Product entity from the DTO
                var product = new Product
                {
                    ProductName = productDTO.ProductName,
                    BrandId = productDTO.BrandId,
                    CategoryId = productDTO.CategoryId,
                    ModelYear = productDTO.ModelYear,
                    ListPrice = productDTO.ListPrice
                    // Initialize other properties if needed
                };

                // Add the product to the DbContext
                _context.Products.Add(product);

                // Save changes asynchronously
                await _context.SaveChangesAsync();

                // Map the generated ProductId back to the DTO
                productDTO.ProductId = product.ProductId;

                // Return a success response with a custom message and the DTO data
                var successResponse = new SuccessResponseDto<ProductDTO>
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Product Added Successfully!!",
                    Data = productDTO
                };
                return Ok(successResponse);
            }
            catch (Exception)
            {
                // Catch any other unexpected exceptions and return a generic error response
                var errorResponse = new ErrorResponseDto
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Failed to add Product"
                };
                return BadRequest(errorResponse);
            }
        }

        // PATCH: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchProduct(int id, JsonPatchDocument<ProductDTO> patchDocument)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var productDTO = new ProductDTO
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                BrandId = product.BrandId,
                CategoryId = product.CategoryId,
                ModelYear = product.ModelYear,
                ListPrice = product.ListPrice
            };

            // Apply patch operations to the DTO
            patchDocument.ApplyTo(productDTO, ModelState);

            // Check if patch operations resulted in a valid state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update the product entity based on changes in the productDTO
            product.ProductName = productDTO.ProductName;
            product.BrandId = productDTO.BrandId;
            product.CategoryId = productDTO.CategoryId;
            product.ModelYear = productDTO.ModelYear;
            product.ListPrice = productDTO.ListPrice;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

        // DELETE: api/Products/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpGet("bycategoryname/{categoryName}")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsByCategoryName(string categoryName)
        {
            var products = await _context.Products
                .Where(p => p.Category.CategoryName == categoryName)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    BrandId = p.BrandId,
                    CategoryId = p.CategoryId,
                    ModelYear = p.ModelYear,
                    ListPrice = p.ListPrice
                })
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound();
            }
            var responseMessage = $"Collection of Products";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Products = products });
        }

        // GET: api/Products/bybrandname/{brandname}
        [Authorize]
        [HttpGet("bybrandname/{brandName}")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsByBrandName(string brandName)
        {
            var products = await _context.Products
                .Where(p => p.Brand.BrandName == brandName)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    BrandId = p.BrandId,
                    CategoryId = p.CategoryId,
                    ModelYear = p.ModelYear,
                    ListPrice = p.ListPrice
                })
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return products;
            }
            var responseMessage = $"Collection of Products";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Products = products });
        }
        // GET: api/Products/bymodelyear/{modelyear}
        [Authorize]
        [HttpGet("bymodelyear/{modelyear}")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsByModelYear(short modelyear)
        {
            var products = await _context.Products
                .Where(p => p.ModelYear == modelyear)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    BrandId = p.BrandId,
                    CategoryId = p.CategoryId,
                    ModelYear = p.ModelYear,
                    ListPrice = p.ListPrice
                })
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound();
            }
            var responseMessage = $"Collection of Products";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Products = products });
            
        }

        // GET: api/Products/numberofproductssoldbyeachstore
        [Authorize]
        [HttpGet("numberofproductssoldbyeachstore")]
        public async Task<ActionResult<IEnumerable<object>>> GetNumberOfProductsSoldByEachStore()
        {
            var productsSoldByStores = await _context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order.Store)
                .GroupBy(oi => oi.Order.Store.StoreName)
                .Select(g => new
                {
                    StoreName = g.Key,
                    NumberOfProductsSold = g.Sum(oi => oi.Quantity)
                })
                .ToListAsync();

            if (productsSoldByStores == null || productsSoldByStores.Count == 0)
            {
                return NotFound();
            }
            var responseMessage = $"Store Name and Number of Products";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Product = productsSoldByStores });
        }

        // GET: api/Products/ProductDetails
        [Authorize]
        [HttpGet("ProductDetails")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductDetails()
        {
            var productDetails = await _context.Products
                .Select(p => new
                {
                    CategoryName = p.Category.CategoryName,
                    ProductName = p.ProductName,
                    BrandName = p.Brand.BrandName
                })
                .ToListAsync();

            if (productDetails == null || productDetails.Count == 0)
            {
                return NotFound();
            }
            var responseMessage = $"Collection of Products";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Products = productDetails });
        }

        // GET: api/Products/purchasedbycustomer/{customerid}
        [Authorize]
        [HttpGet("purchasedbycustomer/{customerid}")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsPurchasedByCustomer(int customerid)
        {
            var productsPurchasedByCustomer = await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Order.CustomerId == customerid)
                .Select(oi => new ProductDTO
                {
                    ProductId = oi.Product.ProductId,
                    ProductName = oi.Product.ProductName,
                    BrandId = oi.Product.BrandId,
                    CategoryId = oi.Product.CategoryId,
                    ModelYear = oi.Product.ModelYear,
                    ListPrice = oi.Product.ListPrice
                })
                .ToListAsync();

            if (productsPurchasedByCustomer == null || productsPurchasedByCustomer.Count == 0)
            {
                return NotFound();
            }
            var responseMessage = $"Collection of Products";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Products = productsPurchasedByCustomer});

        }

        // GET: api/Products/productpurchasedbymaximumcustomer
        [Authorize]
        [HttpGet("productpurchasedbymaximumcustomer")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsPurchasedByMaximumCustomer()
        {
            var productsPurchasedByMaxCustomer = await _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => oi.Order.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    NumberOfProductsPurchased = g.Count()
                })
                .OrderByDescending(g => g.NumberOfProductsPurchased)
                .FirstOrDefaultAsync();

            if (productsPurchasedByMaxCustomer == null)
            {
                return NotFound();
            }

            var productsDTO = await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Order.CustomerId == productsPurchasedByMaxCustomer.CustomerId)
                .Select(oi => new ProductDTO
                {
                    ProductId = oi.Product.ProductId,
                    ProductName = oi.Product.ProductName,
                    BrandId = oi.Product.BrandId,
                    CategoryId = oi.Product.CategoryId,
                    ModelYear = oi.Product.ModelYear,
                    ListPrice = oi.Product.ListPrice
                })
                .ToListAsync();

            var responseMessage = $"Collection of Products";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Products = productsDTO });
        }

        // GET: api/Stock/getproductwithminimumstock
        [Authorize]
        [HttpGet("getproductwithminimumstock")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsWithMinimumStock()
        {
            var productsWithMinimumStock = await _context.Stocks
                .OrderBy(s => s.Quantity)
                .Select(s => s.Product)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    BrandId = p.BrandId,
                    CategoryId = p.CategoryId,
                    ModelYear = p.ModelYear,
                    ListPrice = p.ListPrice
                })
                .ToListAsync();

            if (productsWithMinimumStock == null || productsWithMinimumStock.Count == 0)
            {
                return NotFound();
            }
            var responseMessage = $"Collection of Products";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Products = productsWithMinimumStock });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }

}
