using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Application.Services;
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
            MethodResult<PagingItemsModel<UnitSearchModel>> methodResult = new MethodResult<PagingItemsModel<UnitSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var unitQuery = _unitRepository.Queryable
                                    .Include(x => x.CourseUnitMockTests.Where(y => !y.IsDeleted))
                                    .Where(x => request.CourseLevel == null || x.CourseLevel == request.CourseLevel)
                                    .Where(x => request.CourseLevel == null || x.CourseLevel == request.CourseLevel)
                                    .Include(unit => unit.UnitLessons)
                                    .ThenInclude(unitLesson => unitLesson.Lesson)
                                    .ThenInclude(lesson => lesson.LessonVideos)
                                    .ThenInclude(lessonVideo => lessonVideo.Video)
                                    .Where(x => request.TeacherId == null || x.UnitLessons.Select(l => l.Lesson)
                                                                                   .SelectMany(lv => lv.LessonVideos)
                                                                                   .Select(v => v.Video)
                                                                                   .Select(n => n.TeacherId).Contains(request.TeacherId.Value))

                            .Select(unit => new UnitSearchModel
                            {
                                Id = unit.Id,
                                Name = unit.Name,
                                DisplayName = unit.DisplayName,
                                IsActive = !unit.CourseUnitMockTests.Any(),
                                Type = unit.Type,
                                CourseLevel = unit.CourseLevel,
                                CreatedDate = unit.CreatedDate,
                                CreatedUserId = unit.CreatedUserId,
                                UpdatedDate = unit.UpdatedDate,
                                UpdatedUserId = unit.UpdatedUserId,
                                TeacherId = unit.UnitLessons.Select(l => l.Lesson)
                                                .SelectMany(lv => lv.LessonVideos)
                                                .Select(v => v.Video)
                                                .Select(n => n.TeacherId).FirstOrDefault(),
                            });

            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                unitQuery = unitQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            int totalItem = await unitQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await unitQuery.OrderByDescending(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var teachers = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = unitQuery.Select(x => x.TeacherId ?? Guid.Empty).ToList() });
            if (teachers.IsSuccessStatusCode)
            {
                foreach (var item in lists)
                {
                    item.TeacherName = teachers.Content?.Result?.FirstOrDefault(x => x.Id == item.TeacherId)?.Human.FullName;
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
