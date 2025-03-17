namespace bike_project.Models
{
    public class ProductDTO
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public int BrandId { get; set; }

        public int CategoryId { get; set; }

        public short ModelYear { get; set; }

        public decimal ListPrice { get; set; }

    }
}
