// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using System;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchSuggestExtraPracticeQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ExtraPracticeSuggestModel>>>
    {
        public Guid ExtraPracticeId { get; set; }
    }

    public class GetSuggestExtraPracticeQueryHandler : IRequestHandler<SearchSuggestExtraPracticeQuery, MethodResult<PagingItemsModel<ExtraPracticeSuggestModel>>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;

        public GetSuggestExtraPracticeQueryHandler(IExtraPracticeRepository extraPracticeRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ExtraPracticeSuggestModel>>> Handle(SearchSuggestExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<ExtraPracticeSuggestModel>> methodResult = new MethodResult<PagingItemsModel<ExtraPracticeSuggestModel>>();

            var extraPractice = await _extraPracticeRepository.GetByIdAsync(request.ExtraPracticeId);
            if (extraPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(extraPractice));
                return methodResult;
            }
            var random = new Random();
            var extraPracticeQuery = _extraPracticeRepository.Queryable
                                     .Where(x => x.IsActive && x.Type == extraPractice.Type && x.Id != request.ExtraPracticeId)
                                     .AsNoTracking()
                                     .Select(x => new ExtraPracticeSuggestModel
                                     {
                                         Id = x.Id,
                                         ImagePath = x.ImagePath,
                                         CreatedDate = x.CreatedDate,
                                         Type = x.Type,
                                         Name = x.Name,
                                         Code = x.Code,
                                         InstructionContent = x.InstructionContent,
                                     });
            int totalItem = await extraPracticeQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await extraPracticeQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<ExtraPracticeSuggestModel>(lists.OrderBy(x => random.Next()).ToList(), request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
