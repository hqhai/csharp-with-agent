namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Entities;

    public class UserModel
    {
        public string? Id { get; set; }

        public string? FullName { get; set; }

        public Guid StudentId { get; set; }
        public Guid HumanId { get; set; }

        public Human? Human { get; set; }
    }
}
