// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Lessons;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Repositories;
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

        public CreateLessonCommandHandler(ILessonRepository lessonRepository
            , IMapper mapper, IHomeWorkRepository homeWorkRepository
            , IVideoRepository videoRepository
            , IExtraPracticeRepository extraPracticeRepository)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _videoRepository = videoRepository;
            _extraPracticeRepository = extraPracticeRepository;
        }

        public async Task<MethodResult<LessonModel>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();

            #region Validation

            if (request.HomeWorkIds == null || request.HomeWorkIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HomeWorksNull), nameof(request.HomeWorkIds), request.HomeWorkIds);
                return methodResult;
            }

            if (request.LessonInstructions == null || request.LessonInstructions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonInstructionErrorCode.LessonInstructionsNull), nameof(request.LessonInstructions), request.LessonInstructions);
                return methodResult;
            }

            if (request.ExtraPracticeIds == null || request.ExtraPracticeIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPractiveErrorCode.ExtraPracticesNull), nameof(request.ExtraPracticeIds), request.ExtraPracticeIds);
                return methodResult;
            }

            if (request.VideoIds == null || request.VideoIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideosNull), nameof(request.VideoIds), request.VideoIds);
                return methodResult;
            }

            if (request.ClassForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumErrorCode.ClassForumNull), nameof(request.ClassForum), request.ClassForum);
                return methodResult;
            }

            ClassForum classForum = new ClassForum();
            _mapper.Map(request.ClassForum, classForum);

            if (_homeWorkRepository.IsIdsInValid(request.HomeWorkIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HomeworksNotExist), nameof(request.HomeWorkIds), request.HomeWorkIds);
                return methodResult;
            }

            if (_extraPracticeRepository.IsIdsInValid(request.ExtraPracticeIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPractiveErrorCode.ExtraPracticesNotExist), nameof(request.ExtraPracticeIds), request.ExtraPracticeIds);
                return methodResult;
            }

            if (_videoRepository.IsIdsInValid(request.VideoIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideosNotExist), nameof(request.VideoIds), request.VideoIds);
                return methodResult;
            }

            var isExistName = await _lessonRepository.Queryable.AnyAsync(x => x.Name == request.Name, cancellationToken);
            if (isExistName)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonNameIsExist), nameof(request.Name), request.Name);
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
                lesson.LessonExtraPractices = request.ExtraPracticeIds.Select((x) => new LessonExtraPractice
                {
                    ExtracPraticeId = x
                }).ToList();
                lesson.LessonVideos = request.VideoIds.Select((x) => new LessonVideo
                {
                    VideoId = x
                }).ToList();

                lesson.LessonInstructions = _mapper.Map<IList<LessonInstruction>>(request.LessonInstructions);

                classForum.LessonId = lesson.Id;
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
