// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Application.Services.UserServices.Models;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Lessons;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.LessonQuery
{
    public class SearchLessonQuery : SearchLessonQueryModel, IRequest<MethodResult<PagingItemsModel<LessonSearchModel>>>
    {
    }

    public class SearchLessonQueryHandler : IRequestHandler<SearchLessonQuery, MethodResult<PagingItemsModel<LessonSearchModel>>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IUserService _userService;

        public SearchLessonQueryHandler(ILessonRepository lessonRepository, IUserService userService
            )
        {
            _lessonRepository = lessonRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<LessonSearchModel>>> Handle(SearchLessonQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<LessonSearchModel>> methodResult = new MethodResult<PagingItemsModel<LessonSearchModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var lessonQuery = _lessonRepository.Queryable.Where(p => !p.IsArchive)
                     .Include(x => x.UnitLessons.Where(y => !y.IsDeleted))
                     .Include(x => x.LessonVideos.Where(y => !y.IsDeleted && y.Video != null))
                     .ThenInclude(x => x.Video)
                     .ThenInclude(x => x!.VideoTimeCodes.Where(y => !y.IsDeleted && y.Video != null))
                     .AsNoTracking()
                     .Select(x => new LessonSearchModel
                     {
                         Id = x.Id,
                         Name = x.Name,
                         CourseLevel = x.CourseLevel,
                         TeacherId = x.LessonVideos.Where(y => y.Video != null)
                                                      .Select(y => y.Video).Select(x => x!.TeacherId).FirstOrDefault(),
                         TimeCodeType = x.LessonVideos.Where(y => y.Video != null)
                                                      .Select(y => y.Video)
                                                      .SelectMany(y => y!.VideoTimeCodes.Where(y => !y.IsDeleted))
                                                      .OrderByDescending(x => x.TimeCodeType)
                                                      .Reverse()
                                                      .Select(y => y.TimeCodeType)
                                                      .FirstOrDefault(),
                         CreatedFullName = x.CreatedFullName,
                         UpdatedUserId = x.UpdatedUserId,
                         CreatedDate = x.CreatedDate,
                         UpdatedDate = x.UpdatedDate,
                         UpdatedFullName = x.UpdatedFullName,
                         IsActive = x.UnitLessons.Any()
                     });

            request.Keyword = request.Keyword?.Trim().ToLower(CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    lessonQuery = lessonQuery.Where(m => m.Id == guid);
                }
                else
                {
                    lessonQuery = lessonQuery.Where(m => m.Name != null && m.Name.Contains(request.Keyword));
                }
            }

            if (request.TeacherId != null)
            {
                lessonQuery = lessonQuery.Where(m => m.TeacherId == request.TeacherId);
            }

            if (request.CourseLevel != null)
            {
                lessonQuery = lessonQuery.Where(m => m.CourseLevel == request.CourseLevel);
            }

            if (request.TimeCodeType != null)
            {
                lessonQuery = lessonQuery.Where(m => m.TimeCodeType == request.TimeCodeType);
            }

            int totalItem = await lessonQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await lessonQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lessonQuery.Select(x => x.TeacherId ?? Guid.Empty).ToList() });
            if (teacherResults.IsSuccessStatusCode)
            {
                var teachers = teacherResults.Content?.Result;
                foreach (var item in lists)
                {
                    item.TeacherName = teachers?.FirstOrDefault(x => x.Id == item.TeacherId)?.User?.FullName;
                }
            }

            methodResult.Result = new PagingItemsModel<LessonSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
