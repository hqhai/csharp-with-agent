// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonNoteQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListLessonNoteQuery : IRequest<MethodResult<LessonNoteListModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetListLessonNoteQueryHandler : IRequestHandler<GetListLessonNoteQuery, MethodResult<LessonNoteListModel>>
    {
        private readonly ILessonNoteRepository _lessonNoteRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public GetListLessonNoteQueryHandler(ILessonNoteRepository lessonNoteRepository, ILessonResultRepository lessonResultRepository)
        {
            _lessonNoteRepository = lessonNoteRepository;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<LessonNoteListModel>> Handle(GetListLessonNoteQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonNoteListModel> methodResult = new MethodResult<LessonNoteListModel>();
            var lessonNoteListModel = new LessonNoteListModel();

            var lessonNotes = await _lessonNoteRepository.Queryable
                                        .Where(x => x.LessonResultId == request.LessonResultId)
                                        .OrderBy(x => x.CreatedDate)
                                        .Select(lessonResult => new LessonNoteModel
                                        {
                                            Id = lessonResult.Id,
                                            Name = lessonResult.Name,
                                            Note = lessonResult.Note,
                                            LessonResultId = lessonResult.LessonResultId,
                                            CreatedDate = lessonResult.CreatedDate,
                                        }).ToListAsync(cancellationToken: cancellationToken);

            var lessonResult = await _lessonResultRepository.Queryable.Where(x => x.Id == request.LessonResultId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (lessonResult != null)
            {
                lessonNoteListModel.SummaryNote = lessonResult.SummaryNote;
            }
            lessonNoteListModel.LessonResultId = request.LessonResultId;
            lessonNoteListModel.LessonNotes = lessonNotes;
            methodResult.Result = lessonNoteListModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
