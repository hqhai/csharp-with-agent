// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.LessonQuery.V1i1
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetHistoryLessonQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<LessonHistoryModel>>>
    {
        public Guid OriginalId { get; set; }
    }

    public class GetHistoryLessonQueryHandler : IRequestHandler<GetHistoryLessonQuery, MethodResult<PagingItemsModel<LessonHistoryModel>>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;

        public GetHistoryLessonQueryHandler(ILessonRepository lessonRepository,
                                            IMapper mapper)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<LessonHistoryModel>>> Handle(GetHistoryLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<LessonHistoryModel>>();

            var lessons = _lessonRepository.Queryable
                                           .Where(x => x.OriginalId == request.OriginalId && !x.IsArchive)
                                           .OrderByDescending(x => x.Version)
                                           .AsNoTracking();
            if (lessons == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessons));
                return methodResult;
            }

            int totalItem = await lessons.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var lists = await lessons.ApplyPaging(request)
                                     .AsNoTracking()
                                     .ToListAsync(cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);


            methodResult.Result = new PagingItemsModel<LessonHistoryModel>(_mapper.Map<IList<LessonHistoryModel>>(lists), request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
