using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bike_project.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Mono.TextTemplating;
using System.IO;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Authorization;

namespace bike_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly BikeStores46Context _context;

        public CustomersController(BikeStores46Context context)
        {
            _context = context;
        }


        // GET: api/Customers
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            var customers = await _context.Customers
                .Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Phone = c.Phone,
                    Email = c.Email,
                    Street = c.Street,
                    City = c.City,
                    State = c.State,
                    ZipCode = c.ZipCode
                })
                .ToListAsync();

            var responseMessage = $"Collection of Customer";

          
            return Ok(new { Message = responseMessage, Customers = customers });
        }

        // GET: api/Customers/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            var customerDTO = new CustomerDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Phone = customer.Phone,
                Email = customer.Email,
                Street = customer.Street,
                City = customer.City,
                State = customer.State,
                ZipCode = customer.ZipCode
            };

            return customerDTO;
        }

        // PUT: api/Customers/5
       
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, CustomerDto customerDTO)
        {
            if (id != customerDTO.CustomerId)
            {
                return BadRequest();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            
            customer.FirstName = customerDTO.FirstName;
            customer.LastName = customerDTO.LastName;
            customer.Phone = customerDTO.Phone;
            customer.Email = customerDTO.Email;
            customer.Street = customerDTO.Street;
            customer.City = customerDTO.City;
            customer.State = customerDTO.State;
            customer.ZipCode = customerDTO.ZipCode;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
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

        // POST: api/Customers
       
        [Authorize]
        [HttpPost("add")]
        public async Task<ActionResult<CustomerDto>> PostCustomer(CustomerDto customerDTO)
        {
           
            if (string.IsNullOrEmpty(customerDTO.FirstName) || string.IsNullOrEmpty(customerDTO.LastName)|| string.IsNullOrEmpty(customerDTO.Email)|| string.IsNullOrEmpty(customerDTO.Phone)||string.IsNullOrEmpty(customerDTO.State)|| string.IsNullOrEmpty(customerDTO.City)|| string.IsNullOrEmpty(customerDTO.ZipCode))
            {
            
                return BadRequest(new ErrorResponseDto { TimeStamp = DateTime.UtcNow, Message = "FirstName,LastName,Email,Phone,State,City,Zipcode are required" });
            }

            try
            {
                var customer = new Customer
                {
                    FirstName = customerDTO.FirstName,
                    LastName = customerDTO.LastName,
                    Phone = customerDTO.Phone,
                    Email = customerDTO.Email,
                    Street = customerDTO.Street,
                    City = customerDTO.City,
                    State = customerDTO.State,
                    ZipCode = customerDTO.ZipCode
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                customerDTO.CustomerId = customer.CustomerId;

              
                var successResponse = new SuccessResponseDto<CustomerDto>
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "Customer record added successfully",
                    Data = customerDTO
                };

                return Ok(successResponse);
            }
            catch (Exception)
            {
                
                return BadRequest(new ErrorResponseDto { TimeStamp = DateTime.UtcNow, Message = "Failed to add customer record" });
            }
        }


        // DELETE: api/Customers/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Customers/bycity/{city}
        [Authorize]
        [HttpGet("bycity/{city}")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomersByCity(string city)
        {
            var customers = await _context.Customers
                .Where(c => c.City == city)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Phone = c.Phone,
                    Email = c.Email,
                    Street = c.Street,
                    City = c.City,
                    State = c.State,
                    ZipCode = c.ZipCode
                })
                .ToListAsync();

           
            var responseMessage = $"Collection of Customer";

           
            return Ok(new { Message = responseMessage, Customers = customers });
        }

        //GET: api/Customers/placeorderondate/{orderdate}
        [Authorize]
        [HttpGet("placeorderondate/{orderdate}")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomersByOrderDate(DateTime orderdate)
        {
            var orderDateOnly = new DateOnly(orderdate.Year, orderdate.Month, orderdate.Day);

            var customers = await _context.Orders
                .Where(o => o.OrderDate.Equals(orderDateOnly))
                .Select(o => o.Customer)
                .Distinct()
                .Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Phone = c.Phone,
                    Email = c.Email,
                    Street = c.Street,
                    City = c.City,
                    State = c.State,
                    ZipCode = c.ZipCode
                })
                .ToListAsync();

            // Customize the success response message
            var responseMessage = $"Collection of Customer";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Customers = customers });
        }

        // GET: api/Customers/placedhighestorder
        [Authorize]
        [HttpGet("placedhighestorder")]
        public async Task<ActionResult<CustomerDto>> GetCustomerWithHighestOrder()
        {
            var customerWithHighestOrder = await _context.OrderItems
                .GroupBy(oi => oi.Order.CustomerId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            if (customerWithHighestOrder == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(customerWithHighestOrder);
            if (customer == null)
            {
                return NotFound();
            }

            var customerDTO = new CustomerDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Phone = customer.Phone,
                Email = customer.Email,
                Street = customer.Street,
                City = customer.City,
                State = customer.State,
                ZipCode = customer.ZipCode
            };

            // Customize the success response message
            var responseMessage = $"Collection of Customer";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Customers = customerDTO });
        }

        // GET: api/Customers/zipcode/{zipcode}
        [Authorize]
        [HttpGet("zipcode/{zipcode}")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomersByZipCode(string zipcode)
        {
            var customers = await _context.Customers
                .Where(c => c.ZipCode == zipcode)
                .Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Phone = c.Phone,
                    Email = c.Email,
                    Street = c.Street,
                    City = c.City,
                    State = c.State,
                    ZipCode = c.ZipCode
                })
                .ToListAsync();

            // Customize the success response message
            var responseMessage = $"Collection of Customer";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Customers = customers });
        }

        [Authorize]
        // PATCH: api/customers/edit/{customerid}
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPatch("{customerid}")]
        public async Task<IActionResult> PatchCustomer(int customerid, JsonPatchDocument<CustomerDto> patchDocument)
        {
            var customer = await _context.Customers.FindAsync(customerid);
            if (customer == null)
            {
                return NotFound();
            }

            var customerDto = new CustomerDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Phone = customer.Phone,
                Email = customer.Email,
                Street = customer.Street,
                City = customer.City,
                State = customer.State,
                ZipCode = customer.ZipCode
            };

            // Apply patch operations to the DTO
            patchDocument.ApplyTo(customerDto, ModelState);

            // Check if patch operations resulted in a valid state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update the customer entity based on changes in the customerDTO
            customer.FirstName = customerDto.FirstName;
            customer.LastName = customerDto.LastName;
            customer.Phone = customerDto.Phone;
            customer.Email = customerDto.Email;
            customer.Street = customerDto.Street;
            customer.City = customerDto.City;
            customer.State = customerDto.State;
            customer.ZipCode = customerDto.ZipCode;

            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(customerid))
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


        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
