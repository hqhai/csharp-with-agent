using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
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

            if (request == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            else if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var videoQuery = _videoRepository.Queryable
                        .Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                        .Include(video => video.VideoTimeCodes.Where(x => !x.IsDeleted))
                        .ThenInclude(videoTimeCode => videoTimeCode.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                        .ThenInclude(timeCodeExercise => timeCodeExercise.Exercise)
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
                            Exercises = video.VideoTimeCodes.SelectMany(videoTimeCode => videoTimeCode.TimeCodeExercises)
                                                                     .Select(timeCodeExercise => timeCodeExercise.Exercise)
                                                                     .GroupBy(excercise => excercise.CourseSkill)
                                                                     .OrderByDescending(courseSkillGroup => courseSkillGroup.Count())
                                                                     .Select(courseSkillGroup => new VideoExerciseSearchModel
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
            var lists = await videoQuery.OrderByDescending(x => x.CreatedDate)
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
