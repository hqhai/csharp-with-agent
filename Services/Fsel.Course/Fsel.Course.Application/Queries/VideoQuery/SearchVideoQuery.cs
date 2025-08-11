// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.Videos;
using Fsel.Course.Infrastructure.Common;
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
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ILessonVideoRepository _lessonVideoRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private const int LengthKeyword = 300;

        public SearchVideoQueryHandler(IVideoRepository videoRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            ITimeCodeExerciseRepository timeCodeExerciseRepository,
            IExerciseRepository exerciseRepository,
            ISkillRepository skillRepository,
            ILevelRepository levelRepository,
            IVideoResultRepository videoResultRepository,
            ILessonVideoRepository lessonVideoRepository,
            ILessonModuleRepository lessonModuleRepository)
        {
            _videoRepository = videoRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _exerciseRepository = exerciseRepository;
            _skillRepository = skillRepository;
            _levelRepository = levelRepository;
            _videoResultRepository = videoResultRepository;
            _lessonVideoRepository = lessonVideoRepository;
            _lessonModuleRepository = lessonModuleRepository;
        }

        public async Task<MethodResult<PagingItemsModel<VideoSearchModel>>> Handle(SearchVideoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<VideoSearchModel>> methodResult = new MethodResult<PagingItemsModel<VideoSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.Keyword) && !request.Keyword.IsValidCode())
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.InvalidKeywordCharacter), request.Keyword);
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.Keyword) && request.Keyword.Length > LengthKeyword)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.MaxLength), nameof(request.Keyword), LengthKeyword);
                return methodResult;
            }

            var queryTimeCode = _videoTimeCodeRepository.Queryable;
            if (request.TimeCodeType.HasValue)
            {
                queryTimeCode = queryTimeCode.Where(x => x.TimeCodeType == request.TimeCodeType);
            }

            var queryVideo = _videoRepository.Queryable;

            var videoChildQuery = _videoRepository.Queryable.Where(v => v.OriginalId.HasValue && v.VersionStatus == EnumVersionStatus.LastVersion);

            // Query video cha KHÔNG có con
            var videoParentNoChild = from parent in _videoRepository.Queryable
                                     where !parent.OriginalId.HasValue
                                     join child in _videoRepository.Queryable
                                         on parent.Id equals child.OriginalId into childGroup
                                     from child in childGroup.DefaultIfEmpty()
                                     where child == null
                                     select parent;

            // Hợp nhất cha-không-con + tất cả con
            queryVideo = videoChildQuery.Union(videoParentNoChild);
            request.Keyword = request.Keyword?.Trim().ToLower(CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    queryVideo = queryVideo.Where(m => m.Id == guid);
                }
                else
                {
                    queryVideo = queryVideo.Where(m => m.Name!.Contains(request.Keyword));
                }
            }
            if (request.TeacherId.HasValue)
            {
                queryVideo = queryVideo.Where(x => x.TeacherId == request.TeacherId);
            }
            if (request.ProgramId.HasValue)
            {
                queryVideo = queryVideo.Where(x => x.ProgramId == request.ProgramId);
            }
            if (request.LevelId.HasValue)
            {
                queryVideo = queryVideo.Where(x => x.LevelId == request.LevelId);
            }
            if (request.CourseLevel.HasValue)
            {
                queryVideo = queryVideo.Where(x => x.CourseLevel == request.CourseLevel);
            }
            queryVideo = from video in queryVideo
                         join vtc in queryTimeCode on video.Id equals vtc.VideoId
                         select video;

            queryVideo = queryVideo.Distinct();

            var query = from baseQ in queryVideo
                        join level in _levelRepository.Queryable on baseQ.LevelId equals level.Id into levelJoin
                        from level in levelJoin.DefaultIfEmpty()
                        join videoResultGroup in _videoResultRepository.Queryable on baseQ.Id equals videoResultGroup.VideoId into videoResults
                        select new VideoSearchModel
                        {
                            Id = baseQ.Id,
                            CourseLevel = baseQ.CourseLevel,
                            CreatedDate = baseQ.CreatedDate,
                            CreatedFullName = baseQ.CreatedFullName,
                            CreatedUserId = baseQ.CreatedUserId,
                            Name = baseQ.Name,
                            SubFilePath = baseQ.SubFilePath,
                            UpdatedDate = baseQ.UpdatedDate,
                            UpdatedFullName = baseQ.UpdatedFullName,
                            UpdatedUserId = baseQ.UpdatedUserId,
                            VideoFilePath = baseQ.VideoFilePath,
                            LevelName = level.Name,
                            OriginalId = baseQ.OriginalId,
                            IsActive = videoResults.Any()
                        };

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var videoIds = lists.Select(x => x.Id).ToList();

            var videoSkills = await (
                              from vtc in _videoTimeCodeRepository.Queryable.WhereBulkContains(videoIds, x => x.VideoId)
                              join tce in _timeCodeExerciseRepository.Queryable on vtc.Id equals tce.VideoTimeCodeId
                              join e in _exerciseRepository.Queryable on tce.ExerciseId equals e.Id
                              join s in _skillRepository.Queryable on e.SkillId equals s.Id into skillJoin
                              from s in skillJoin.DefaultIfEmpty() // LEFT JOIN
                              select new
                              {
                                  vtc.VideoId,
                                  SkillName = s != null ? s.Name : null,
                                  CourseSkill = e.CourseSkill,
                                  ExerciseId = e.Id
                              }
                          ).ToListAsync(cancellationToken);

            foreach (var item in lists)
            {
                var skills = videoSkills.Where(x => x.VideoId == item.Id);
                item.Exercises = skills.Distinct().GroupBy(x => x.CourseSkill).Select(x => new VideoExerciseSearchModel
                {
                    CourseSkill = x.Key,
                    Count = x.Select(x => x.ExerciseId).Distinct().Count(),
                }).ToList();
                item.Skills = skills.Where(x => !string.IsNullOrEmpty(x.SkillName)).Select(x => x.SkillName).Distinct().ToList();
            }
            methodResult.Result = new PagingItemsModel<VideoSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
