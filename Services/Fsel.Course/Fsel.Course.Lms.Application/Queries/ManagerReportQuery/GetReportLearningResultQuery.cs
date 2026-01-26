// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
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

    public class GetReportLearningResultQuery : SearchReportLearningResultQueryModel, IRequest<MethodResult<IList<LearningResultReportModel>>>
    {
        public GetReportLearningResultQuery()
        {
            ManagerReportType = EnumManagerReportType.ReportLearningResults;
        }

        public GetReportLearningResultQuery(SearchReportLearningResultQueryModel source)
        {
            ManagerReportType = EnumManagerReportType.ReportLearningResults;
            CopyFrom(source);
        }
    }

    public class GetReportLearningResultQueryHandler : IRequestHandler<GetReportLearningResultQuery, MethodResult<IList<LearningResultReportModel>>>
    {
        private readonly IMediator _mediator;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly ISenderService _senderService;
        private readonly IUnitRepository _unitRepository;

        public GetReportLearningResultQueryHandler(
            IMediator mediator,
            ICourseResultRepository courseResultRepository,
            IUnitResultRepository unitResultRepository,
            ICategoryRepository categoryRepository,
            ICourseModuleRepository courseModuleRepository,
            ISenderService senderService,
            IUnitRepository unitRepository)
        {
            _mediator = mediator;
            _courseResultRepository = courseResultRepository;
            _unitResultRepository = unitResultRepository;
            _categoryRepository = categoryRepository;
            _courseModuleRepository = courseModuleRepository;
            _senderService = senderService;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<IList<LearningResultReportModel>>> Handle(
            GetReportLearningResultQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LearningResultReportModel>>();
            var reports = new List<LearningResultReportModel>();
            var nowUtc = DateTime.UtcNow;

            var students = await GetStudentsAsync(request, methodResult, cancellationToken);
            if (students == null || !students.Any())
            {
                methodResult.Result = reports;
                return methodResult;
            }

            var program = await _categoryRepository.GetProgramLevelsAsync(request.ProgramId, cancellationToken);
            var studentIds = students.Select(x => x.Id).ToList();
            var userIds = students.Where(x => x.UserId.HasValue).Select(x => x.UserId!.Value).ToList();

            var mailHistories = await GetMailHistoriesAsync(userIds);

            var unitResultLookup = await GetUnitResultsAsync(
                studentIds,
                request.EndDate,
                cancellationToken);

            foreach (var student in students)
            {
                unitResultLookup.TryGetValue(student.Id, out var unitData);
                var levelName = program?.Levels.FirstOrDefault(x => x.Id == student.LevelId)?.Code;

                reports.Add(new LearningResultReportModel
                {
                    FullName = student.FullName,
                    UserName = student.UserName,
                    Email = student.Email,
                    LevelName = levelName,
                    StudentId = student.Id,
                    PhoneNumber = student.PhoneNumber,
                    SchoolName = student.School,
                    SchoolGrade = student.SchoolGrade,
                    SchoolClass = student.SchoolClass,
                    ProcessDate = student.CreatedDate,
                    ExpiredDate = student.ExpiredDate,
                    Status = student.ExpiredDate > nowUtc
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

            methodResult.Result = reports;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        #region Private methods

        private async Task<IList<HistorySendMailLearningProgressModel>> GetMailHistoriesAsync(
        List<Guid> userIds)
        {
            return (await _senderService.GetHistoriesSendMailLearningProgress(
                    new GetHistoriesSendMailLearningProgressModel
                    {
                        UserIds = userIds
                    })).Content?.Result ?? new List<HistorySendMailLearningProgressModel>();
        }

        private async Task<List<StudentDtoModel>?> GetStudentsAsync(
            GetReportLearningResultQuery request,
            MethodResult<IList<LearningResultReportModel>> methodResult,
            CancellationToken cancellationToken)
        {
            var studentResult = await _mediator.Send(new GetStudentReportQuery(request), cancellationToken);
            if (!studentResult.IsOK)
            {
                methodResult.AddError(studentResult.ErrorMessages);
                return null;
            }

            return studentResult.Result?.ToList() ?? new List<StudentDtoModel>();
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

        #endregion Private methods
    }
}