// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonNoteCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateLessonSummaryNoteCommand : IRequest<MethodResult<bool>>
    {
        public Guid LessonResultId { get; set; }

        public string? SummaryNote { get; set; }
    }

    public class UpdateLessonSummaryNoteCommandHandel : IRequestHandler<UpdateLessonSummaryNoteCommand, MethodResult<bool>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;

        public UpdateLessonSummaryNoteCommandHandel(ILessonResultRepository lessonResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateLessonSummaryNoteCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);

            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }
            await _lessonResultRepository.ExecuteTransactionAsync(async () =>
            {
                lessonResult.SummaryNote = request.SummaryNote;

                await _lessonResultRepository.BulkUpdateList(new List<LessonResult> { lessonResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.LessonId };
                });

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
