namespace Fsel.Identity.Application.Queries.StudentRanking

{
    using System.Globalization;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Commands.StudentCompetitionSnapShotCmd;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
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
        public bool? IsSearching { get; set; }

        public bool IsCityLeaderBoard { get; set; }
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
        private readonly ISystemService _systemService;

        private const double Process_Ratio = 0.75;
        private const double Overall_Ratio = 0.25;
        private const int NumberStudentOnLeaderboard = 200;
        private const string Special_Event = "EVThaiNguyenTHPT";
        private const string Parent_Special_Event = "ThaiNguyen";

        public GetStudentCompetitionByEventCodeQueryHandler(IStudentRankingEventRepository eventRepository, IStudentRepository studentRepository, ICompetitionEventsRepository competitionEventsRepository, IMediator mediator, IMapper mapper, IStudentCompetitionSnapShotRepository studentCompetitionSnapShotRepository, ILmsCourseService lmsCourseService, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, ISystemService systemService)
        {
            _eventRepository = eventRepository;
            _studentRepository = studentRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _mediator = mediator;
            _mapper = mapper;
            _studentCompetitionSnapShotRepository = studentCompetitionSnapShotRepository;
            _lmsCourseService = lmsCourseService;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<PagingItemStudentRankingModel>> Handle(GetStudentCompetitionByEventCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemStudentRankingModel> methodResult = new MethodResult<PagingItemStudentRankingModel>();
            List<StudentRankingModel> result = new List<StudentRankingModel>();
            List<StudentRankingModel> resultTemp = new List<StudentRankingModel>();

            var timeNowVI = DateTimeHelper.ConvertTimeFromUtc(DateTime.UtcNow, EnumCountryKey.Vietnam);



            #region for CITY-THAINGUYEN-THPT

            if (request.EventCode == Special_Event)
            {
                request.EventCode = Parent_Special_Event;
                request.IsCityLeaderBoard = true;
            }

            #endregion


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

            #endregion Validate

            var weekEventRules = competitionEvents.EventContent.WeekEvents.FirstOrDefault(x => x.WeekNumber == request.WeekNumber);
            if (weekEventRules == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(weekEventRules), weekEventRules);
                return methodResult;
            }




            var resultSnapShots = _studentCompetitionSnapShotRepository.Queryable.FirstOrDefault(x => x.EventCode == request.EventCode && x.StartDate == weekEventRules.StartDate && x.EndDate == weekEventRules.EndDate);


            List<Guid> schoolCompetitionIds = new List<Guid>();
            if (competitionEvents.SchoolIds != null)
            {
                schoolCompetitionIds = competitionEvents.SchoolIds.ToList();
            }

            if (resultSnapShots == null)
            {
                result = (from studentEvent in _eventRepository.Queryable
                          join studentCompetitionEvent in _studentCompetitionEventsRepository.Queryable.Where(x => (x.CompetitionEventId == competitionEvents.Id && !request.IsCityLeaderBoard) ||
                                                                                                            (x.CompetitionEventId == competitionEvents.ParentEventId && !request.IsCityLeaderBoard))
                          on studentEvent.StudentId equals studentCompetitionEvent.StudentId
                          join student in _studentRepository.Queryable.Include(x => x.Human) on studentCompetitionEvent.StudentId equals student.Id into resultGroup
                          from student in resultGroup.DefaultIfEmpty()
                          select new StudentRankingModel
                          {
                              StudentId = studentEvent.StudentId,
                              SchoolName = student.School,
                              Grade = student.SchoolGrade,
                              OverallScore = studentEvent.OverallScore,
                              Process = studentEvent != null ? studentEvent.Process : 0, // Thêm kiểm tra null và mặc định giá trị nếu null
                              FullName = student.Human != null ? student.Human.FullName : string.Empty,
                              Email = student.Human != null ? student.Human.Email : string.Empty,
                              AvatarPath = student.Human != null ? student.Human.AvatarPath : string.Empty, // Thêm kiểm tra null và mặc định giá trị nếu null
                              UserId = student.Human != null ? student.Human.UserId : new Guid(),
                              RankingScore = studentEvent.RankingScore,
                              CourseResultId = studentEvent.CourseResultId,
                              CourseType = studentEvent.CourseType,
                              SchoolId = student.SchoolId
                          }).OrderByDescending(x => x.RankingScore).ToList();



                if (competitionEvents.ParentEventId.HasValue)
                {
                    result = (from r in result
                              join schoolCompetition in schoolCompetitionIds on r.SchoolId equals schoolCompetition
                              select new StudentRankingModel
                              {
                                  StudentId = r.StudentId,
                                  SchoolName = r.SchoolName,
                                  Grade = r.Grade,
                                  OverallScore = r.OverallScore,
                                  Process = r.Process,
                                  FullName = r.FullName,
                                  Email = r.Email,
                                  AvatarPath = r.AvatarPath,
                                  UserId = r.UserId,
                                  RankingScore = r.RankingScore,
                                  CourseResultId = r.CourseResultId,
                                  CourseType = r.CourseType
                              }).ToList();
                }

                resultTemp = result;

                #region Filter

                var eventStudentIds = result.Select(x => x.StudentId).ToList();
                ActiveCourseResultModel query = new ActiveCourseResultModel
                {
                    StudentIds = eventStudentIds
                };


                #region Event FOR THPT
                IList<Guid>? schoolIds = result.Where(x => x.SchoolId != null).Select(x => (Guid)x.SchoolId!).ToList();
                GetLocationsByIdsQueryModel querySchool = new GetLocationsByIdsQueryModel
                {
                    Ids = schoolIds
                };
                var highSchoolResult = await _systemService.GetSchoolByIds(schoolIds);
                var highSchool = highSchoolResult?.Content?.Result;
                var highSchoolFilter = highSchool?.Where(x => x.EducationLevel == EnumEducationLevel.Secondary || x.EducationLevel == EnumEducationLevel.InterLevel).Select(x => x.Id) ?? null;
                result = request.IsCityLeaderBoard ? CustomLeaderBoardForHighSchool(highSchoolFilter, result) : result;
                #endregion



                var activeCourseResult = await _lmsCourseService.GetActiveCourseResultByStudentId(query);
                var activeCourseResultIds = activeCourseResult?.Content?.Result;

                if (activeCourseResultIds == null || activeCourseResultIds.Count == 0)
                {
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    return methodResult;
                }

                result = result.DistinctBy(x => x.CourseResultId).Where(x => activeCourseResultIds.Contains(x.CourseResultId)).ToList();
                resultTemp = result;
                result = result.Take(NumberStudentOnLeaderboard).ToList();

                #endregion
            }
            else
            {
                result = ConvertHelper.Deserialize<IList<StudentRankingModel>>(resultSnapShots.WeekCompetitionData)?.ToList() ?? new List<StudentRankingModel>();
            }
            #region Search

            if (request.IsSearching == true)
            {
                var resultSearch = result.Select((item, index) =>
                new StudentRankingModel
                {
                    StudentId = item.StudentId,
                    SchoolName = item.SchoolName,
                    Grade = item.Grade,
                    OverallScore = item.OverallScore,
                    Process = item.Process,
                    FullName = item.FullName,
                    Email = item.Email,
                    AvatarPath = item.AvatarPath,
                    UserId = item.UserId,
                    RankingScore = item.RankingScore,
                    CourseResultId = item.CourseResultId,
                    CourseType = item.CourseType,
                    EventRankingPosition = (index + 1).ToString(CultureInfo.CurrentCulture)
                }).ToList();

                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    resultSearch = resultSearch.Where(x => (x.FullName != null && x.FullName.ToLower().Contains(request.Keyword.ToLower().Trim())) || (x.Email != null && x.Email.ToLower() == request.Keyword.ToLower().Trim())).ToList();
                    resultSearch.AddRange(resultTemp
                                         .Where(x =>
                                             (x.FullName != null && x.FullName.ToLower().Contains(request.Keyword.ToLower().Trim())) ||
                                             (x.Email != null && x.Email.ToLower() == request.Keyword.ToLower().Trim()))
                                         .ToList()
                                         .Where(newItem => !resultSearch.Any(existingItem => existingItem.StudentId == newItem.StudentId))
                                         .Select(newItem => new StudentRankingModel
                                         {
                                             StudentId = newItem.StudentId,
                                             SchoolName = newItem.SchoolName,
                                             Grade = newItem.Grade,
                                             OverallScore = newItem.OverallScore,
                                             Process = newItem.Process != null ? newItem.Process : 0, // Thêm kiểm tra null và mặc định giá trị nếu null
                                             FullName = newItem.FullName ?? string.Empty,
                                             Email = newItem.Email ?? string.Empty,
                                             AvatarPath = newItem.AvatarPath ?? string.Empty, // Thêm kiểm tra null và mặc định giá trị nếu null
                                             UserId = newItem.UserId != Guid.Empty ? newItem.UserId : Guid.NewGuid(),
                                             RankingScore = newItem.RankingScore,
                                             CourseResultId = newItem.CourseResultId,
                                             CourseType = newItem.CourseType,
                                             EventRankingPosition = "200+"
                                         }).ToList()
                    );
                }

                if (resultSearch.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }

                var listsSearch = resultSearch.ApplyPaging(request).ToList();
                int totalItemSearch = resultSearch.Count;
                var resultPagingSearch = new PagingItemsModel<StudentRankingModel>(listsSearch, request, totalItemSearch);

                methodResult.Result = new PagingItemStudentRankingModel
                {
                    Items = resultPagingSearch.Items,
                    PagingInfo = resultPagingSearch.PagingInfo,
                    WeekEvent = weekEventRules
                };

                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            #endregion
            #region Filter

            //result = result.Where(x => x.CourseType == request.CourseType || request.IsCityLeaderBoard).ToList();

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

            #endregion Snapshot

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

        private static List<StudentRankingModel> CustomLeaderBoardForHighSchool(IEnumerable<Guid>? highSchoolFilter, List<StudentRankingModel> result)
        {


            if (highSchoolFilter != null)
            {
                result = (from r in result
                          join hs in highSchoolFilter on r.SchoolId equals hs into highSchoolGroup
                          from hs in highSchoolGroup.DefaultIfEmpty()
                          select new StudentRankingModel
                          {
                              StudentId = r.StudentId,
                              SchoolName = r.SchoolName,
                              Grade = r.Grade,
                              OverallScore = r.OverallScore,
                              Process = r.Process,
                              FullName = r.FullName,
                              Email = r.Email,
                              AvatarPath = r.AvatarPath,
                              UserId = r.UserId,
                              RankingScore = r.RankingScore,
                              CourseResultId = r.CourseResultId,
                              CourseType = r.CourseType
                          }).ToList();

            }

            return result;
        }
    }
}
