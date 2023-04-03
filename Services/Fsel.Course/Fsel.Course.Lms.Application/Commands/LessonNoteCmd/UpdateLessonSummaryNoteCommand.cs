// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonNoteCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateLessonSummaryNoteCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class UpdateLessonSummaryNoteCommandHandel : IRequestHandler<UpdateLessonSummaryNoteCommand, MethodResult<bool>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMapper _mapper;

        public UpdateLessonSummaryNoteCommandHandel(ILessonResultRepository lessonResultRepository, IMapper mapper)
        {
            _lessonResultRepository = lessonResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(UpdateLessonSummaryNoteCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.Id);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonResultErrorCode.LessonResultsDoesNotExist),
                                                nameof(request.Id), request.Id);
                return methodResult;
            }
            /*if (!lessonResult.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(lessonResult.ErrorMessages);
                return methodResult;
            }*/
            await _lessonResultRepository.ExecuteTransactionAsync(async () =>
            {
                var result = _lessonResultRepository.Update(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
