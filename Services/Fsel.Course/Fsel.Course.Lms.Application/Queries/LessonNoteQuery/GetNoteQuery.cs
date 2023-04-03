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

    public class GetNoteQuery : IRequest<MethodResult<IList<LessonResultModel>>>
    {
        public Guid Id { get; set; }
    }

    public class GetNoteQueryHandler : IRequestHandler<GetNoteQuery, MethodResult<IList<LessonResultModel>>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMapper _mapper;

        public GetNoteQueryHandler(ILessonResultRepository lessonResultRepository, IMapper mapper)
        {
            _lessonResultRepository = lessonResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<LessonResultModel>>> Handle(GetNoteQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<LessonResultModel>> methodResult = new MethodResult<IList<LessonResultModel>>();
            var lessonResultQuery = await _lessonResultRepository.Queryable
                                        .Include(x => x.LessonNotes)
                                        .Where(x => x.Id == request.Id)
                                        .Select(lessonResult => new LessonResultModel
                                        {
                                            Percent = lessonResult.Percent,
                                            CourseId = lessonResult.CourseId,
                                            UnitId = lessonResult.UnitId,
                                            LessonId = lessonResult.LessonId,
                                            StudentId = lessonResult.StudentId,
                                            Status = lessonResult.Status,
                                            LessonNotes = _mapper.Map<IList<LessonNoteModel>>(lessonResult.LessonNotes)
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
