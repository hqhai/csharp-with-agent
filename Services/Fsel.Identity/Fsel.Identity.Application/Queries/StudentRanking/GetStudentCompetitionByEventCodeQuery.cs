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
    using Fsel.Identity.Application.Services.SystemService;
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

        public bool IsFinalLeaderBoard { get; set; }

        public bool IsCityLeaderBoard { get; set; }
        public bool IsNormalLeaderboard { get; set; }

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
        private const int TakeTopLeaderBoardEverEvent = 1;
        private const int TakeTopNineLeaderBoardCity = 9;
        private const int TakeAllLeaderBoard = 0;
        private const string? PositionOutOfTop = "200+";

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

            #region Validate

            var competitionEvents = _competitionEventsRepository.Queryable
                                    .FirstOrDefault(x => !string.IsNullOrEmpty(x.EventContentStr) && x.EventCode == request.EventCode);

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

            #region Get-Config
            var weekEventRules = competitionEvents.EventContent.WeekEvents.FirstOrDefault(x => x.WeekNumber == request.WeekNumber);
            if (weekEventRules == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(weekEventRules), weekEventRules);
                return methodResult;
            }
            var resultSnapShots = _studentCompetitionSnapShotRepository.Queryable.FirstOrDefault(x => x.EventCode == request.EventCode && x.StartDate == weekEventRules.StartDate && x.EndDate == weekEventRules.EndDate);

            #endregion

            if (resultSnapShots == null)
            {
                result = (from studentEvent in _eventRepository.Queryable
                          join studentCompetitionEvent in _studentCompetitionEventsRepository.Queryable.Where(x => (x.CompetitionEventId == competitionEvents.Id) ||
                                                                                                            (x.CompetitionEventId == competitionEvents.ParentEventId && !request.IsCityLeaderBoard))

                          on studentEvent.StudentId equals studentCompetitionEvent.StudentId
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
                              CourseResultId = studentEvent.CourseResultId,
                              CourseType = studentEvent.CourseType,
                              SchoolId = student.SchoolId
                          })
                          .OrderByDescending(x => x.RankingScore)
                          .ThenBy(x => x.FullName).ToList();

                int take = request.IsCityLeaderBoard ? TakeTopNineLeaderBoardCity : TakeAllLeaderBoard;


                if (request.IsFinalLeaderBoard)
                {
                    result = await GetFinalLeaderBoard(result, competitionEvents.Id, cancellationToken);
                }
                else if (request.IsCityLeaderBoard)
                {
                    result = await GetSingleStudentRanking(result, competitionEvents.Id, competitionEvents.ParentEventId, take, request.IsCityLeaderBoard, request.IsFinalLeaderBoard, true, false, cancellationToken);
                }
                else if (request.IsNormalLeaderboard)
                {
                    result = await GetSingleStudentRanking(result, competitionEvents.Id, competitionEvents.ParentEventId, take, false, false, true, true, cancellationToken);
                } 
                else
                {
                    result = await GetSingleStudentRanking(result, competitionEvents.Id, competitionEvents.ParentEventId, take, request.IsCityLeaderBoard, request.IsFinalLeaderBoard, false, false, cancellationToken);
                }
                resultTemp = result;
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
                }).Take(NumberStudentOnLeaderboard).ToList();

                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    resultSearch = resultSearch.Where(x => (x.FullName != null && x.FullName.ToLower(CultureInfo.InvariantCulture).Contains(request.Keyword.ToLower(CultureInfo.InvariantCulture).Trim(), StringComparison.InvariantCulture)) || (x.Email != null && x.Email.ToLower(CultureInfo.InvariantCulture) == request.Keyword.ToLower(CultureInfo.InvariantCulture).Trim())).ToList();
                    resultSearch.AddRange(resultTemp
                                         .Where(x =>
                                             (x.FullName != null && x.FullName.ToLower(CultureInfo.InvariantCulture).Contains(request.Keyword.ToLower(CultureInfo.InvariantCulture).Trim(), StringComparison.InvariantCulture)) ||
                                             (x.Email != null && x.Email.ToLower(CultureInfo.InvariantCulture) == request.Keyword.ToLower(CultureInfo.InvariantCulture).Trim()))
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
                                             EventRankingPosition = PositionOutOfTop
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


            #region Filter CourseType

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



        /// <summary>
        /// Filter những học sinh chỉ học Trung học phổ thông
        /// </summary>
        /// <param name="highSchoolFilter"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static List<StudentRankingModel> CustomLeaderBoardForHighSchool(IEnumerable<Guid>? highSchoolFilter, List<StudentRankingModel> result)
        {

            if (highSchoolFilter != null)
            {
                result = (from r in result
                          join hs in highSchoolFilter on r.SchoolId equals hs
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
                              CourseType = r.CourseType,
                              SchoolId = r.SchoolId,
                          }).ToList();

            }

            return result;
        }




        /// <summary>
        /// Lấy dữ liệu ranking đơn lẻ của từng sự kiện
        /// </summary>
        /// <param name="competitionEventId"></param>
        /// <param name="parentCompetitionEventId"></param>
        /// <param name="takeRecord"></param>
        /// <param name="isCityLeaderBoard"></param>
        /// <param name="isFinalLeaderBoard"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<List<StudentRankingModel>> GetSingleStudentRanking(List<StudentRankingModel> result, Guid competitionEventId, Guid? parentCompetitionEventId, int takeRecord, bool isCityLeaderBoard, bool isFinalLeaderBoard, bool firstTime, bool isNormalLeaderboard, CancellationToken cancellationToken)
        {
            var competitionEvents = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(x => x.Id == competitionEventId, cancellationToken);
            List<Guid> schoolCompetitionIds = new List<Guid>();

            if (competitionEvents == null)
            {
                return new List<StudentRankingModel>();
            }

            #region filter Event Thường
            if (competitionEvents.SchoolIds != null && !isFinalLeaderBoard)
            {
                schoolCompetitionIds = competitionEvents.SchoolIds.ToList();
            }

            if (!isFinalLeaderBoard && !isCityLeaderBoard)
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
                              CourseType = r.CourseType,
                              SchoolId = r.SchoolId,
                          }).ToList();
            }
            #endregion



            #region filter theo trạng thái của học sinh
            var eventStudentIds = result.Select(x => x.StudentId).ToList();
            ActiveCourseResultModel query = new ActiveCourseResultModel
            {
                StudentIds = eventStudentIds
            };

            var activeCourseResult = await _lmsCourseService.GetActiveCourseResultByStudentId(query);
            var activeCourseResultIds = activeCourseResult?.Content?.Result;

            if (activeCourseResultIds == null || activeCourseResultIds.Count == 0)
            {

                return new List<StudentRankingModel>();
            }
            #endregion
            if (isNormalLeaderboard)
            {
                return result;
            } 
            #region filter theo cấp
            IList<Guid>? schoolIds = result.Where(x => x.SchoolId != null).Select(x => (Guid)x.SchoolId!).ToList();
            var highSchoolResult = await _systemService.GetSchoolByIds(schoolIds);
            var highSchool = highSchoolResult?.Content?.Result;
            var highSchoolFilter = !isCityLeaderBoard ? highSchool?.Where(x => x.EducationLevel == EnumEducationLevel.Secondary || x.EducationLevel == EnumEducationLevel.InterLevel).Select(x => x.Id) : highSchool?.Where(x => x.EducationLevel == EnumEducationLevel.HighSchool).Select(x => x.Id);


            var tempHighSchool = !isCityLeaderBoard ? highSchool?.Where(x => x.EducationLevel == EnumEducationLevel.Secondary || x.EducationLevel == EnumEducationLevel.InterLevel) : highSchool?.Where(x => x.EducationLevel == EnumEducationLevel.HighSchool);
            result = (firstTime && !isCityLeaderBoard) ? result : CustomLeaderBoardForHighSchool(highSchoolFilter, result);
            #endregion

            result = result.DistinctBy(x => x.CourseResultId).Where(x => activeCourseResultIds.Contains(x.CourseResultId)).ToList();
            result = takeRecord > 0 ? result.Take(takeRecord).ToList() : result;

            return result;
        }


        /// <summary>
        /// Lấy dữ liệu ranking của tỉnh
        /// Custom riêng cho Thái Nguyên Fsel-4293
        /// Lấy 9 học sinh THCS của từng huyện + 9 học sinh THPT của toàn tỉnh
        /// </summary>
        /// <param name="competitionEventId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<List<StudentRankingModel>> GetFinalLeaderBoard(List<StudentRankingModel> result, Guid competitionEventId, CancellationToken cancellationToken)
        {
            var competitionParent = _competitionEventsRepository.Queryable.Include(x => x.CompetitionEvents).Where(x => x.Id == competitionEventId).FirstOrDefault();

            var resultSecondarySchool = new List<StudentRankingModel>();
            IList<Guid> childrenCodes = competitionParent!.CompetitionEvents.Where(x => x.EventCode != Special_Event).Select(x => x.Id).ToList();

            foreach (var childCode in childrenCodes)
            {
                var everEvent = await GetSingleStudentRanking(result, childCode, competitionParent.Id, TakeTopLeaderBoardEverEvent, false, false, false, false, cancellationToken);
                resultSecondarySchool.AddRange(everEvent);
            };


            Guid childCodeHighSchool = competitionParent!.CompetitionEvents.Where(x => x.EventCode == Special_Event).Select(x => x.Id).FirstOrDefault();
            var resultHighSchool = await GetSingleStudentRanking(result, childCodeHighSchool, competitionParent.Id, TakeTopNineLeaderBoardCity, true, false, false, false, cancellationToken);
            resultSecondarySchool.AddRange(resultHighSchool);

            return resultSecondarySchool.OrderByDescending(x => x.RankingScore).ThenBy(x => x.FullName).ToList();
        }


    }
}
