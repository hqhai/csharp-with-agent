namespace Fsel.System.Infrastructure.Configs
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BannerScopeEntityTypeConfiguration : IEntityTypeConfiguration<BannerScope>
    {
        public void Configure(EntityTypeBuilder<BannerScope> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Property(e => e.CourseLevel)
                   .HasMaxLength(100)
                   .HasConversion(
                   v => v.ToString(),
                   v => v.EnumParse<EnumCourseLevel>());

            builder.HasOne(a => a.Banner)
                   .WithMany(b => b.BannerScopes)
                   .HasForeignKey(b => b.BannerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
