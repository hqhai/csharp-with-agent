
namespace Fsel.Identity.Application.Queries.StudentRanking

{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Commands.StudentCompetitionSnapShotCmd;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentCompetitionSnapShot;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentCompetitionByEventCodeQuery : BaseQueryModel, IRequest<MethodResult<PagingItemStudentRankingModel>>
    {
        public EnumCourseType CourseType { get; set; }

        public string? EventCode { get; set; }

        public int WeekNumber { get; set; }

    }

    public class GetStudentCompetitionByEventCodeQueryHandler : IRequestHandler<GetStudentCompetitionByEventCodeQuery, MethodResult<PagingItemStudentRankingModel>>
    {
        private readonly IStudentRankingEventRepository _eventRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IStudentCompetitionSnapShotRepository _studentCompetitionSnapShotRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILmsCourseService _lmsCourseService;

        private const double Process_Ratio = 0.75;
        private const double Overall_Ratio = 0.25;

        public GetStudentCompetitionByEventCodeQueryHandler(IStudentRankingEventRepository eventRepository, IStudentRepository studentRepository, ICompetitionEventsRepository competitionEventsRepository, IMediator mediator, IMapper mapper, IStudentCompetitionSnapShotRepository studentCompetitionSnapShotRepository, ILmsCourseService lmsCourseService, IStudentCompetitionEventsRepository studentCompetitionEventsRepository)
        {
            _eventRepository = eventRepository;
            _studentRepository = studentRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _mediator = mediator;
            _mapper = mapper;
            _studentCompetitionSnapShotRepository = studentCompetitionSnapShotRepository;
            _lmsCourseService = lmsCourseService;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
        }

        public async Task<MethodResult<PagingItemStudentRankingModel>> Handle(GetStudentCompetitionByEventCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemStudentRankingModel> methodResult = new MethodResult<PagingItemStudentRankingModel>();
            List<StudentRankingModel> result = new List<StudentRankingModel>();

            var timeNowVI = DateTimeHelper.ConvertTimeFromUtc(DateTime.UtcNow, EnumCountryKey.Vietnam);

            var competitionEvents = _competitionEventsRepository.Queryable
                                    .FirstOrDefault(x => !string.IsNullOrEmpty(x.EventContentStr) && x.EventCode == request.EventCode);

            #region Validate

            if (competitionEvents == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvents), competitionEvents);
                return methodResult;
            }

            if (competitionEvents.EventContent == null || competitionEvents.EventContent.WeekEvents == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvents.EventContent), competitionEvents.EventContent);
                return methodResult;
            }
            #endregion

            var weekEventRules = competitionEvents.EventContent.WeekEvents.FirstOrDefault(x => x.WeekNumber == request.WeekNumber);
            if (weekEventRules == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(weekEventRules), weekEventRules);
                return methodResult;
            }

            var resultSnapShots = _studentCompetitionSnapShotRepository.Queryable.FirstOrDefault(x => x.EventCode == request.EventCode && x.StartDate == weekEventRules.StartDate && x.EndDate == weekEventRules.EndDate);

            if (resultSnapShots == null)
            {
                result = (from studentEvent in _eventRepository.Queryable
                          join studentCompetitionEvent in _studentCompetitionEventsRepository.Queryable.Where(x => x.CompetitionEventId == competitionEvents.Id) on studentEvent.StudentId equals studentCompetitionEvent.StudentId
                          join student in _studentRepository.Queryable.Include(x => x.User) on studentCompetitionEvent.StudentId equals student.Id into resultGroup
                          from student in resultGroup.DefaultIfEmpty()
                          select new StudentRankingModel
                          {
                              StudentId = studentEvent.StudentId,
                              SchoolName = student.School,
                              Grade = student.SchoolGrade,
                              OverallScore = studentEvent.OverallScore,
                              Process = studentEvent != null ? studentEvent.Process : 0, // Thêm kiểm tra null và mặc định giá trị nếu null
                              FullName = student.User != null ? student.User.FullName : string.Empty,
                              Email = student.User != null ? student.User.Email : string.Empty,
                              AvatarPath = student.User != null ? student.User.AvatarPath : string.Empty, // Thêm kiểm tra null và mặc định giá trị nếu null
                              UserId = student.User != null ? student.UserId : new Guid(),
                              RankingScore = studentEvent.RankingScore,
                              CourseResultId = studentEvent.CourseResultId
                          }).OrderByDescending(x => x.RankingScore).ToList();

                #region Filter

                var eventStudentIds = result.Select(x => x.StudentId).ToList();
                ActiveCourseResultModel query = new ActiveCourseResultModel
                {
                    StudentIds = eventStudentIds
                };
                var activeCourseResult = await _lmsCourseService.GetActiveCourseResultByStudentId(query);
                var activeCourseResultIds = activeCourseResult?.Content?.Result;

                if (activeCourseResultIds == null || activeCourseResultIds.Count == 0)
                {
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    return methodResult;
                }

                result = result.DistinctBy(x => x.CourseResultId).Where(x => activeCourseResultIds.Contains(x.CourseResultId)).ToList();

                #endregion
            }
            else
            {
                result = ConvertHelper.Deserialize<IList<StudentRankingModel>>(resultSnapShots.WeekCompetitionData)?.ToList() ?? new List<StudentRankingModel>();
            }

            #region Filter

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                result = result.Where(x => (x.FullName != null && x.FullName.ToLower().Contains(request.Keyword.ToLower().Trim())) || (x.Email != null && x.Email.ToLower() == request.Keyword.ToLower().Trim())).ToList();
            }
            #endregion

            #region Snapshot
            // Lưu Snapshot theo tuần.
            if (weekEventRules.EndDate.Date == timeNowVI && resultSnapShots == null)
            {
                StudentCompetitionSnapShotModel snapshotModel = new StudentCompetitionSnapShotModel
                {
                    EventCode = request.EventCode,
                    WeekCompetitionData = ConvertHelper.Serialize(result),
                    StartDate = weekEventRules!.StartDate,
                    EndDate = weekEventRules!.EndDate,
                    WeekNumber = request.WeekNumber,

                };
                await UpdateSnapShot(snapshotModel, cancellationToken);
            }
            #endregion

            var lists = result.ApplyPaging(request).ToList();
            int totalItem = result.Count;
            var resultPaging = new PagingItemsModel<StudentRankingModel>(lists, request, totalItem);

            methodResult.Result = new PagingItemStudentRankingModel
            {
                Items = resultPaging.Items,
                PagingInfo = resultPaging.PagingInfo,
                WeekEvent = weekEventRules
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }


        /// <summary>
        /// Lưu dữ liệu snapshot theo tuần
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task UpdateSnapShot(StudentCompetitionSnapShotModel model, CancellationToken cancellationToken)
        {
            var modelCommand = _mapper.Map<CreateStudentCompetitionSnapShotModel>(model);

            CreateSnapShotResultCommand cmd = new CreateSnapShotResultCommand
            {
                WeekNumber = modelCommand.WeekNumber,
                EndDate = modelCommand.EndDate,
                StartDate = modelCommand.StartDate,
                WeekCompetitionData = modelCommand.WeekCompetitionData,
                EventCode = modelCommand.EventCode
            };
            await _mediator.Send(cmd, cancellationToken);
        }
    }
}
