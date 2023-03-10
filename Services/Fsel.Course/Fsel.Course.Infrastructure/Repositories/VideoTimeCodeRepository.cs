using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class VideoTimeCodeRepository : BaseRepository<VideoTimeCode>, IVideoTimeCodeRepository
    {
        public VideoTimeCodeRepository(BaseDbContext dbContext) : base(dbContext)
        {
        }
    }
}