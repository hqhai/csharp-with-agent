using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Newtonsoft.Json;

namespace Fsel.Course.Domain.Entities
{
    public class CourseUnitMockTest : Entity
    {
        /// <summary>
        /// Số thứ tự
        /// </summary>
        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int DisplayOrder { get; set; }

        [JsonIgnore]
        public Unit? Unit { get; set; }

        [JsonIgnore]
        public Course? Course { get; set; }

        [JsonIgnore]
        public MockTest? MockTest { get; set; }

        public Guid? UnitId { get; set; }

        [JsonIgnore]
        public Guid CourseId { get; set; }

        public Guid? MockTestId { get; set; }

        public ICollection<UnitResult> CourseUnitMockTestResults { get; set; } = new List<UnitResult>();
    }
}
