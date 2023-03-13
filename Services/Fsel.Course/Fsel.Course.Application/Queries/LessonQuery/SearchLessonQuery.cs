using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Lessons;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.LessonQuery
{
    public class SearchLessonQuery : SearchLessonQueryModel, IRequest<MethodResult<PagingItemsModel<LessonModel>>>
    {
    }

    public class SearchLessonQueryHandler : IRequestHandler<SearchLessonQuery, MethodResult<PagingItemsModel<LessonModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;

        public SearchLessonQueryHandler(IMapper mapper, ILessonRepository lessonRepository)
        {
            _mapper = mapper;
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<PagingItemsModel<LessonModel>>> Handle(SearchLessonQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<LessonModel>> methodResult = new MethodResult<PagingItemsModel<LessonModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var lessonQuery = from i in _lessonRepository.Queryable
                              select new LessonModel
                              {
                                  Id = i.Id,
                                  Name = i.Name,
                                  DisplayName = i.DisplayName,
                                  InstructionContent = i.InstructionContent,
                                  IsActive = i.IsActive,
                                  TeacherId = i.TeacherId,
                                  CourseLevel = i.CourseLevel,
                                  CreatedDate = i.CreatedDate,
                                  CreatedUserId = i.CreatedUserId,
                                  UpdatedDate = i.UpdatedDate,
                                  UpdatedUserId = i.UpdatedUserId,
                              };
            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                lessonQuery = lessonQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            int totalItem = await lessonQuery.CountAsync().ConfigureAwait(false);
            var lists = await lessonQuery.OrderByDescending(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<LessonModel>
            {
                Items = _mapper.Map<IEnumerable<LessonModel>>(lists),
                PagingInfo = new PagingInfoModel { Page = request.Page, PageSize = request.PageSize, TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
