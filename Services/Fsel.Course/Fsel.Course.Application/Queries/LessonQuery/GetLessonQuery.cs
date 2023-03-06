using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Common.Models.Queries;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Application.Queries.LessonQuery
{
    public class GetLessonQuery : IRequest<MethodResult<PagingItemsModel<LessonModel>>>
    {
        public Guid? Id { get; set; }
    }
    public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery,MethodResult<PagingItemsModel<LessonModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;

        public GetLessonQueryHandler(IMapper mapper, ILessonRepository lessonRepository)
        {
            _mapper = mapper;
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<PagingItemsModel<LessonModel>>> Handle(GetLessonQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<LessonModel>> methodResult = new MethodResult<PagingItemsModel<LessonModel>>();

            if (request.Id == null)
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
            if (!string.IsNullOrEmpty(request.Id.ToString()) && request.Id.ToString()!=null)
            {
                lessonQuery = lessonQuery.Where(m => m.Id == request.Id || (m.Name ?? string.Empty).Contains(request.Id.Value.ToString()));
            }

            int totalItem = await lessonQuery.CountAsync().ConfigureAwait(false);
            var lists = await lessonQuery.OrderByDescending(x => x.Id)
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<LessonModel>
            {
                Items = _mapper.Map<IEnumerable<LessonModel>>(lists),
                PagingInfo = new PagingInfoModel { TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
