// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.ClassForums;
using Fsel.Course.Domain.Models.CommandModels.LessonInstructions;
using Fsel.Course.Domain.Models.CommandModels.Lessons;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.LessonCmd
{
    public class UpdateLessonCommand : UpdateLessonCommandModel, IRequest<MethodResult<LessonModel>>
    {
    }

    public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand, MethodResult<LessonModel>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ILessonExtraPracticeRepository _lessonExtraPracticeRepository;
        private readonly ILessonVideoRepository _lessonVideoRepository;
        private readonly ILessonHomeWorkRepository _lessonHomeWorkRepository;
        private readonly ILessonInstructionRepository _lessonInstructionRepository;
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly ISkillRepository _skillRepository;

        public UpdateLessonCommandHandler(ILessonRepository lessonRepository
            , IMapper mapper, IHomeWorkRepository homeWorkRepository
            , IVideoRepository videoRepository
            , IClassForumRepository classForumRepository
            , ILessonExtraPracticeRepository lessonExtraPracticeRepository
            , ILessonVideoRepository lessonVideoRepository
            , ILessonHomeWorkRepository lessonHomeWorkRepository
            , ILessonInstructionRepository lessonInstructionRepository
            , IExtraPracticeRepository extraPracticeRepository
            , ISkillRepository skillRepository)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _videoRepository = videoRepository;
            _classForumRepository = classForumRepository;
            _lessonExtraPracticeRepository = lessonExtraPracticeRepository;
            _lessonVideoRepository = lessonVideoRepository;
            _lessonHomeWorkRepository = lessonHomeWorkRepository;
            _lessonInstructionRepository = lessonInstructionRepository;
            _extraPracticeRepository = extraPracticeRepository;
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<LessonModel>> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();

            #region Validation

            var lesson = await _lessonRepository.GetByIdAsync(request.Id);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson));
                return methodResult;
            }
            if (request.ClassForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ClassForum));
                return methodResult;
            }
            var skillId = request.ClassForum.SkillId;
            if (skillId.HasValue)
            {
                var skillExists = await _skillRepository.AnyGuidAsync(skillId.Value);
                if (!skillExists)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(skillId), skillId);
                    return methodResult;
                }
            }
            if (request.LessonInstructions == null || request.LessonInstructions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.LessonInstructions));
                return methodResult;
            }
            foreach (var lessonInstruction in request.LessonInstructions)
            {
                if (lessonInstruction.SkillId.HasValue)
                {
                    var skillExists = await _skillRepository.AnyGuidAsync(lessonInstruction.SkillId.Value);
                    if (!skillExists)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonInstruction.SkillId), lessonInstruction.SkillId);
                        return methodResult;
                    }
                }
            }
            if (request.HomeWorkIds == null || request.HomeWorkIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.HomeWorkIds));
                return methodResult;
            }

            if (request.VideoIds == null || request.VideoIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.VideoIds));
                return methodResult;
            }

            var isLessonUsed = await _lessonRepository.IsLessonUsed(request.Id);
            if (isLessonUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonUsed), nameof(request.Id), request.Id);
                return methodResult;
            }

            if (request.ExtraPracticeIds != null && request.ExtraPracticeIds.Count > 0)
            {
                if (_extraPracticeRepository.IsIdsInValid(request.ExtraPracticeIds))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ExtraPracticeIds));
                    return methodResult;
                }
            }

            if (_videoRepository.IsIdsInValid(request.VideoIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.VideoIds));

                return methodResult;
            }

            if (_homeWorkRepository.IsIdsInValid(request.HomeWorkIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.HomeWorkIds));

                return methodResult;
            }

            var isExistName = await _lessonRepository.Queryable.AnyAsync(x => x.Name == request.Name && x.Id != request.Id, cancellationToken);
            if (isExistName)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isExistName));
                return methodResult;
            }

            #endregion Validation

            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                _mapper.Map(request, lesson);
                if (!lesson.IsValid())
                {
                    methodResult.AddErrorBadRequest(lesson.ErrorMessages);
                    return methodResult;
                }
                await UpdateLessonExtraPracticeAsync(lesson, request.ExtraPracticeIds, cancellationToken);
                await UpdateLessonHomeWorkAsync(lesson, request.HomeWorkIds, cancellationToken);
                await UpdateLessonVideoAsync(lesson, request.VideoIds, cancellationToken);
                await UpdateLessonIntructionAsync(lesson, request.LessonInstructions, cancellationToken);
                await UpdateLessonClassForumAsync(lesson, request.ClassForum, cancellationToken);

                lesson = _lessonRepository.Update(lesson);
                await _lessonRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<LessonModel>(lesson);
                return methodResult;
            });

            return methodResult;
        }

        private async Task UpdateLessonExtraPracticeAsync(Lesson lesson, IList<Guid>? extraPracticeIds, CancellationToken cancellationToken)
        {
            var lessonExtraPractices = await _lessonExtraPracticeRepository.Queryable.Where(x => x.LessonId == lesson.Id).ToListAsync(cancellationToken);
            if (extraPracticeIds != null && extraPracticeIds.Any())
            {
                lesson.LessonExtraPractices = extraPracticeIds.Select(x => new LessonExtraPractice
                {
                    LessonId = lesson.Id,
                    ExtracPraticeId = x
                }).ToList();

                if (lessonExtraPractices != null && lessonExtraPractices.Any())
                {
                    await _lessonExtraPracticeRepository.DeleteListAsync(lessonExtraPractices);
                    await _lessonExtraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            else if (lessonExtraPractices.Any())
            {
                await _lessonExtraPracticeRepository.DeleteListAsync(lessonExtraPractices);
                await _lessonExtraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateLessonHomeWorkAsync(Lesson lesson, IList<Guid>? homeWorkIds, CancellationToken cancellationToken)
        {
            var lessonHomeWorks = await _lessonHomeWorkRepository.Queryable.Where(x => x.LessonId == lesson.Id).ToListAsync(cancellationToken);
            if (homeWorkIds != null && homeWorkIds.Any())
            {
                lesson.LessonHomeWorks = homeWorkIds.Select(x => new LessonHomeWork
                {
                    LessonId = lesson.Id,
                    HomeWorkId = x
                }).ToList();
            }
            else if (lessonHomeWorks.Any())
            {
                await _lessonHomeWorkRepository.DeleteListAsync(lessonHomeWorks);
                await _lessonHomeWorkRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateLessonClassForumAsync(Lesson lesson, CreateClassForumCommandModel? requestClassForum, CancellationToken cancellationToken)
        {
            var classForum = await _classForumRepository.Queryable.Include(x => x.ClassForumFiles).FirstOrDefaultAsync(x => x.LessonId == lesson.Id, cancellationToken);
            if (classForum != null && requestClassForum != null)
            {
                _mapper.Map(requestClassForum, classForum);
                classForum.ClassForumFiles = requestClassForum.FilePaths?.Select(x => new ClassForumFile
                {
                    FilePath = x,
                }).ToList() ?? new List<ClassForumFile>();

                _classForumRepository.Update(classForum);
                await _classForumRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateLessonVideoAsync(Lesson lesson, IList<Guid>? videoIds, CancellationToken cancellationToken)
        {
            var lessonVideos = await _lessonVideoRepository.Queryable.Where(x => x.LessonId == lesson.Id).ToListAsync(cancellationToken);
            if (videoIds != null && videoIds.Any())
            {
                var lessonVideoRemotes = lessonVideos.Where(x => !videoIds.Contains(x.VideoId)).ToList();

                if (lessonVideoRemotes != null && lessonVideoRemotes.Any())
                {
                    await _lessonVideoRepository.DeleteListAsync(lessonVideoRemotes);
                    await _lessonVideoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                lessonVideos.AddRange(videoIds.Where(x => !lessonVideos.Any(y => y.VideoId == x)).Select(x => new LessonVideo
                {
                    LessonId = lesson.Id,
                    VideoId = x
                }).ToList());

                lesson.LessonVideos = lessonVideos;
            }
            else if (lessonVideos.Any())
            {
                await _lessonVideoRepository.DeleteListAsync(lessonVideos);
                await _lessonVideoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateLessonIntructionAsync(Lesson lesson, IList<UpdateLessonInstructionCommandModel>? requestLessonInstructions, CancellationToken cancellationToken)
        {
            var lessonInstructions = await _lessonInstructionRepository.Queryable.Where(x => x.LessonId == lesson.Id).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);

            if (requestLessonInstructions != null && requestLessonInstructions.Any())
            {
                if (lessonInstructions != null && lessonInstructions.Any())
                {
                    foreach (var lessonInstruction in lessonInstructions.Distinct())
                    {
                        var request = requestLessonInstructions.FirstOrDefault(x => x.Id == lessonInstruction.Id);
                        _mapper.Map(request, lessonInstruction);
                    }
                    lesson.LessonInstructions = lessonInstructions.OrderBy(x => x.CourseSkill).ToList();
                }
                else
                {
                    lesson.LessonInstructions = _mapper.Map<List<LessonInstruction>>(requestLessonInstructions.OrderBy(x => x.CourseSkill).ToList());
                }
            }
        }
    }
}
