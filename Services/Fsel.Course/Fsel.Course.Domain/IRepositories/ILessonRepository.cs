using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Domain.IRepositories
{
    public interface ILessonRepository : IRepository<Lesson>
    {
        Task<bool> IsUnitLesson(Guid Id);
    }
}