// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonNoteQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
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
        private readonly IMapper _mapper;

        public GetListLessonNoteQueryHandler(ILessonNoteRepository lessonNoteRepository, IMapper mapper, ILessonResultRepository lessonResultRepository)
        {
            _lessonNoteRepository = lessonNoteRepository;
            _mapper = mapper;
            _lessonResultRepository = lessonResultRepository;
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
                                            Name = lessonResult.Name,
                                            Note = lessonResult.Note,
                                            LessonResultId = lessonResult.LessonResultId
                                        }).ToListAsync(cancellationToken: cancellationToken);
            var lessonResult = await _lessonResultRepository.Queryable.Where(x => x.Id == request.LessonResultId).Select(x => x.SummaryNote).ToListAsync(cancellationToken: cancellationToken);
            lessonNoteListModel.LessonNotes = lessonNotes;
            lessonNoteListModel.SummaryNote = lessonResult.FirstOrDefault();
            lessonNoteListModel.LessonResultId = request.LessonResultId;

            methodResult.Result = lessonNoteListModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
