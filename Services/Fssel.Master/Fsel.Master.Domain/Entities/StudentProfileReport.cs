using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Shared.Enums;

namespace Fsel.Master.Domain.Entities
{
    [Table("StudentProfile_Report", Schema = "dbo")]
    public class StudentProfileReport
    {
        [Key]
        public Guid StudentId { get; set; }

        public Guid UserId { get; set; }

        public string? FullName { get; set; }

        public EnumGender? Gender { get; set; }

        public DateTime? Birthday { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public Guid? SchoolId { get; set; }

        public string? SchoolName { get; set; }

        public string? SchoolGrade { get; set; }

        public string? SchoolClass { get; set; }

        public Guid? ProvinceId { get; set; }

        public string? ProvinceName { get; set; }

        public Guid? DistrictId { get; set; }

        public string? DistrictName { get; set; }

        public Guid? PackageId { get; set; }

        public long? NumberOfToken { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
