namespace Fsel.Identity.Application.Commands.StudentCmd.StudentEventCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StudentEventViewLearningRecordCommand : IRequest<MethodResult<bool>>
    {
        public Guid? UserId { get; set; }
    }

    public class StudentEventViewLearningRecordCommandHandler : IRequestHandler<StudentEventViewLearningRecordCommand, MethodResult<bool>>
    {
        private readonly AuthContext _authContext;
        private readonly IStudentEventLearningRecordRepository _studentEventLearningRecordRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<User> _userManager;

        public StudentEventViewLearningRecordCommandHandler(AuthContext authContext, IStudentEventLearningRecordRepository studentEventLearningRecordRepository, IStudentRepository studentRepository, UserManager<User> userManager)
        {
            _authContext = authContext;
            _studentEventLearningRecordRepository = studentEventLearningRecordRepository;
            _studentRepository = studentRepository;
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(StudentEventViewLearningRecordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var userId = request.UserId ?? _authContext.CurrentUserId;

            var student = await (from s in _studentRepository.Queryable
                                 join u in _userManager.Users on s.UserId equals u.Id
                                 where u.Id == userId
                                 select s).FirstOrDefaultAsync(cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var studentEventLearningRecord = await _studentEventLearningRecordRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == student.Id, cancellationToken);
            if (studentEventLearningRecord == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _studentEventLearningRecordRepository.ExecuteTransactionAsync(async () =>
            {
                studentEventLearningRecord.IsView = true;
                _studentEventLearningRecordRepository.Update(studentEventLearningRecord);
                await _studentEventLearningRecordRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            return methodResult;
        }
    }
}
