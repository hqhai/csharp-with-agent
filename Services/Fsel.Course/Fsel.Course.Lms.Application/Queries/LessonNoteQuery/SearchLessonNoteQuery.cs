// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonNoteQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.LessonNotes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SearchLessonNoteQuery : SearchLessonNoteQueryModel, IRequest<MethodResult<PagingItemsModel<LessonNoteModel>>>
    {
    }

    public class SearchLessonNoteQueryHandler : IRequestHandler<SearchLessonNoteQuery, MethodResult<PagingItemsModel<LessonNoteModel>>>
    {
        private readonly ILessonNoteRepository _lessonNoteRepository;
        private readonly AuthContext _authContext;

        public SearchLessonNoteQueryHandler(ILessonNoteRepository lessonNoteRepository, AuthContext authContext)
        {
            _lessonNoteRepository = lessonNoteRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<LessonNoteModel>>> Handle(SearchLessonNoteQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = _lessonNoteRepository.Queryable.Include(x => x.LessonResult).Where(x => x.CreatedUserId == _authContext.CurrentUserId).OrderByDescending(x => x.CreatedDate).AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }

            if (request.UnitId != null)
            {
                query = query.Where(m => m.LessonResult!.UnitId == request.UnitId);
            }

            if (request.LessonId != null)
            {
                query = query.Where(m => m.LessonResult!.LessonId == request.LessonId);
            }

            if (request.Type != null)
            {
                query = query.Where(m => m.Type == request.Type);
            }

            var methodResult = await _lessonNoteRepository.GetListByPageResultAsync<LessonNoteModel>(query, request, cancellationToken);
            return methodResult;
        }
    }
}
