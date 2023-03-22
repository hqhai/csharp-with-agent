using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Courses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Queries
{
    public class SearchCourseQuery : SearchCourseQueryModel, IRequest<MethodResult<PagingItemsModel<CourseSearchLMSModel>>>
    {
    }

    public class SearchCourseQueryHandler : IRequestHandler<SearchCourseQuery, MethodResult<PagingItemsModel<CourseSearchLMSModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public SearchCourseQueryHandler(IMapper mapper, ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<CourseSearchLMSModel>>> Handle(SearchCourseQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<CourseSearchLMSModel>> methodResult = new MethodResult<PagingItemsModel<CourseSearchLMSModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var courseQuery = _courseRepository.Queryable
                              .Where(x => request.CourseLevel == null || x.CourseLevel == request.CourseLevel)
                              .Select(course => new CourseSearchLMSModel
                              {
                                  Id = course.Id,
                                  Name = course.Name,
                                  CourseLevel = course.CourseLevel,
                              });

            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                courseQuery = courseQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            int totalItem = await courseQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await courseQuery.OrderByDescending(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<CourseSearchLMSModel>
            {
                Items = _mapper.Map<IEnumerable<CourseSearchLMSModel>>(lists),
                PagingInfo = new PagingInfoModel { Page = request.Page, PageSize = request.PageSize, TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public MethodResult<PagingItemsModel<CourseSearchModel>> Handle(SearchCourseQuery message)
        {
            throw new NotImplementedException();
        }
    }
}
