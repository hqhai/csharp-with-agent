// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReviewFselQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ReviewFsels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchReviewTeacherRatingQuery : SearchReviewTeacherRatingQueryModel, IRequest<MethodResult<PagingItemsModel<ReviewTeacherRatingSearchModel>>>
    {
    }

    public class SearchReviewTeacherRatingQueryHandler : IRequestHandler<SearchReviewTeacherRatingQuery, MethodResult<PagingItemsModel<ReviewTeacherRatingSearchModel>>>
    {
        private readonly IUserService _userService;
        private readonly IVideoRepository _videoRepository;

        public SearchReviewTeacherRatingQueryHandler(IUserService userService, IVideoRepository videoRepository)
        {
            _userService = userService;
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ReviewTeacherRatingSearchModel>>> Handle(SearchReviewTeacherRatingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ReviewTeacherRatingSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var videos = await _videoRepository.Queryable.Include(x => x.VideoResults).ToListAsync(cancellationToken);
            var teacherIds = videos.Select(x => x.TeacherId).Distinct().ToList();
            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = teacherIds });
            if (!teacherResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var teachers = teacherResults.Content?.Result;

            var teacherRatingQuery = teachers?.OrderBy(x => x.CreatedDate).Select(x => new ReviewTeacherRatingSearchModel
            {
                Id = x.Id,
                Code = x.Human!.Code,
                CreatedDate = x.CreatedDate,
                FullName = x.Human.FullName,
            }).AsEnumerable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                teacherRatingQuery = teacherRatingQuery?.Where(m => m.Id.ToString() == request.Keyword || (m.FullName != null && m.FullName.Contains(request.Keyword, StringComparison.CurrentCulture)));
            }

            int totalItem = teacherRatingQuery!.Count();
            var lists = teacherRatingQuery!.Skip((request!.Page - 1) * request!.PageSize).Take(request!.PageSize).ToList();

            foreach (var item in lists)
            {
                var video = videos.FirstOrDefault(x => x.Id == item.Id);
                item.Scores = video?.VideoResults.Average(x => x.NumberOfStars) ?? 0;
            }

            methodResult.Result = new PagingItemsModel<ReviewTeacherRatingSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
