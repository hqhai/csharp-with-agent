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

    public class GetListLessonNoteQuery : IRequest<MethodResult<IList<LessonNoteModel>>>
    {
        public Guid Id { get; set; }
    }

    public class GetListLessonNoteQueryHandler : IRequestHandler<GetListLessonNoteQuery, MethodResult<IList<LessonNoteModel>>>
    {
        private readonly ILessonNoteRepository _lessonNoteRepository;
        private readonly IMapper _mapper;

        public GetListLessonNoteQueryHandler(ILessonNoteRepository lessonNoteRepository, IMapper mapper)
        {
            _lessonNoteRepository = lessonNoteRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LessonNoteModel>>> Handle(GetListLessonNoteQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<LessonNoteModel>> methodResult = new MethodResult<IList<LessonNoteModel>>();
            var lessonResultQuery = await _lessonNoteRepository.Queryable
                                        .Where(x => x.Id == request.Id)
                                        .Select(lessonResult => new LessonNoteModel
                                        {
                                            Name = lessonResult.Name,
                                            Note = lessonResult.Note,
                                            LessonResultId = lessonResult.LessonResultId
                                        }).ToListAsync(cancellationToken: cancellationToken);
            if (lessonResultQuery == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonResultErrorCode.LessonResultsDoesNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            methodResult.Result = lessonResultQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
