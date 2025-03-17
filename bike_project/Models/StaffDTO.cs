namespace bike_project.Models
{
    public class StaffDTO
    {
        public int StaffId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public byte Active { get; set; }

        public int StoreId { get; set; }

        public int? ManagerId { get; set; }

        public string? Password { get; set; }
    }
}
