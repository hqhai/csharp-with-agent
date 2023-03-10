using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Common.Models.Queries;
using Fsel.Course.Common.Models.Queries.Video;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Application.Queries.VideoQuery
{
    public class SearchVideoQuery : SearchVideoQueryModel, IRequest<MethodResult<PagingItemsModel<VideoModel>>>
    {
    }

    public class SearchVideoQueryHandler : IRequestHandler<SearchVideoQuery, MethodResult<PagingItemsModel<VideoModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IVideoRepository _videoRepository;

        public SearchVideoQueryHandler(IMapper mapper, IVideoRepository videoRepository)
        {
            _mapper = mapper;
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<PagingItemsModel<VideoModel>>> Handle(SearchVideoQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<VideoModel>> methodResult = new MethodResult<PagingItemsModel<VideoModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var VideoQuery = from i in _videoRepository.Queryable
                             select new VideoModel
                             {
                                 Id = i.Id,
                                 Name = i.Name,
                                 IsActive = i.IsActive,
                                 CourseLevel = i.CourseLevel,
                                 CreatedDate = i.CreatedDate,
                                 CreatedUserId = i.CreatedUserId,
                                 UpdatedDate = i.UpdatedDate,
                                 UpdatedUserId = i.UpdatedUserId,
                             };

            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                VideoQuery = VideoQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }

            int totalItem = await VideoQuery.CountAsync().ConfigureAwait(false);
            var lists = await VideoQuery.OrderByDescending(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<VideoModel>
            {
                Items = _mapper.Map<IEnumerable<VideoModel>>(lists),
                PagingInfo = new PagingInfoModel { Page = request.Page, PageSize = request.PageSize, TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}