namespace bike_project.Models
{
    public class ErrorResponseDto
    {
        public DateTime TimeStamp { get; set; }
        public DateOnly DateOnly { get; set; }
        public string Message { get; set; }
    }
}
