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
    public class UpdateLessonCommand : UpdateLessonCommandModel, IRequest<MethodResult<LessonModel>>
    {
    }

    public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand, MethodResult<LessonModel>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonHomeWorkRepository _lessonHomeWorkRepository;
        private readonly ILessonExtraPracticeRepository _lessonExtraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IClassForumRepository _classForumRepository;

        public UpdateLessonCommandHandler(ILessonRepository lessonRepository
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

        public async Task<MethodResult<LessonModel>> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
        {
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();

            #region Validation

            if (request == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError("Not Found");
                return methodResult;
            }

            var lesson = await _lessonRepository.GetByIdAsync(request.Id);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LS01V),
                                                nameof(request.Id), request.Id);
                return methodResult;
            }

            if (!lesson.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(lesson.ErrorMessages);
                return methodResult;
            }

            if (request.HomeWorkIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HW03V),
                                                nameof(request.HomeWorkIds), request.HomeWorkIds);
                return methodResult;
            }

            if (request.VideoIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VD03V), nameof(request.VideoIds), request.VideoIds);
                return methodResult;
            }

            if (request.ExtraPracticeIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPractiveErrorCode.EP03V),
                    nameof(request.ExtraPracticeIds), request.ExtraPracticeIds);
                return methodResult;
            }

            var isLessonUsed = await _lessonRepository.IsLessonUsed(request.Id);

            if (isLessonUsed)
            {
                methodResult.AddErrorBadRequest(
                    nameof(EnumLessonErrorCode.LS02V), nameof(request.Id), request.Id);
                return methodResult;
            }

            if (_extraPracticeRepository.IsIdsInValid(request.ExtraPracticeIds))
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumExtraPractiveErrorCode.EP03V));

                return methodResult;
            }

            if (_videoRepository.IsIdsInValid(request.VideoIds))
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumVideoErrorCode.VD03V));

                return methodResult;
            }

            if (_classForumRepository.IsIdsInValid(new List<Guid> { request.ClassForumId }))
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumClassForumErrorCode.CF01V));
                return methodResult;
            }

            if (_homeWorkRepository.IsIdsInValid(request.HomeWorkIds))
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(
                    nameof(EnumHomeWorkErrorCode.HW03V));

                return methodResult;
            }

            if (!lesson.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(lesson.ErrorMessages);
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
