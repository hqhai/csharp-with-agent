// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentRanking

{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Commands.StudentCompetitionSnapShotCmd;
    using Fsel.Identity.Application.Queries.GoogleSheetQuery;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentCompetitionSnapShot;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentCompetitionSchoolQuery : BaseQueryModel, IRequest<MethodResult<PagingItemStudentRankingModel>>
    {
        public EnumCourseType CourseType { get; set; }

        public string? SchoolCode { get; set; }

        public int WeekNumber { get; set; }

        public bool IsConvertData { get; set; }
    }

    public class GetStudentCompetitionSchoolQueryHandler : IRequestHandler<GetStudentCompetitionSchoolQuery, MethodResult<PagingItemStudentRankingModel>>
    {
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IStudentRepository _studentRepository;
        private const double Process_Ratio = 0.75;
        private const double Overall_Ratio = 0.25;
        private readonly IMediator _mediator;
        private readonly IStudentCompetitionSnapShotRepository _studentCompetitionSnapShotRepository;
        private readonly IMapper _mapper;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRankingEventRepository _studentRankingEventRepository;

        public GetStudentCompetitionSchoolQueryHandler(ILmsCourseService lmsCourseService, IStudentRepository studentRepository, IMediator mediator, IStudentCompetitionSnapShotRepository studentCompetitionSnapShotRepository, IMapper mapper, ICompetitionEventsRepository competitionEventsRepository, IStudentRankingEventRepository studentRankingEventRepository)
        {
            _lmsCourseService = lmsCourseService;
            _studentRepository = studentRepository;
            _mediator = mediator;
            _studentCompetitionSnapShotRepository = studentCompetitionSnapShotRepository;
            _mapper = mapper;
            _competitionEventsRepository = competitionEventsRepository;
            _studentRankingEventRepository = studentRankingEventRepository;
        }

        public async Task<MethodResult<PagingItemStudentRankingModel>> Handle(GetStudentCompetitionSchoolQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemStudentRankingModel> methodResult = new MethodResult<PagingItemStudentRankingModel>();

            var timeNowVI = DateTimeHelper.ConvertTimeFromUtc(DateTime.UtcNow, EnumCountryKey.Vietnam);
            var listStudentCompetitionResult = await _mediator.Send(new GetListStudentSchoolQuery { SchoolCode = request.SchoolCode }, cancellationToken);
            var listStudentCompetition = listStudentCompetitionResult?.Result;

            // Đọc Data Week Events
            var competitionEvents = _competitionEventsRepository.Queryable
                                    .FirstOrDefault(x => !string.IsNullOrEmpty(x.EventContentStr) && x.EventCode == request.SchoolCode);

            if (competitionEvents == null)
            {
                return methodResult;
            }

            if (competitionEvents.EventContent == null || competitionEvents.EventContent.WeekEvents == null)
            {
                return methodResult;
            }

            var weekEventRules = competitionEvents.EventContent.WeekEvents.FirstOrDefault(x => x.WeekNumber == request.WeekNumber);

            var resultSnapShot = _studentCompetitionSnapShotRepository.Queryable.FirstOrDefault(x => x.EventCode == request.SchoolCode && x.StartDate == weekEventRules!.StartDate && x.EndDate == weekEventRules!.EndDate);


            IList<StudentRankingModel> studentRanking = new List<StudentRankingModel>();

            if (resultSnapShot == null)
            {
                // Lọc dữ liệu học sinh
                List<Guid> competitionStudentIds = listStudentCompetition!.Select(x => x.StudentId).ToList();
                var listStudentCompetion = listStudentCompetition!.ToList();
                var studentProgressAndOverall = await _lmsCourseService.GetStudentProgress(new StudentCompetitionStatQueryModel
                {
                    StudentIds = competitionStudentIds,
                    CourseType = request.CourseType,
                });
                var studentResults = studentProgressAndOverall?.Content?.Result;

                if (studentResults == null)
                {
                    studentResults = new List<CompetitionStudentProgressModel>();
                }
                var studentInfos = _studentRepository.Queryable.Include(x => x.User).Where(x => competitionStudentIds.Contains(x.Id)).ToList();

                studentRanking = (from studentFile in listStudentCompetion
                                  where studentFile != null
                                  join studentResult in studentResults on studentFile.StudentId equals studentResult.StudentId into resultGroup
                                  from studentResult in resultGroup.DefaultIfEmpty()
                                  join studentInfo in studentInfos on studentFile.StudentId equals studentInfo.Id into infoGroup
                                  from studentInfo in infoGroup.DefaultIfEmpty()
                                  where studentResult != null
                                  select new StudentRankingModel
                                  {
                                      StudentId = studentFile.StudentId,
                                      SchoolName = studentFile.SchoolName,
                                      Grade = studentFile.Grade.ToString(),
                                      Process = studentResult?.ContentCompleted ?? 0, // Thêm kiểm tra null và mặc định giá trị nếu null
                                      OverallScore = studentResult?.TotalScore ?? 0, // Thêm kiểm tra null và mặc định giá trị nếu null
                                      CompetitionEndDate = weekEventRules!.EndDate,
                                      FullName = studentFile.FullName,
                                      Email = studentFile.Email,
                                      AvatarPath = studentInfo?.User?.AvatarPath ?? string.Empty, // Thêm kiểm tra null và mặc định giá trị nếu null
                                      UserId = studentFile.UserId,
                                      RankingScore = Process_Ratio * (studentResult?.ContentCompleted ?? 0) + Overall_Ratio * (studentResult?.TotalScore ?? 0),
                                      CourseResultId = studentResult.CourseResultId,
                                      CourseType = studentResult.CourseType
                                  }).OrderByDescending(x => (Process_Ratio * x.Process + Overall_Ratio * x.OverallScore)).ToList();
            }
            else
            {
                studentRanking = ConvertHelper.Deserialize<IList<StudentRankingModel>>(resultSnapShot.WeekCompetitionData)!.ToList();
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                studentRanking = studentRanking.Where(x => (x.FullName != null && x.FullName.ToLower(CultureInfo.InvariantCulture).Contains(request.Keyword.ToLower(CultureInfo.InvariantCulture).Trim(), StringComparison.InvariantCulture)) || (x.Email != null && x.Email.ToLower(CultureInfo.InvariantCulture) == request.Keyword.ToLower(CultureInfo.InvariantCulture).Trim())).ToList();
            }

            var lists = studentRanking.ApplyPaging(request).ToList();
            int totalItem = studentRanking.Count;
            var resultPaging = new PagingItemsModel<StudentRankingModel>(lists, request, totalItem);

            if (request.IsConvertData)
            {
                IList<StudentRankingEvent> studentRankingEvents = new List<StudentRankingEvent>();
                studentRankingEvents = _mapper.Map<IList<StudentRankingEvent>>(studentRanking);

                await _studentRankingEventRepository.ExecuteTransactionAsync(async () =>
                {
                    await _studentRankingEventRepository.AddList(studentRankingEvents);
                    await _studentRankingEventRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                    return methodResult;
                });
            }

            TimeSpan timeWeek = new TimeSpan(weekEventRules!.EndDate.Hour, weekEventRules.EndDate.Minute, 0);
            TimeSpan timeNow = new TimeSpan(timeNowVI.Hour, timeNowVI.Minute, 0);

            // Lưu Snapshot theo tuần.
            if (weekEventRules!.EndDate.Date == timeNowVI && resultSnapShot == null)
            {
                StudentCompetitionSnapShotModel snapshotModel = new StudentCompetitionSnapShotModel
                {
                    EventCode = request.SchoolCode,
                    WeekCompetitionData = ConvertHelper.Serialize(lists),
                    StartDate = weekEventRules!.StartDate,
                    EndDate = weekEventRules!.EndDate,
                    WeekNumber = request.WeekNumber,

                };
                await UpdateSnapShot(snapshotModel, cancellationToken);
            }

            methodResult.Result = new PagingItemStudentRankingModel
            {
                Items = resultPaging.Items,
                PagingInfo = resultPaging.PagingInfo,
                WeekEvent = weekEventRules
            };
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }

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
