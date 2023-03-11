using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Common.Models.Queries;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.CourseQuery
{
    public class SearchCourseQuery : SearchCourseQueryModel, IRequest<MethodResult<PagingItemsModel<CourseModel>>>
    {
    }

    public class SearchCourseQueryHandler : IRequestHandler<SearchCourseQuery, MethodResult<PagingItemsModel<CourseModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public SearchCourseQueryHandler(IMapper mapper, ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<CourseModel>>> Handle(SearchCourseQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<CourseModel>> methodResult = new MethodResult<PagingItemsModel<CourseModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var courseQuery = from i in _courseRepository.Queryable
                              select new CourseModel
                              {
                                  Id = i.Id,
                                  Name = i.Name,
                                  NumberOfLessons = i.NumberOfLessons,
                                  NumberOfUnits = i.NumberOfUnits,
                                  IsPublish = i.IsPublish,
                                  CourseLevel = i.CourseLevel,
                                  CreatedDate = i.CreatedDate,
                                  CreatedUserId = i.CreatedUserId,
                                  CreatedFullName = i.CreatedFullName,
                                  UpdatedDate = i.UpdatedDate,
                                  UpdatedUserId = i.UpdatedUserId,
                                  UpdatedFullName = i.UpdatedFullName,
                              };
            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                courseQuery = courseQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            int totalItem = await courseQuery.CountAsync().ConfigureAwait(false);
            var lists = await courseQuery.OrderByDescending(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<CourseModel>
            {
                Items = _mapper.Map<IEnumerable<CourseModel>>(lists),
                PagingInfo = new PagingInfoModel { Page = request.Page, PageSize = request.PageSize, TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}