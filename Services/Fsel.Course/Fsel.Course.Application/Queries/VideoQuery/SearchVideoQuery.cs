using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Videos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.VideoQuery
{
    public class SearchVideoQuery : SearchVideoQueryModel, IRequest<MethodResult<PagingItemsModel<VideoSearchModel>>>
    {
    }

    public class SearchVideoQueryHandler : IRequestHandler<SearchVideoQuery, MethodResult<PagingItemsModel<VideoSearchModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IVideoRepository _videoRepository;

        public SearchVideoQueryHandler(IMapper mapper
            , IVideoRepository videoRepository)
        {
            _mapper = mapper;
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<PagingItemsModel<VideoSearchModel>>> Handle(SearchVideoQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<VideoSearchModel>> methodResult = new MethodResult<PagingItemsModel<VideoSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var videoQuery = _videoRepository.Queryable
                        .Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                        .Include(video => video.VideoTimeCodes.Where(x => !x.IsDeleted))
                        .ThenInclude(videoTimeCode => videoTimeCode.TimeCodeExcercises.Where(x => !x.IsDeleted && x.Excercise != null))
                        .ThenInclude(timeCodeExcercise => timeCodeExcercise.Excercise)
                        .Where(x => x.Type == EnumVideoType.Lesson)
                        .Where(x => !request.Level.HasValue || x.CourseLevel == request.Level.Value)
                        .Where(x => !request.TeacherId.HasValue || x.TeacherId == request.TeacherId.Value)
                        .Where(x => request.TimeCodeType == null || x.VideoTimeCodes.Select(n => n.TimeCodeType).Contains(request.TimeCodeType.Value))
                        .Select(video => new VideoSearchModel
                        {
                            Id = video.Id,
                            Name = video.Name,
                            IsActive = !video.LessonVideos.Any(),
                            CourseLevel = video.CourseLevel,
                            CreatedDate = video.CreatedDate,
                            CreatedFullName = video.CreatedFullName,
                            UpdatedDate = video.UpdatedDate,
                            UpdatedFullName = video.UpdatedFullName,
                            Excercises = video.VideoTimeCodes.SelectMany(videoTimeCode => videoTimeCode.TimeCodeExcercises)
                                                                     .Select(timeCodeExcercise => timeCodeExcercise.Excercise)
                                                                     .GroupBy(excercise => excercise.CourseSkill)
                                                                     .OrderByDescending(courseSkillGroup => courseSkillGroup.Count())
                                                                     .Select(courseSkillGroup => new VideoExcerciseSearchModel
                                                                     {
                                                                         CourseSkill = courseSkillGroup.Key,
                                                                         Count = courseSkillGroup.Count()
                                                                     }).ToList()
                        });

            //Keyword
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                videoQuery = videoQuery.Where(m => m.Id.ToString() == request.Keyword
                                        || (m.Name ?? string.Empty).Contains(request.Keyword)
                                        );
            }

            int totalItem = await videoQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await videoQuery.OrderByDescending(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<VideoSearchModel>
            {
                Items = _mapper.Map<IEnumerable<VideoSearchModel>>(lists),
                PagingInfo = new PagingInfoModel
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalItems = totalItem
                }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
