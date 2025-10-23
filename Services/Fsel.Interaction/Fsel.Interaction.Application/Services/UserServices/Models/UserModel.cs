using Fsel.Core.Base.BaseModels;

namespace Fsel.Interaction.Application.Services.UserServices.Models
{
    public class UserModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
    }
}
