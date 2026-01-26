// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Course.Lms.Application.Services.SenderService;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using Fsel.Shared.Models.ShareModels.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReportLearningResultQuery : SearchReportLearningResultQueryModel, IRequest<MethodResult<SearchReportLearningResultModel>>
    {
        public SearchReportLearningResultQuery()
        {
            ManagerReportType = EnumManagerReportType.ReportLearningResults;
        }
    }

    public class SearchReportLearningResultQueryHandler : IRequestHandler<SearchReportLearningResultQuery, MethodResult<SearchReportLearningResultModel>>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ISenderService _senderService;
        private readonly ICategoryRepository _categoryRepository;

        public SearchReportLearningResultQueryHandler(
            IMediator mediator,
            IMapper mapper,
            ICourseResultRepository courseResultRepository,
            IUnitResultRepository unitResultRepository,
            ICourseModuleRepository courseModuleRepository,
            IUnitRepository unitRepository,
            ISenderService senderService,
            ICategoryRepository categoryRepository)
        {
            _mediator = mediator;
            _mapper = mapper;
            _courseResultRepository = courseResultRepository;
            _unitResultRepository = unitResultRepository;
            _courseModuleRepository = courseModuleRepository;
            _unitRepository = unitRepository;
            _senderService = senderService;
            _categoryRepository = categoryRepository;
        }

        public async Task<MethodResult<SearchReportLearningResultModel>> Handle(SearchReportLearningResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var result = new MethodResult<SearchReportLearningResultModel>();

            if (request.PageSize > 100)
            {
                result.StatusCode = StatusCodes.Status400BadRequest;
                return result;
            }

            var report = await GetOverallReportAsync(request, cancellationToken);
            var students = await GetStudentsAsync(request, result, cancellationToken);
            if (!students.Any())
            {
                result.Result = report;
                return result;
            }

            var program = await _categoryRepository.GetProgramLevelsAsync(request.ProgramId, cancellationToken);

            var studentIds = students.Select(x => x.Id).ToList();
            var userIds = students
                .Where(x => x.UserId.HasValue)
                .Select(x => x.UserId!.Value)
                .ToList();

            var mailHistories = await GetMailHistoriesAsync(userIds);
            var unitResultDict = await GetUnitResultsAsync(studentIds, request.EndDate, cancellationToken);

            var learningResults = BuildLearningResults(
                students,
                program,
                unitResultDict,
                mailHistories);

            report.PagingItems = new PagingItemsModel<LearningResultModel>(
                learningResults,
                request,
                report.TotalStudent);

            result.Result = report;
            result.StatusCode = StatusCodes.Status200OK;
            return result;
        }

        #region Private methods

        private async Task<SearchReportLearningResultModel> GetOverallReportAsync(
        SearchReportLearningResultQuery request,
        CancellationToken cancellationToken)
        {
            var overallResult = await _mediator.Send(new GetOverallReportLearningResultQuery(request), cancellationToken);
            return _mapper.Map<SearchReportLearningResultModel>(overallResult.Result);
        }

        private async Task<List<StudentDtoModel>> GetStudentsAsync(
            SearchReportLearningResultQuery request,
            MethodResult<SearchReportLearningResultModel> result,
            CancellationToken cancellationToken)
        {
            var studentResult = await _mediator.Send(new GetStudentReportQuery(request), cancellationToken);
            if (!studentResult.IsOK)
            {
                result.AddError(studentResult.ErrorMessages);
                return new List<StudentDtoModel>();
            }
            var students = studentResult.Result?.ToList() ?? new List<StudentDtoModel>();
            if (request.StudentIds?.Any() == true)
            {
                students = students.Where(x => request.StudentIds.Contains(x.Id)).ToList();
            }
            return students;
        }

        private async Task<IList<HistorySendMailLearningProgressModel>> GetMailHistoriesAsync(
            List<Guid> userIds)
        {
            return (await _senderService.GetHistoriesSendMailLearningProgress(
                    new GetHistoriesSendMailLearningProgressModel
                    {
                        UserIds = userIds
                    })).Content?.Result ?? new List<HistorySendMailLearningProgressModel>();
        }

        private async Task<Dictionary<Guid, UnitResultAggregateDto>> GetUnitResultsAsync(
            List<Guid> studentIds,
            DateTime? endDate,
            CancellationToken cancellationToken)
        {
            var unitResults = await (
                from cr in _courseResultRepository.ReadQueryable
                    .WhereBulkContains(studentIds, x => x.StudentId)

                join ur in _unitResultRepository.ReadQueryable
                    on cr.Id equals ur.CourseResultId

                join u in _unitRepository.ReadQueryable
                    on ur.UnitId equals u.Id

                join cm in _courseModuleRepository.ReadQueryable
                    on new { ur.CourseId, u.OriginalId }
                    equals new { cm.CourseId, cm.OriginalId }

                where cr.WorkingStatus == EnumWorkingStatus.Active
                      && ur.Status == EnumResultStatus.Done
                      && (!endDate.HasValue ||
                          (ur.UpdatedDate ?? ur.CreatedDate).Date <= endDate.Value.Date)

                group new { ur, cm } by cr.StudentId
                into g

                select new UnitResultAggregateDto
                {
                    StudentId = g.Key,
                    OverallPercent = g.Average(x => x.ur.Percent),
                    Units = g.Select(x => new UnitPercentDto
                    {
                        DisplayOrder = x.cm.DisplayOrder,
                        Percent = x.ur.Percent
                    })
                    .Distinct()
                    .OrderBy(x => x.DisplayOrder)
                    .ToList()
                }).ToListAsync(cancellationToken);

            return unitResults.ToDictionary(x => x.StudentId);
        }

        private List<LearningResultModel> BuildLearningResults(
            List<StudentDtoModel> students,
            Category? program,
            Dictionary<Guid, UnitResultAggregateDto> unitResultDict,
            IList<HistorySendMailLearningProgressModel> mailHistories)
        {
            var results = new List<LearningResultModel>();

            foreach (var student in students)
            {
                unitResultDict.TryGetValue(student.Id, out var unitData);

                var levelName = program?.Levels
                    .FirstOrDefault(x => x.Id == student.LevelId)
                    ?.Code;

                results.Add(new LearningResultModel
                {
                    StudentId = student.Id,
                    Email = student.Email,
                    FullName = student.FullName,
                    PhoneNumber = student.PhoneNumber,
                    SchoolClass = student.SchoolClass,
                    SchoolGrade = student.SchoolGrade,
                    SchoolName = student.School,
                    LevelName = levelName,
                    UserName = student.UserName,
                    ProcessDate = student.CreatedDate,
                    ExpiredDate = student.ExpiredDate,
                    Status = student.ExpiredDate > DateTime.UtcNow
                        ? EnumLearningStatus.InProgress
                        : EnumLearningStatus.Expired,
                    OverallPercent = NumberHelper.ConvertRound(unitData?.OverallPercent ?? 0),
                    OverallModules = unitData?.Units.Select(x => new OverallModuleReportModel
                    {
                        Index = x.DisplayOrder,
                        DisplayOrder = x.DisplayOrder,
                        Percent = x.Percent,
                        Type = nameof(Domain.Entities.Unit)
                    }).ToList() ?? new(),
                    NumberOfEmailsSent = mailHistories.Count(x =>
                        student.UserId.HasValue &&
                        x.ReceiverId == student.UserId &&
                        x.Template == EnumSenderTemplate.LearningProgressWarning)
                });
            }

            return results;
        }

        #endregion Private methods
    }

    #region Internal DTOs

    public class UnitResultAggregateDto
    {
        public Guid StudentId { get; set; }
        public double OverallPercent { get; set; }
        public IList<UnitPercentDto> Units { get; set; } = new List<UnitPercentDto>();
    }

    public class UnitPercentDto
    {
        public int DisplayOrder { get; set; }
        public double Percent { get; set; }
    }

    #endregion Internal DTOs
}
