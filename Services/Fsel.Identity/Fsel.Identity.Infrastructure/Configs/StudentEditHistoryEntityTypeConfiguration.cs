using Fsel.Common.Helpers;
using Fsel.Identity.Domain.Entities;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Identity.Infrastructure.Configs
{
    public class StudentEditHistoryEntityTypeConfiguration : IEntityTypeConfiguration<StudentEditHistory>
    {
        public void Configure(EntityTypeBuilder<StudentEditHistory> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Property(e => e.Type)
                 .HasMaxLength(100)
                 .HasConversion(
                     v => v.ToString(),
                     v => v.EnumParse<EnumStudentEditHistoryType>());
        }
    }
}
