// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReviewLessonDetailQuery : SearchReviewLessonDetailQueryModel, IRequest<MethodResult<ReviewLessonDetailSearchModel>>
    {
    }

    public class SearchReviewLessonDetailQueryHandler : IRequestHandler<SearchReviewLessonDetailQuery, MethodResult<ReviewLessonDetailSearchModel>>
    {
        private readonly IUserService _userService;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ITrainingService _trainingService;

        public SearchReviewLessonDetailQueryHandler(IUserService userService, ILessonRepository lessonRepository, ILessonResultRepository lessonResultRepository, IVideoResultRepository videoResultRepository, ITrainingService trainingService)
        {
            _userService = userService;
            _lessonRepository = lessonRepository;
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<ReviewLessonDetailSearchModel>> Handle(SearchReviewLessonDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ReviewLessonDetailSearchModel>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var lesson = await _lessonRepository.GetByIdAsync(request.LessonId);
            if (lesson == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var lessonResultIds = await _lessonResultRepository.Queryable.Where(x => x.LessonId == request.LessonId && x.UnitId == request.UnitId && x.CourseId == request.CourseId).Select(x => x.Id).ToListAsync(cancellationToken);
            var videoResults = await _videoResultRepository.Queryable.Where(x => lessonResultIds.Contains(x.LessonResultId) && x.Status == EnumResultStatus.Done)
                                                       .OrderByDescending(x => x.CreatedDate)
                                                       .ToListAsync(cancellationToken);
            var studentIds = videoResults.Select(x => x.StudentId).Distinct().ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults.Content?.Result;

            var classStudentResults = await _trainingService.GetClassByStudentIdsAsync(new GetClassListByStudentIdsModel { StudentIds = studentIds });
            var classeStudents = classStudentResults.Content?.Result;
            var query = videoResults.Select(x => GetReview(students, classeStudents, x));
            if (request.NumberOfStars != null)
            {
                query = query.Where(x => x.Stars + 0.5 >= request.NumberOfStars && x.Stars < request.NumberOfStars + 0.5);
            }
            query = query.ToList();
            int totalItem = query.Count();
            var stars = query.Any() ? NumberHelper.ConvertRound(query.Average(x => x.Stars)) : default;
            var lists = query.ApplySortAndPaging(request).ToList();

            foreach (var item in lists)
            {
                item.Stars = NumberHelper.ConvertRound(item.Stars);
            }
            methodResult.Result = new ReviewLessonDetailSearchModel { Stars = stars, Name = lesson.Name, PagingItemsModel = new PagingItemsModel<ReviewLessonDetailModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static ReviewLessonDetailModel GetReview(IList<StudentModel>? students, IList<ClassStudentModel>? classeStudents, VideoResult x)
        {
            var student = students?.FirstOrDefault(y => y.Id == x.StudentId);
            var classStudent = classeStudents?.FirstOrDefault(y => y.StudentId == x.StudentId);
            return new ReviewLessonDetailModel
            {
                Id = x.Id,
                Code = student?.User?.Code,
                FullName = student?.User?.FullName,
                ClassCode = classStudent?.Code,
                StudentId = x.StudentId,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate,
                Feedback = x.Feedback,
                Stars = x.NumberOfStars
            };
        }
    }
}
