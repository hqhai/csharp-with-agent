// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
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
        private readonly ITrainingService _trainingService;

        public SearchReviewLessonDetailQueryHandler(IUserService userService, ILessonRepository lessonRepository, ILessonResultRepository lessonResultRepository, ITrainingService trainingService)
        {
            _userService = userService;
            _lessonRepository = lessonRepository;
            _lessonResultRepository = lessonResultRepository;
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
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var query = _lessonResultRepository.Queryable.Include(x => x.VideoResult)
                                                        .Where(x => x.VideoResult != null && x.LessonId == request.LessonId && x.Status == EnumResultStatus.Done)
                                                        .Select(x => new ReviewLessonDetailModel
                                                        {
                                                            Id = x.Id,
                                                            StudentId = x.StudentId,
                                                            CreatedDate = x.CreatedDate,
                                                            Stars = x.VideoResult != null ? x.VideoResult.NumberOfStars : default
                                                        });

            var result = await query.ToListAsync(cancellationToken);
            var stars = NumberHelper.ConvertDoubleDecimal(result.Average(x => x.Stars));
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var studentIds = lists.Select(x => x.StudentId).Distinct().ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults.Content?.Result;

            var classStudentResults = await _trainingService.GetClassByStudentIdsAsync(new GetClassListByStudentIdsModel { StudentIds = studentIds });
            var classeStudents = classStudentResults.Content?.Result;
            foreach (var item in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                var classStudent = classeStudents?.FirstOrDefault(x => x.StudentId == item.StudentId);
                item.Code = student?.Human?.Code;
                item.ClassCode = classStudent?.Code;
            }
            methodResult.Result = new ReviewLessonDetailSearchModel { Stars = stars, Name = lesson.Name, PagingItemsModel = new PagingItemsModel<ReviewLessonDetailModel>(lists, request, totalItem) };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
