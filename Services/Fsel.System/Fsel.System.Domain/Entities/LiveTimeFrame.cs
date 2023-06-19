using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;

namespace Fsel.System.Domain.Entities
{
    public class LiveTimeFrame : Entity
    {
        [Required(ErrorMessage = "The DateTime field is required.")]
        public DateTime? StartTime { get; set; }

        [Required(ErrorMessage = "The DateTime field is required.")]
        public DateTime? EndTime { get; set; }
    }
}
