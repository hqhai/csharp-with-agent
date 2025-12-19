namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using EFCore.BulkExtensions;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteStudentsInEventByAdminSchoolCommand : IRequest<MethodResult<bool>>
    {
        public EnumCompetitionEventCategory Category { get; set; }
    }

    public class DeleteStudentsInEventByAdminSchoolCommandHandler : IRequestHandler<DeleteStudentsInEventByAdminSchoolCommand, MethodResult<bool>>
    {
        private readonly AuthContext _authContext;
        private readonly UserManager<User> _userManager;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IOrderService _orderService;

        public DeleteStudentsInEventByAdminSchoolCommandHandler(AuthContext authContext, UserManager<User> userManager, IStudentRepository studentRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, ICompetitionEventsRepository competitionEventsRepository, IOrderService orderService)
        {
            _authContext = authContext;
            _userManager = userManager;
            _studentRepository = studentRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _orderService = orderService;
        }

        public async Task<MethodResult<bool>> Handle(DeleteStudentsInEventByAdminSchoolCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;

            //var currentUser = await _userManager.Users.Include(p => p.UserSchools).FirstOrDefaultAsync(p => p.Id == _authContext.CurrentUserId, cancellationToken);
            //if (currentUser == null)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(currentUser));
            //    return methodResult;
            //}

            //var schoolId = currentUser.UserSchools.FirstOrDefault()?.SchoolId;
            //if (!schoolId.HasValue)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolId));
            //    return methodResult;
            //}

            //var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            //var competitionEvents = _competitionEventsRepository.Queryable.ToList();
            //var competitionEvent = competitionEvents.Where(p => p.SchoolIds != null && p.SchoolIds.Contains(schoolId.Value) && p.Category == request.Category).FirstOrDefault();

            //if (competitionEvent == null || competitionEvent.EventContent == null || competitionEvent.EventContent.ActionConfigs == null || competitionEvent.EventContent.ActionConfigs.Any(p => p.Action == EnumSchoolEventRuleAction.ImportStudent && p.EndDate.HasValue && p.EndDate.Value < currentDate))
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvent));
            //    return methodResult;
            //}

            //var studentEntities = from s in _studentRepository.Queryable
            //                      join h in _humanRepository.Queryable on s.HumanId equals h.Id
            //                      join u in _userManager.Users on h.UserId equals u.Id
            //                      join sce in _studentCompetitionEventsRepository.Queryable on s.Id equals sce.StudentId
            //                      join ce in _competitionEventsRepository.Queryable on sce.CompetitionEventId equals ce.Id
            //                      where s.SchoolId == schoolId.Value && ce.Id == competitionEvent.Id
            //                      select new { User = u, Human = h, Student = s, StudentCompetitionEvent = sce };

            //var query = await studentEntities.ToListAsync(cancellationToken);

            //var users = query.Select(p => p.User).ToList();
            //if (users == null || users.Count == 0)
            //{
            //    methodResult.Result = true;
            //    methodResult.StatusCode = StatusCodes.Status200OK;
            //    return methodResult;
            //}
            //var humans = query.Select(p => p.Human).ToList();
            //var students = query.Select(p => p.Student).ToList();
            //var studentCompetitionEvents = query.Select(p => p.StudentCompetitionEvent).ToList();

            //var userIds = users.Select(p => p.Id).ToList();

            //await _studentRepository.ExecuteTransactionAsync(async () =>
            //{
            //    await _studentCompetitionEventsRepository.BulkDeleteList(studentCompetitionEvents, false);
            //    await _studentRepository.BulkDeleteList(students, false);
            //    await _humanRepository.BulkDeleteList(humans, false);

            //    await _studentRepository.DbContext.BulkDeleteAsync(users, bulkConfig: null);

            //    var deleteOrdersResult = await _orderService.DeleteOrderOfStudentEvent(new DeleteOrderOfStudentsInEventCommandModel()
            //    {
            //        UserIds = userIds
            //    });

            //    if (!deleteOrdersResult.IsSuccessStatusCode)
            //    {
            //        methodResult.AddError(deleteOrdersResult.Error);
            //        return methodResult;
            //    }

            //    methodResult.StatusCode = StatusCodes.Status201Created;
            //    methodResult.Result = true;
            //    return methodResult;
            //});

            //return methodResult;
        }
    }
}
