using bike_project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bike_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffsController : ControllerBase
    {
        private readonly BikeStores46Context _context;
        private readonly IConfiguration config;
        public StaffsController(BikeStores46Context context, IConfiguration config)
        {
            _context = context;
            this.config = config;   
        }

        // GET: api/Staffs
        //[Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StaffDTO>>> GetStaffs()
        {
            var staffs = await _context.Staffs
           .Select(s => new StaffDTO
           {
               StaffId = s.StaffId,
               FirstName = s.FirstName,
               LastName = s.LastName,
               Email = s.Email,
               Phone = s.Phone,
               Active = s.Active,
               StoreId = s.StoreId,
               ManagerId = s.ManagerId
           })
           .ToListAsync();
            var responseMessage = $"Collection of Staffs";

         
            return Ok(new { Message = responseMessage, Products = staffs });

        }

        // GET: api/Staffs/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<StaffDTO>> GetStaff(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);

            if (staff == null)
            {
                return NotFound();
            }

            var staffDto = new StaffDTO
            {
                StaffId = staff.StaffId,
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                Email = staff.Email,
                Phone = staff.Phone,
                Active = staff.Active,
                StoreId = staff.StoreId,
                ManagerId = staff.ManagerId
            };
            var responseMessage = $"Staff";

           
            return Ok(new { Message = responseMessage, Products = staffDto });
        }

        // PUT: api/Staffs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStaff(int id, StaffDTO staffDto)
        {
            if (id != staffDto.StaffId)
            {
                return BadRequest();
            }

            var staff = await _context.Staffs.FindAsync(id);

            if (staff == null)
            {
                return NotFound();
            }

            // Update properties of existing Staff entity
            staff.FirstName = staffDto.FirstName;
            staff.LastName = staffDto.LastName;
            staff.Email = staffDto.Email;
            staff.Phone = staffDto.Phone;
            staff.Active = staffDto.Active;
            staff.StoreId = staffDto.StoreId;
            staff.ManagerId = staffDto.ManagerId;
            staff.Password= staffDto.Password;

            _context.Entry(staff).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StaffExists(id))
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
        public async Task<ActionResult<StaffDTO>> PostStaff(StaffDTO staffDto)
        {
            // Check if required fields are provided in the payload
            if (string.IsNullOrEmpty(staffDto.FirstName) || string.IsNullOrEmpty(staffDto.LastName) || string.IsNullOrEmpty(staffDto.Email) || staffDto.Phone == null || staffDto.Active == 0 || staffDto.StoreId == 0 || staffDto.ManagerId == 0)
            {
                var errorResponse = new ErrorResponseDto
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = "FirstName, Phone, Email, StoreId, ManagerId, Active, and LastName are required"
                };
                return BadRequest(errorResponse);
            }

            try
            {
                var staff = new Staff
                {
                    FirstName = staffDto.FirstName,
                    LastName = staffDto.LastName,
                    Email = staffDto.Email,
                    Phone = staffDto.Phone,
                    Active = staffDto.Active,
                    StoreId = staffDto.StoreId,
                    ManagerId = staffDto.ManagerId,
                    Password = BCrypt.Net.BCrypt.EnhancedHashPassword(staffDto.Password)
                };

                // Add staff to the database
                _context.Staffs.Add(staff);
                await _context.SaveChangesAsync();
                staffDto.StaffId = staff.StaffId;

                // Upload blob and check if successful
                bool isUploaded = await Helper.UploadBlob(config, staff);

                if (isUploaded)
                {
                    // Return success message with created StaffDto
                    var responseMessage = "Record Added Successfully!!";
                    return Ok(new { Message = responseMessage, Staff = staffDto });
                }
                else
                {
                    // Handle blob upload failure if needed
                    var errorResponse = new ErrorResponseDto
                    {
                        TimeStamp = DateTime.UtcNow,
                        Message = "Failed to upload blob"
                    };
                    return BadRequest(errorResponse);
                }
            }
            catch (Exception ex)
            {
                // Handle any unexpected exceptions
                var errorResponse = new ErrorResponseDto
                {
                    TimeStamp = DateTime.UtcNow,
                    Message = $"Failed to add Staff: {ex.Message}"
                };
                return BadRequest(errorResponse);
            }
        }





        // DELETE: api/Staffs/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }

            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Staffs/edit/{staffid}

        [HttpPatch("edit/{staffid}")]
        public async Task<IActionResult> PatchStaff(int staffid, [FromBody] JsonPatchDocument<StaffDTO> patchDocument)
        {
            var staff = await _context.Staffs.FindAsync(staffid);
            if (staff == null)
            {
                return NotFound();
            }

            var staffDto = new StaffDTO
            {
                StaffId = staff.StaffId,
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                Email = staff.Email,
                Phone = staff.Phone,
                Active = staff.Active,
                StoreId = staff.StoreId,
                ManagerId = staff.ManagerId,
                Password = staff.Password // Include other properties as needed
            };

            patchDocument.ApplyTo(staffDto, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update the Staff entity with changes from StaffDTO
            staff.FirstName = staffDto.FirstName;
            staff.LastName = staffDto.LastName;
            staff.Email = staffDto.Email;
            staff.Phone = staffDto.Phone;
            staff.Active = staffDto.Active;
            staff.StoreId = staffDto.StoreId;
            staff.ManagerId = staffDto.ManagerId;
            staff.Password = staffDto.Password;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StaffExists(staffid))
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
        // GET: api/Staffs/storename/{storename}
        [Authorize]
        [HttpGet("storename/{storename}")]
        public async Task<ActionResult<IEnumerable<StaffDTO>>> GetStaffByStoreName(string storename)
        {
            var staffs = await _context.Staffs
                .Where(s => s.Store.StoreName == storename) // Assuming StoreName is a property of Store model
                .Select(s => new StaffDTO
                {
                    StaffId = s.StaffId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    Phone = s.Phone,
                    Active = s.Active,
                    StoreId = s.StoreId,
                    ManagerId = s.ManagerId
                })
                .ToListAsync();

            if (!staffs.Any())
            {
                return NotFound();
            }
            var responseMessage = $"Collection of Staffs";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Products = staffs });

        }
        // GET: api/Staffs/salesmadebystaff/{staffid}
        [Authorize]
        [HttpGet("salesmadebystaff/{staffid}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetSalesByStaff(int staffid)
        {
            var orders = await _context.Orders
                .Where(o => o.StaffId == staffid)
                .Select(o => new
                {
                    OrderId = o.OrderId,
                    CustomerName = o.Customer.FirstName
                })
                .ToListAsync();

            if (!orders.Any())
            {
                return NotFound();
            }
            var responseMessage = $"List of Orderid,customername";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Orders = orders });

        }

        // GET: api/Staffs/managerdetails/{staffid}
        [Authorize]
        [HttpGet("managerdetails/{staffid}")]
        public async Task<ActionResult<StaffDTO>> GetManagerDetails(int staffid)
        {
            // Find the staff member based on staffid
            var staff = await _context.Staffs.FindAsync(staffid);

            if (staff == null)
            {
                return NotFound("Staff member not found.");
            }

            // Check if the staff member has a manager
            if (staff.ManagerId == null)
            {
                return NotFound("This staff member does not have a manager.");
            }

            // Find the manager based on the ManagerId of the staff member
            var manager = await _context.Staffs.FindAsync(staff.ManagerId);

            if (manager == null)
            {
                return NotFound("Manager details not found.");
            }

            // Create a DTO object for the manager details
            var managerDto = new StaffDTO
            {
                StaffId = manager.StaffId,
                FirstName = manager.FirstName,
                LastName = manager.LastName,
                Email = manager.Email,
                Phone = manager.Phone,
                Active = manager.Active,
                StoreId = manager.StoreId,
                ManagerId = manager.ManagerId
            };
            var responseMessage = $"Staff";

            // Return both the custom message and the collection of categories
            return Ok(new { Message = responseMessage, Manager = managerDto });
        }



        private bool StaffExists(int id)
        {
            return _context.Staffs.Any(e => e.StaffId == id);
        }
    }
}
