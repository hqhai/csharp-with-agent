// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonNoteQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListLessonNoteQuery : BaseQueryModel, IRequest<MethodResult<LessonNoteListModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetListLessonNoteQueryHandler : IRequestHandler<GetListLessonNoteQuery, MethodResult<LessonNoteListModel>>
    {
        private readonly ILessonNoteRepository _lessonNoteRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly QuestBoardPublisher _questBoardPublisher;

        public GetListLessonNoteQueryHandler(ILessonNoteRepository lessonNoteRepository, ILessonResultRepository lessonResultRepository, QuestBoardPublisher questBoardPublisher)
        {
            _lessonNoteRepository = lessonNoteRepository;
            _lessonResultRepository = lessonResultRepository;
            _questBoardPublisher = questBoardPublisher;
        }

        public async Task<MethodResult<LessonNoteListModel>> Handle(GetListLessonNoteQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonNoteListModel> methodResult = new MethodResult<LessonNoteListModel>();
            var lessonNoteListModel = new LessonNoteListModel();

            var lessonNotes = await _lessonNoteRepository.Queryable
                                        .Where(x => x.LessonResultId == request.LessonResultId)
                                        .Select(lessonResult => new LessonNoteModel
                                        {
                                            Id = lessonResult.Id,
                                            Name = lessonResult.Name,
                                            Note = lessonResult.Note,
                                            LessonResultId = lessonResult.LessonResultId,
                                            CreatedDate = lessonResult.CreatedDate,
                                        })
                                        .ApplySort(request)
                                        .ToListAsync(cancellationToken: cancellationToken);

            var lessonResult = await _lessonResultRepository.Queryable.Where(x => x.Id == request.LessonResultId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (lessonResult != null)
            {
                lessonNoteListModel.SummaryNote = lessonResult.SummaryNote;

                #region Do QuestBoard

                await DoQuestBoard(lessonResult.StudentId, cancellationToken);

                #endregion Do QuestBoard
            }
            lessonNoteListModel.LessonResultId = request.LessonResultId;
            lessonNoteListModel.LessonNotes = lessonNotes;
            methodResult.Result = lessonNoteListModel;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }

        private async Task DoQuestBoard(Guid studentId, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = studentId,
                Type = EnumQuestBoardType.LearningQuests,
                Category = EnumQuestBoardCategory.BackupNotes,
                Value = 1
            }, cancellationToken);
        }
    }
}
