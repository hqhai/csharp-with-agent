namespace Fsel.Identity.Application.Queries.EventQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentEventLearningRecordQuery : IRequest<MethodResult<StudentEventLearningRecordModel>>
    {
        public Guid? UserId { get; set; }
    }

    public class GetStudentEventLearningRecordQueryHandler : IRequestHandler<GetStudentEventLearningRecordQuery, MethodResult<StudentEventLearningRecordModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IStudentEventLearningRecordRepository _studentEventLearningRecordRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IHumanRepository _humanRepository;
        private readonly UserManager<User> _userManager;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IMapper _mapper;

        public GetStudentEventLearningRecordQueryHandler(AuthContext authContext, IStudentEventLearningRecordRepository studentEventLearningRecordRepository, IStudentRepository studentRepository, IHumanRepository humanRepository, UserManager<User> userManager, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, IMapper mapper)
        {
            _authContext = authContext;
            _studentEventLearningRecordRepository = studentEventLearningRecordRepository;
            _studentRepository = studentRepository;
            _humanRepository = humanRepository;
            _userManager = userManager;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<StudentEventLearningRecordModel>> Handle(GetStudentEventLearningRecordQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentEventLearningRecordModel>();

            var userId = request.UserId ?? _authContext.CurrentUserId;

            var student = await (from s in _studentRepository.Queryable
                                 join h in _humanRepository.Queryable on s.HumanId equals h.Id
                                 join u in _userManager.Users on h.UserId equals u.Id
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

            methodResult.Result = _mapper.Map<StudentEventLearningRecordModel>(studentEventLearningRecord);
            return methodResult;
        }
    }
}
