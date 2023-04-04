// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonNoteCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.LessonNotes;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateLessonNoteCommand : CreateLessonNoteCommandModel, IRequest<MethodResult<LessonNoteModel>>
    {
    }

    public class CreateLessonNoteCommandHandler : IRequestHandler<CreateLessonNoteCommand, MethodResult<LessonNoteModel>>
    {
        private readonly ILessonNoteRepository _lessonNoteRepository;
        private readonly IMapper _mapper;

        public CreateLessonNoteCommandHandler(ILessonNoteRepository lessonNoteRepository, IMapper mapper)
        {
            _lessonNoteRepository = lessonNoteRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<LessonNoteModel>> Handle(CreateLessonNoteCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonNoteModel> methodResult = new MethodResult<LessonNoteModel>();
            LessonNote lessonNote = _mapper.Map<LessonNote>(request);
            if (!lessonNote.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(lessonNote.ErrorMessages);
                return methodResult;
            }
            await _lessonNoteRepository.ExecuteTransactionAsync(async () =>
            {
                lessonNote = _lessonNoteRepository.Add(lessonNote);
                await _lessonNoteRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<LessonNoteModel>(lessonNote);
                return methodResult;
            });
            return methodResult;
        }
    }
}
