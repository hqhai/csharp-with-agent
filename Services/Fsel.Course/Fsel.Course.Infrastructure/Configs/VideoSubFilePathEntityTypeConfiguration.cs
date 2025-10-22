using Fsel.Course.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fsel.Course.Infrastructure.Configs
{
    public class VideoSubFilePathEntityTypeConfiguration : IEntityTypeConfiguration<VideoSubFilePath>
    {
        public void Configure(EntityTypeBuilder<VideoSubFilePath> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.HasOne(a => a.Video)
             .WithMany(b => b.VideoSubFilePaths)
             .HasForeignKey(b => b.VideoId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
