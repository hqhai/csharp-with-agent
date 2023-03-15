using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IVideoRepository : IRepository<Video>
    {
        Task<bool> IsVideoUsed(Guid Id);
    }
}
