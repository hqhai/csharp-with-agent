// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonNoteCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.LessonNotes;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateLessonNoteCommand : UpdateLessonNoteCommandModel, IRequest<MethodResult<LessonNoteModel>>
    {
    }

    public class UpdateLessonNoteCommandHandler : IRequestHandler<UpdateLessonNoteCommand, MethodResult<LessonNoteModel>>
    {
        private readonly ILessonNoteRepository _lessonNoteRepository;
        private readonly IMapper _mapper;

        public UpdateLessonNoteCommandHandler(ILessonNoteRepository lessonNoteRepository, IMapper mapper)
        {
            _lessonNoteRepository = lessonNoteRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<LessonNoteModel>> Handle(UpdateLessonNoteCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonNoteModel> methodResult = new MethodResult<LessonNoteModel>();

            var lessonNote = await _lessonNoteRepository.GetByIdAsync(request.Id);
            if (lessonNote == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonNote));
                return methodResult;
            }

            _mapper.Map(request, lessonNote);
            if (!lessonNote.IsValid())
            {
                methodResult.AddErrorBadRequest(lessonNote.ErrorMessages);
                return methodResult;
            }
            await _lessonNoteRepository.ExecuteTransactionAsync(async () =>
            {
                lessonNote = _lessonNoteRepository.Update(lessonNote);
                await _lessonNoteRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<LessonNoteModel>(lessonNote);
                return methodResult;
            });
            return methodResult;
        }
    }
}
