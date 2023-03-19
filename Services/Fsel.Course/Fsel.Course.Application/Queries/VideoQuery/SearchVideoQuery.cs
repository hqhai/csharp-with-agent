using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
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
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly ITimeCodeExcerciseRepository _timeCodeExcerciseRepository;
        private readonly IExcerciseRepository _excerciseRepository;

        public SearchVideoQueryHandler(IMapper mapper
            , IVideoRepository videoRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , ITimeCodeExcerciseRepository timeCodeExcerciseRepository
            , IExcerciseRepository excerciseRepository)
        {
            _mapper = mapper;
            _videoRepository = videoRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _timeCodeExcerciseRepository = timeCodeExcerciseRepository;
            _excerciseRepository = excerciseRepository;
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
                             .Include(video => video.VideoTimeCodes
                             .Where(x => request.TimeCodeType == null ? false : x.TimeCodeType == request.TimeCodeType))
                                .ThenInclude(videoTimeCode => videoTimeCode.TimeCodeExcercises)
                                    .ThenInclude(timeCodeExcercise => timeCodeExcercise.Excercise)
                                    .Where(x => request.Level == null ? false : x.CourseLevel == request.Level)
                                    .Where(x => request.TeacherId == null ? false : x.TeacherId == request.TeacherId)
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
