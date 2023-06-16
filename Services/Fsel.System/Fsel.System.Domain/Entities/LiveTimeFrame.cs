using Fsel.Core.Entities;

namespace Fsel.System.Domain.Entities
{
    public class LiveTimeFrame : Entity
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
