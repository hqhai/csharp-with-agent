using System.ComponentModel.DataAnnotations;
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
        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumCourseUnitMockTestErrorCode.CUM01C))]
        public int OrderNumber { get; set; }

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
    }
}
