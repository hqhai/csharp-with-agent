// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonNoteCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.LessonNotes;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateLessonNoteCommand : CreateLessonNoteCommandModel, IRequest<MethodResult<LessonNoteModel>>
    {
    }

    public class CreateLessonNoteCommandHandler : IRequestHandler<CreateLessonNoteCommand, MethodResult<LessonNoteModel>>
    {
        private readonly ILessonNoteRepository _lessonNoteRepository;
        private readonly IMapper _mapper;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ILessonResultRepository _lessonResultRepository;

        public CreateLessonNoteCommandHandler(ILessonNoteRepository lessonNoteRepository, IMapper mapper, QuestBoardPublisher questBoardPublisher, ILessonResultRepository lessonResultRepository)
        {
            _lessonNoteRepository = lessonNoteRepository;
            _mapper = mapper;
            _questBoardPublisher = questBoardPublisher;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<LessonNoteModel>> Handle(CreateLessonNoteCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonNoteModel> methodResult = new MethodResult<LessonNoteModel>();

            LessonNote lessonNote = _mapper.Map<LessonNote>(request);
            if (!lessonNote.IsValid())
            {
                methodResult.AddErrorBadRequest(lessonNote.ErrorMessages);
                return methodResult;
            }

            if (request.Type == EnumNoteType.PersonalCourse && request.LessonResultId != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.LessonResultId));
                return methodResult;
            }
            else if (request.Type == EnumNoteType.CourseNote && request.LessonResultId == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.LessonResultId));
                return methodResult;
            }

            await _lessonNoteRepository.ExecuteTransactionAsync(async () =>
            {
                lessonNote = _lessonNoteRepository.Add(lessonNote);
                await _lessonNoteRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<LessonNoteModel>(lessonNote);

                #region Do QuestBoard

                if (request.LessonResultId.HasValue)
                {
                    var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId.Value);
                    if (lessonResult != null)
                    {
                        await DoQuestBoard(lessonResult.StudentId, cancellationToken);
                    }
                }

                #endregion Do QuestBoard

                return methodResult;
            });
            return methodResult;
        }

        private async Task DoQuestBoard(Guid studentId, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = studentId,
                Type = EnumQuestBoardType.LearningQuests,
                Category = EnumQuestBoardCategory.GalaxyNotes,
                Value = 1
            }, cancellationToken);
        }
    }
}
