// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Lessons;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;

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
        private readonly IExtraPracticeRepository _extraPracticeRepository;

        public UpdateLessonCommandHandler(ILessonRepository lessonRepository
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

        public async Task<MethodResult<LessonModel>> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();

            #region Validation

            var lesson = await _lessonRepository.GetIncludeByIdAsync(request.Id);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonIdNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            if (!lesson.IsValid())
            {
                methodResult.AddErrorBadRequest(lesson.ErrorMessages);
                return methodResult;
            }

            if (request.LessonInstructions == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonInstructionErrorCode.LessonInstructionsNull), nameof(request.LessonInstructions), request.LessonInstructions);
                return methodResult;
            }

            if (request.HomeWorkIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HomeWorkIdsNull), nameof(request.HomeWorkIds), request.HomeWorkIds);
                return methodResult;
            }

            if (request.VideoIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoIdsNull), nameof(request.VideoIds), request.VideoIds);
                return methodResult;
            }

            if (request.ExtraPracticeIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPractiveErrorCode.ExtraPracticeIdsNull), nameof(request.ExtraPracticeIds), request.ExtraPracticeIds);
                return methodResult;
            }

            var isLessonUsed = await _lessonRepository.IsLessonUsed(request.Id);
            if (isLessonUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonUsed), nameof(request.Id), request.Id);
                return methodResult;
            }

            if (_extraPracticeRepository.IsIdsInValid(request.ExtraPracticeIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPractiveErrorCode.ExtraPracticeIdsNotExist), nameof(request.ExtraPracticeIds), request.ExtraPracticeIds);

                return methodResult;
            }

            if (_videoRepository.IsIdsInValid(request.VideoIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoIdsNotExist), nameof(request.VideoIds), request.VideoIds);

                return methodResult;
            }

            if (_homeWorkRepository.IsIdsInValid(request.HomeWorkIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HomeworkIdsNotExist), nameof(request.HomeWorkIds), request.HomeWorkIds);

                return methodResult;
            }

            #endregion Validation

            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                lesson.LessonExtraPractices = request.ExtraPracticeIds.Select(x => new LessonExtraPractice
                {
                    ExtracPraticeId = x
                }).ToList();
                lesson.LessonHomeWorks = request.HomeWorkIds.Select(x => new LessonHomeWork
                {
                    HomeWorkId = x
                }).ToList();
                lesson.LessonVideos = request.VideoIds.Select(x => new LessonVideo
                {
                    VideoId = x
                }).ToList();

                _mapper.Map(request.ClassForum, lesson.ClassForum);
                lesson.LessonInstructions = _mapper.Map<IList<LessonInstruction>>(request.LessonInstructions);
                _mapper.Map(request, lesson);

                lesson = _lessonRepository.Update(lesson);
                await _lessonRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<LessonModel>(lesson);
                return methodResult;
            });

            return methodResult;
        }
    }
}
