namespace bike_project.Models
{
    public class SuccessResponseDto<T>
    {
        public DateTime TimeStamp { get; set; }
        public DateOnly DateOnly { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
