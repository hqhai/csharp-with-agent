// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteVideoTimeCodeAnswersCommand : IRequest<MethodResult<bool>>
    {
        public Guid VideoResultId { get; set; }
        public Guid LessonResultId { get; set; }
    }

    public class DeleteVideoTimeCodeAnswersCommandHandler : IRequestHandler<DeleteVideoTimeCodeAnswersCommand, MethodResult<bool>>
    {
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public DeleteVideoTimeCodeAnswersCommandHandler(IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository, IVideoResultRepository videoResultRepository, ILessonResultRepository lessonResultRepository)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoResultRepository = videoResultRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteVideoTimeCodeAnswersCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var videoResult = await _videoResultRepository.Queryable.Include(x => x.VideoTimeCodeResults).Where(x => x.Id == request.VideoResultId).FirstOrDefaultAsync(cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var videoTimeCodeResultIds = videoResult.VideoTimeCodeResults.Select(x => x.Id).ToList();
            await _videoTimeCodeAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                var videoTimeCodeAnswers = await _videoTimeCodeAnswerRepository.Queryable.Where(p => videoTimeCodeResultIds.Contains(p.VideoTimeCodeResultId ?? default)).ToListAsync(cancellationToken);
                if (videoTimeCodeAnswers.Count > 0)
                {
                    foreach (var item in videoTimeCodeAnswers)
                    {
                        await _videoTimeCodeAnswerRepository.DeleteAsync(item);
                    }
                    await _videoTimeCodeAnswerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                var videoResult = await _videoResultRepository.GetByIdAsync(request.VideoResultId);
                if (videoResult != null)
                {
                    videoResult.Status = EnumResultStatus.Process;
                    _videoResultRepository.Update(videoResult, false, x => x.StudentId, x => x.VideoId, x => x.LessonResultId);
                    await _videoResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
                if (lessonResult != null)
                {
                    lessonResult.Status = EnumResultStatus.Process;
                    _lessonResultRepository.Update(lessonResult, false, x => x.LessonId, x => x.CourseId, x => x.UnitId, x => x.StudentId);
                    await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            return methodResult;
        }
    }
}
