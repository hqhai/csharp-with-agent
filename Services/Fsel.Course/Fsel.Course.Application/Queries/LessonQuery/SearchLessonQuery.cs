// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
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
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IUserService _userService;

        public SearchLessonQueryHandler(IMapper mapper
            , ILessonRepository lessonRepository
            , IUserService userService
            )
        {
            _mapper = mapper;
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

            var lessonQuery = _lessonRepository.Queryable
                     .Include(x => x.LessonVideos.Where(y => !y.IsDeleted && y.Video != null))
                     .ThenInclude(x => x.Video)
                     .ThenInclude(x => x!.VideoTimeCodes.Where(y => !y.IsDeleted && y.Video != null))
                     .Where(x => !request.CourseLevel.HasValue || x.CourseLevel == request.CourseLevel)
                     .Where(x => !request.TimeCodeType.HasValue || x.LessonVideos.Where(y => y.Video != null)
                                                                                  .Select(y => y.Video)
                                                                                  .SelectMany(y => y!.VideoTimeCodes)
                                                                                  .Select(y => y.TimeCodeType)
                                                                                  .Contains(request.TimeCodeType.Value))

                     .AsNoTracking()
                     .Select(x => new LessonSearchModel
                     {
                         Id = x.Id,
                         Name = x.Name,
                         CourseLevel = x.CourseLevel,
                         TimeCodeType = x.LessonVideos.Where(y => y.Video != null)
                                                      .Select(y => y.Video)
                                                      .SelectMany(y => y!.VideoTimeCodes)
                                                      .OrderByDescending(x => x.TimeCodeType)
                                                      .Reverse()
                                                      .Select(y => y.TimeCodeType)
                                                      .FirstOrDefault(),
                         CreatedFullName = x.CreatedFullName,
                         UpdatedUserId = x.UpdatedUserId,
                         CreatedDate = x.CreatedDate,
                         UpdatedDate = x.UpdatedDate,
                         UpdatedFullName = x.UpdatedFullName,
                         IsActive = !x.LessonVideos.Any()
                     });

            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                lessonQuery = lessonQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            int totalItem = await lessonQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await lessonQuery.OrderByDescending(x => x.CreatedDate)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teachers = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lessonQuery.Select(x => x.TeacherId ?? Guid.Empty).ToList() });
            if (teachers.IsSuccessStatusCode)
            {
                foreach (var item in lists)
                {
                    item.TeacherName = teachers.Content?.Result?.FirstOrDefault(x => x.Id == item.TeacherId)?.Human?.FullName;
                }
            }

            methodResult.Result = new PagingItemsModel<LessonSearchModel>
            {
                Items = _mapper.Map<IEnumerable<LessonSearchModel>>(lists),
                PagingInfo = new PagingInfoModel { Page = request.Page, PageSize = request.PageSize, TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
