using System.ComponentModel.DataAnnotations;

namespace bike_project.Models
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "CategoryName is required")]
        [MaxLength(50, ErrorMessage = "CategoryName cannot exceed 50 characters")]

        public string CategoryName { get; set; }



    }
}
