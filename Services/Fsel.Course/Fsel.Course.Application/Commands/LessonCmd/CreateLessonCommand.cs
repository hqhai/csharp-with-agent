// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Lessons;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.LessonCmd
{
    public class CreateLessonCommand : CreateLessonCommandModel, IRequest<MethodResult<LessonModel>>
    {
    }

    public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, MethodResult<LessonModel>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonHomeWorkRepository _lessonHomeWorkRepository;
        private readonly ILessonExtraPracticeRepository _lessonExtraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IClassForumRepository _classForumRepository;

        public CreateLessonCommandHandler(ILessonRepository lessonRepository
            , ILessonHomeWorkRepository lessonHomeWorkRepository
            , ILessonExtraPracticeRepository lessonExtraPracticeRepository
            , IMapper mapper, IHomeWorkRepository homeWorkRepository
            , IVideoRepository videoRepository
            , IExtraPracticeRepository extraPracticeRepository
            , IClassForumRepository classForumRepository)
        {
            _lessonRepository = lessonRepository;
            _lessonHomeWorkRepository = lessonHomeWorkRepository;
            _lessonExtraPracticeRepository = lessonExtraPracticeRepository;
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _videoRepository = videoRepository;
            _extraPracticeRepository = extraPracticeRepository;
            _classForumRepository = classForumRepository;
        }

        public async Task<MethodResult<LessonModel>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
        {
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();

            #region Validation

            if (request == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            if (request.HomeWorkIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HW03V), nameof(request.HomeWorkIds));
                return methodResult;
            }

            if (request.ExtraPracticeIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPractiveErrorCode.EP03V), nameof(request.ExtraPracticeIds));
                return methodResult;
            }

            if (request.VideoIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VD03V), nameof(request.VideoIds));
                return methodResult;
            }

            if (request.ClassForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumErrorCode.CF03V), nameof(request.ClassForum));
                return methodResult;
            }

            ClassForum classForum = new ClassForum();
            _mapper.Map(request.ClassForum, classForum);

            if (_homeWorkRepository.IsIdsInValid(request.HomeWorkIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HW03V), nameof(request.HomeWorkIds), request.HomeWorkIds);
                return methodResult;
            }

            if (_extraPracticeRepository.IsIdsInValid(request.ExtraPracticeIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPractiveErrorCode.EP03V), nameof(request.ExtraPracticeIds), request.ExtraPracticeIds);
                return methodResult;
            }

            if (_videoRepository.IsIdsInValid(request.VideoIds))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VD03V), nameof(request.VideoIds), request.VideoIds);
                return methodResult;
            }

            Lesson lesson = _mapper.Map<Lesson>(request);

            if (!lesson.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(lesson.ErrorMessages);
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
