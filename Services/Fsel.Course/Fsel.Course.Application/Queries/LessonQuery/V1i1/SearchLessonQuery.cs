// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.LessonQuery.V1i1
{
    using System.Globalization;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Application.Services.UserServices;
    using Fsel.Course.Application.Services.UserServices.Models;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using Fsel.Course.Domain.Models.QueryModels.Lessons.V1i1;
    using Fsel.Course.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchLessonQuery : SearchLessonQueryModel, IRequest<MethodResult<PagingItemsModel<LessonSearchModel>>>
    {
    }

    public class SearchLessonQueryHandler : IRequestHandler<SearchLessonQuery, MethodResult<PagingItemsModel<LessonSearchModel>>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserService _userService;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ISkillRepository _skillRepository;

        public SearchLessonQueryHandler(ILessonRepository lessonRepository,
                                        ILessonModuleRepository lessonModuleRepository,
                                        IVideoRepository videoRepository,
                                        ILevelRepository levelRepository,
                                        ICategoryRepository categoryRepository,
                                        IUserService userService,
                                        IVideoTimeCodeRepository videoTimeCodeRepository,
                                        ITimeCodeExerciseRepository timeCodeExerciseRepository,
                                        IExerciseRepository exerciseRepository,
                                        ISkillRepository skillRepository)
        {
            _lessonRepository = lessonRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _videoRepository = videoRepository;
            _levelRepository = levelRepository;
            _categoryRepository = categoryRepository;
            _userService = userService;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _exerciseRepository = exerciseRepository;
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<PagingItemsModel<LessonSearchModel>>> Handle(SearchLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<LessonSearchModel>> methodResult = new MethodResult<PagingItemsModel<LessonSearchModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var lessonQuery = from a in _lessonRepository.ReadQueryable
                              where !a.IsArchive && a.VersionStatus == EnumVersionStatus.LastVersion
                              select new
                              {
                                  Lesson = a,
                                  Videos = (from b in _lessonModuleRepository.ReadQueryable
                                            join v in _videoRepository.ReadQueryable on b.OriginalId equals v.OriginalId
                                            where b.LessonId == a.Id && b.LessonConfigType == EnumLessonConfigType.Video
                                            select v).ToList()
                              };

            request.Keyword = request.Keyword?.Trim().ToLower(CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                lessonQuery = lessonQuery.Where(m => m.Lesson.Name != null && m.Lesson.Name.Contains(request.Keyword));
            }

            if (request.ProgramId.HasValue)
            {
                lessonQuery = lessonQuery.Where(x => x.Lesson.ProgramId == request.ProgramId);
            }

            if (request.LevelId.HasValue)
            {
                lessonQuery = lessonQuery.Where(x => x.Lesson.LevelId == request.LevelId);
            }

            if (request.TeacherId.HasValue)
            {
                lessonQuery = lessonQuery.Where(x => x.Videos.Any(v => v.TeacherId == request.TeacherId));
            }

            if (request.TimeCodeType.HasValue)
            {
                lessonQuery = lessonQuery.Where(x => x.Videos.Any(v => v.VideoTimeCodes.Any(t => t.TimeCodeType == request.TimeCodeType)));
            }
            var lesson = lessonQuery.Select(x => new LessonSearchModel
            {
                Id = x.Lesson.Id,
                CreatedDate = x.Lesson.CreatedDate,
                CreatedUserId = x.Lesson.CreatedUserId,
                CreatedFullName = x.Lesson.CreatedFullName,
                UpdatedDate = x.Lesson.UpdatedDate,
                UpdatedFullName = x.Lesson.UpdatedFullName,
                UpdatedUserId = x.Lesson.UpdatedUserId,
                Name = x.Lesson.Name,
                Status = x.Lesson.Status,
                LevelId = x.Lesson.LevelId,
                ProgramId = x.Lesson.ProgramId,
                Overview = x.Lesson.InstructionContent,
                Videos = x.Videos.Select(v => new VideoSearchModel
                {
                    VideoId = v.Id,
                    TeacherId = v.TeacherId,
                    TimeCodeTypes = v.VideoTimeCodes.Select(t => t.TimeCodeType).Distinct().ToList(),
                }).ToList(),
                OriginalId = x.Lesson.OriginalId,
                Skills = x.Lesson.LessonInstructions.Select(i => i.Skill.Name).ToList()
            });

            int totalItem = await lesson.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lessons = await lesson.ApplySortAndPaging(request)
                                    .AsNoTracking()
                                    .ToListAsync(cancellationToken: cancellationToken)
                                    .ConfigureAwait(false);

            lessons.ForEach(x =>
            {
                x.Skills = x.Skills?.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            });

            foreach (var item in lessons)
            {
                var level = await _levelRepository.ReadQueryable.FirstOrDefaultAsync(x => x.Id == item.LevelId, cancellationToken);
                item.NameLevel = level?.Name;

                var category = await _categoryRepository.ReadQueryable.FirstOrDefaultAsync(x => x.Id == item.ProgramId, cancellationToken);
                item.NameProgram = category?.Name;
            }

            var teacherResults = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = lessonQuery.SelectMany(x => x.Videos.Select(c => c.TeacherId).ToList()).ToList() });
            if (teacherResults.IsSuccessStatusCode)
            {
                var teachers = teacherResults.Content?.Result;
                foreach (var item in lessons.Where(x => x.Videos != null).SelectMany(x => x.Videos!))
                {
                    item.NameTeacher = teachers?.FirstOrDefault(x => x.Id == item.TeacherId)?.Human?.FullName;
                }
            }

            methodResult.Result = new PagingItemsModel<LessonSearchModel>(lessons, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
