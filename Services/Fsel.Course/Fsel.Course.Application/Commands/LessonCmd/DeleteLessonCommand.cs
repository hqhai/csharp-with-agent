using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.LessonCmd
{
    public class DeleteLessonCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand, MethodResult<bool>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonHomeWorkRepository _lessonHomeWorkRepository;
        private readonly ILessonExtraPracticeRepository _lessonExtraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly ILessonVideoRepository _lessonVideoRepository;

        public DeleteLessonCommandHandler(ILessonRepository lessonRepository
            , ILessonHomeWorkRepository lessonHomeWorkRepository
            , ILessonExtraPracticeRepository lessonExtraPracticeRepository
            , IMapper mapper, IHomeWorkRepository homeWorkRepository
            , IVideoRepository videoRepository
            , IClassForumRepository classForumRepository
            , IExtraPracticeRepository extraPracticeRepository
            , ILessonVideoRepository lessonVideoRepository)
        {
            _lessonRepository = lessonRepository;
            _lessonHomeWorkRepository = lessonHomeWorkRepository;
            _lessonExtraPracticeRepository = lessonExtraPracticeRepository;
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _videoRepository = videoRepository;
            _classForumRepository = classForumRepository;
            _extraPracticeRepository = extraPracticeRepository;
            _lessonVideoRepository = lessonVideoRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var IsUnitLesson = await _lessonRepository.IsUnitLesson(request.Id);

            if (IsUnitLesson)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumLessonErrorCode.LS02V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                return methodResult;
            }

            var lesson = _lessonRepository.Queryable.Include(e => e.UnitLessons)
                                .Include(e => e.ClassForum).Include(e => e.LessonHomeWorks)
                                .Include(e => e.LessonVideos).Include(e => e.LessonExtraPractices)
                                .FirstOrDefault(e => e.Id == request.Id);
            if (lesson == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumLessonErrorCode.LS01V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                return methodResult;
            }

            if (!lesson.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(lesson.ErrorMessages);
                return methodResult;
            }

            if (lesson.ClassForum == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(nameof(EnumHomeWorkErrorCode.HW03V));
                return methodResult;
            }

            if (lesson.LessonExtraPractices == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumLessonExtraPracticeErrorCode.ELP03V));
                return methodResult;
            }

            if (lesson.LessonHomeWorks == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumeLessonHomeWordErrorCode.EHW03V));
                return methodResult;
            }

            if (lesson.LessonVideos == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumLessonVideoErrorCode.LV03V));
                return methodResult;
            }

            #endregion Validation

            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                var result = await _lessonRepository.DeleteAsync(lesson);

                await _lessonHomeWorkRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                await _lessonExtraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                await _lessonVideoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                await _lessonRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = result;
                return methodResult;
            });

            return methodResult;
        }
    }
}
