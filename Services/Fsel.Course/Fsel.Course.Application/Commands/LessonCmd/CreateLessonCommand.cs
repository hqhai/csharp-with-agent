// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Lessons;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.LessonCmd
{
    public class CreateLessonCommand : CreateLessonCommandModel, IRequest<MethodResult<LessonModel>>
    {
    }

    public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, MethodResult<LessonModel>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly ISkillRepository _skillRepository;

        public CreateLessonCommandHandler(ILessonRepository lessonRepository
            , IMapper mapper, IHomeWorkRepository homeWorkRepository
            , IVideoRepository videoRepository
            , IExtraPracticeRepository extraPracticeRepository
            , ISkillRepository skillRepository)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _videoRepository = videoRepository;
            _extraPracticeRepository = extraPracticeRepository;
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<LessonModel>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();

            #region Validation

            if (request.HomeWorkIds == null || request.HomeWorkIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.HomeWorkIds));
                return methodResult;
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

            if (request.VideoIds == null || request.VideoIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.VideoIds));
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
            ClassForum classForum = new ClassForum();
            _mapper.Map(request.ClassForum, classForum);

            if (_homeWorkRepository.IsIdsInValid(request.HomeWorkIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.HomeWorkIds));
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

            var isExistName = await _lessonRepository.Queryable.AnyAsync(x => x.Name == request.Name, cancellationToken);
            if (isExistName)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isExistName));
                return methodResult;
            }

            Lesson lesson = _mapper.Map<Lesson>(request);

            if (!lesson.IsValid())
            {
                methodResult.AddErrorBadRequest(lesson.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                lesson.LessonHomeWorks = request.HomeWorkIds.Select((x) => new LessonHomeWork
                {
                    HomeWorkId = x
                }).ToList();

                if (request.ExtraPracticeIds != null && request.ExtraPracticeIds.Count > 0)
                {
                    lesson.LessonExtraPractices = request.ExtraPracticeIds.Select((x) => new LessonExtraPractice
                    {
                        ExtracPraticeId = x
                    }).ToList();
                }
                lesson.LessonVideos = request.VideoIds.Select((x) => new LessonVideo
                {
                    VideoId = x
                }).ToList();

                lesson.LessonInstructions = _mapper.Map<IList<LessonInstruction>>(request.LessonInstructions);

                classForum.LessonId = lesson.Id;
                if (request.ClassForum.FilePaths != null && request.ClassForum.FilePaths.Count > 0)
                {
                    classForum.ClassForumFiles = request.ClassForum.FilePaths.Select(x => new ClassForumFile
                    {
                        FilePath = x,
                    }).ToList();
                }

                lesson.ClassForum = classForum;
                lesson = _lessonRepository.Add(lesson);
                await _lessonRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<LessonModel>(lesson);
                return methodResult;
            });

            return methodResult;
        }
    }
}
