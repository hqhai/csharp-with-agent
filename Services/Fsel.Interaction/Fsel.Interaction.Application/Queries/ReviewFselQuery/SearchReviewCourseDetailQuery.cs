// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.ReviewFselQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.TrainingServices;
    using Fsel.Interaction.Application.Services.TrainingServices.Models;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.FselReviews;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReviewCourseDetailQuery : SearchReviewFselQueryModel, IRequest<MethodResult<StudentReviewSearchModel>>
    {
    }

    public class SearchReviewCourseDetailQueryHandler : IRequestHandler<SearchReviewCourseDetailQuery, MethodResult<StudentReviewSearchModel>>
    {
        private readonly IStudentReviewRepository _studentReviewRepository;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;
        private readonly ICourseService _courseService;

        public SearchReviewCourseDetailQueryHandler(IStudentReviewRepository studentReviewRepository, IUserService userService, ITrainingService trainingService, ICourseService courseService)
        {
            _studentReviewRepository = studentReviewRepository;
            _userService = userService;
            _trainingService = trainingService;
            _courseService = courseService;
        }

        public async Task<MethodResult<StudentReviewSearchModel>> Handle(SearchReviewCourseDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentReviewSearchModel>();

            if (request.PageSize > 100 || !request.CourseId.HasValue)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var courseResults = await _courseService.GetListCourseByIds(new List<Guid> { request.CourseId.Value });
            if (!courseResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError), nameof(courseResults));
                return methodResult;
            }
            var course = courseResults?.Content?.Result?.FirstOrDefault();
            if (course == null)
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var query = _studentReviewRepository.Queryable.Include(x => x.StudentReviewDetails)
                .Where(x => x.ReviewType == EnumReviewType.Course && x.CourseId == request.CourseId).Select(x => new StudentReviewTypeModel
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    CreatedFullName = x.CreatedFullName,
                    CreatedUserId = x.CreatedUserId,
                    CourseId = x.CourseId,
                    ReviewType = x.ReviewType,
                    StudentId = x.StudentId,
                    Stars = x.StudentReviewDetails.Average(x => x.VoteStars),
                    StudentReviewQuestionTypes = x.StudentReviewDetails.Select(x => new StudentReviewQuestionTypeModel
                    {
                        Id = x.Id,
                        Content = x.Content,
                        ReviewQuestionType = x.ReviewQuestionType,
                        VoteStars = x.VoteStars,
                    }).ToList(),
                });
            if (request.NumberOfStars != null)
            {
                query = query.Where(x => x.Stars >= request.NumberOfStars && x.Stars < request.NumberOfStars + 0.5);
            }
            var result = await query.ToListAsync(cancellationToken);
            var stars = NumberHelper.ConvertDoubleDecimal(result.Average(x => x.Stars));
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var studentIds = lists.Select(x => x.StudentId).ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults.Content?.Result;

            var classStudentResults = await _trainingService.GetClassByStudentIdsAsync(new GetClassListByStudentIdsModel { CourseId = request.CourseId, StudentIds = studentIds });
            var classeStudents = classStudentResults.Content?.Result;
            foreach (var item in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                item.Stars = NumberHelper.ConvertDoubleDecimal(item.Stars);
                var classStudent = classeStudents?.FirstOrDefault(x => x.StudentId == item.StudentId);
                item.Code = course.Code;
                item.CodeStudent = student?.Human?.Code;
                item.ClassCode = classStudent?.Code;
            }

            methodResult.Result = new StudentReviewSearchModel { Stars = stars, Code = course.Code, PagingItems = new PagingItemsModel<StudentReviewTypeModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
