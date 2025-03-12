namespace Fsel.Identity.Application.Services.OrderService.Model
{
    public class CreateOrderForStudentsEventCommandModels
    {
        public DateTime ExpiredDate { get; set; }
        public IList<CreateOrderForStudentsEventCommandModel> Students { get; set; } = new List<CreateOrderForStudentsEventCommandModel>();
    }

    public class CreateOrderForStudentsEventCommandModel
    {
        public Guid UserId { get; set; }
        public Guid StudentId { get; set; }
        public string? StudentCode { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
