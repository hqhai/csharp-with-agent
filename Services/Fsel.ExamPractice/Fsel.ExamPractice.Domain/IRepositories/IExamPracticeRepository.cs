namespace Fsel.ExamPractice.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.ExamPractice.Domain.Entities;

    public interface IExamPracticeRepository : IRepository<ExamPractice>
    {
        Task<bool> IsUsingByClient(Guid id);
    }
}
