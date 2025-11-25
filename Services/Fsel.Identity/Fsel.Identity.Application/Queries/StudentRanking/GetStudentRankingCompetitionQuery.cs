// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentRanking

{
    using System.Linq.Dynamic.Core;

    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;

    public class GetStudentRankingCompetitionQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<StudentRankingModel>>>
    {
        public EnumCourseType CourseType { get; set; }
    }

    public class GetStudentRankingCompetitionQueryHandler : IRequestHandler<GetStudentRankingCompetitionQuery, MethodResult<PagingItemsModel<StudentRankingModel>>>
    {
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IHostEnvironment _environment;
        private readonly IStudentRepository _studentRepository;
        private const double Process_Ratio = 0.75;
        private const double Overall_Ratio = 0.25;
        private DateTime _expiredCompetition = new DateTime(2024, 6, 14, 16, 59, 0); // thời điểm khóa leaderboard

        public GetStudentRankingCompetitionQueryHandler(ILmsCourseService lmsCourseService, IHostEnvironment environment, IStudentRepository studentRepository)
        {
            _lmsCourseService = lmsCourseService;
            _environment = environment;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<PagingItemsModel<StudentRankingModel>>> Handle(GetStudentRankingCompetitionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<StudentRankingModel>> methodResult = new MethodResult<PagingItemsModel<StudentRankingModel>>();



            if (DateTime.UtcNow > _expiredCompetition)
            {
                string fileResult = request.CourseType == EnumCourseType.Academic ? ResourceSettings.AcademicStudentsResult : ResourceSettings.IeltsStudentResult;
                string pathResult = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileResult);
                var finalResult = ConvertHelper.DeserializeFromFilePath<List<StudentRankingModel>>(pathResult);

                int totalResult = finalResult.Count();
                var listsResult = finalResult.ApplyPaging(request).ToList();
                methodResult.Result = new PagingItemsModel<StudentRankingModel>(listsResult, request, totalResult);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.AcademicStudentsName);
            if (_environment.IsProduction() && request.CourseType == EnumCourseType.Academic)
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.AcademicStudentsName);
            }
            else if (_environment.IsStaging() && request.CourseType == EnumCourseType.Academic)
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.AcademicStudentsStagingName);
            }
            else if (_environment.IsProduction() && request.CourseType == EnumCourseType.Ielts)
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.IeltsStudentsName);
            }
            else if (_environment.IsStaging() && request.CourseType == EnumCourseType.Ielts)
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.IeltsStudentsStagingName);
            }

            var listStudentCompetition = ConvertHelper.DeserializeFromFilePath<IList<StudentJoinCompetitionModel>>(path);
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

            var result = from studentFile in listStudentCompetion
                         where studentFile != null
                         join studentResult in studentResults! on studentFile.StudentId equals studentResult.StudentId into resultGroup
                         from studentResult in resultGroup.DefaultIfEmpty()
                         join studentInfo in studentInfos on studentFile.StudentId equals studentInfo.Id into infoGroup
                         from studentInfo in infoGroup.DefaultIfEmpty()
                         where studentInfo != null
                         select new StudentRankingModel
                         {
                             StudentId = studentFile.StudentId,
                             SchoolName = studentFile.SchoolName,
                             Grade = studentFile.Grade.ToString(),
                             Process = studentResult?.ContentCompleted ?? 0, // Thêm kiểm tra null và mặc định giá trị nếu null
                             OverallScore = studentResult?.TotalScore ?? 0, // Thêm kiểm tra null và mặc định giá trị nếu null
                             CompetitionEndDate = new DateTime(2024, 6, 15),
                             FullName = studentFile.FullName,
                             AvatarPath = studentInfo?.User?.AvatarPath ?? string.Empty, // Thêm kiểm tra null và mặc định giá trị nếu null
                             UserId = studentFile.UserId,
                             RankingScore = Process_Ratio * studentResult?.ContentCompleted + Overall_Ratio * studentResult?.TotalScore
                         };

            result = result.OrderByDescending(x => (Process_Ratio * x.Process + Overall_Ratio * x.OverallScore));
            int totalItem = result.Count();
            var lists = result.ApplyPaging(request).ToList();
            methodResult.Result = new PagingItemsModel<StudentRankingModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
