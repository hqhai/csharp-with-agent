// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Application.Services.UserServices.Models;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Units;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.UnitQuery
{
    public class SearchUnitQuery : SearchUnitQueryModel, IRequest<MethodResult<PagingItemsModel<UnitSearchModel>>>
    {
    }

    public class SearchUnitQueryHandler : IRequestHandler<SearchUnitQuery, MethodResult<PagingItemsModel<UnitSearchModel>>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public SearchUnitQueryHandler(IMapper mapper, IUnitRepository unitRepository, IUserService userService)
        {
            _unitRepository = unitRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<UnitSearchModel>>> Handle(SearchUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<UnitSearchModel>> methodResult = new MethodResult<PagingItemsModel<UnitSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var unitQuery = _unitRepository.Queryable
                                    .Include(x => x.CourseUnitMockTests.Where(y => !y.IsDeleted))
                                    .Include(unit => unit.UnitLessons)
                                    .ThenInclude(unitLesson => unitLesson.Lesson)
                                    .ThenInclude(lesson => lesson!.LessonVideos)
                                    .ThenInclude(lessonVideo => lessonVideo.Video)
                                    .Where(x => x.CourseLevel == request.CourseLevel)

                            .Select(unit => new UnitSearchModel
                            {
                                Id = unit.Id,
                                Name = unit.Name,
                                Code = unit.Code,
                                IsActive = unit.CourseUnitMockTests.Any(),
                                CourseLevel = unit.CourseLevel,
                                CreatedDate = unit.CreatedDate,
                                CreatedUserId = unit.CreatedUserId,
                                UpdatedDate = unit.UpdatedDate,
                                UpdatedUserId = unit.UpdatedUserId,
                                TeacherIds = unit.UnitLessons.Select(l => l.Lesson)
                                                .SelectMany(lv => lv!.LessonVideos)
                                                .Select(v => v.Video)
                                                .Select(n => n!.TeacherId ?? Guid.Empty).Distinct().ToList(),
                            });

            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                unitQuery = unitQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            //Keyword
            if (request.TeacherId.HasValue)
            {
                unitQuery = unitQuery.Where(m => m.TeacherIds != null && m.TeacherIds.Contains(request.TeacherId.Value));
            }

            int totalItem = await unitQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await unitQuery.OrderByDescending(x => x.CreatedDate)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teachers = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = unitQuery.SelectMany(x => x.TeacherIds!).ToList() });
            if (teachers.IsSuccessStatusCode)
            {
                foreach (var item in lists)
                {
                    item.TeacherNames = teachers.Content?.Result?.Where(x => item.TeacherIds!.Contains(x.Id)).Select(x => x.Human?.FullName ?? string.Empty).ToList();
                }
            }

            methodResult.Result = new PagingItemsModel<UnitSearchModel>
            {
                Items = _mapper.Map<IEnumerable<UnitSearchModel>>(lists),
                PagingInfo = new PagingInfoModel { Page = request.Page, PageSize = request.PageSize, TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
