// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.SkillQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchSkillQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<SkillModel>>>
    {
    }

    public class SearchSkillQueryHandler : IRequestHandler<SearchSkillQuery, MethodResult<PagingItemsModel<SkillModel>>>
    {
        private readonly ISkillRepository _skillRepository;

        public SearchSkillQueryHandler(ISkillRepository skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<PagingItemsModel<SkillModel>>> Handle(SearchSkillQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SkillModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var query = _skillRepository.Queryable;
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var queryCode = query.Where(x => x.Code != null && x.Code.Contains(request.Keyword));
                var queryName = query.Where(x => x.Name != null && x.Name.Contains(request.Keyword));
                query = queryCode.Union(queryName);
            }
            var querySkill = query.Select(x => new SkillModel
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Description = x.Description,
                FilePath = x.FilePath,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate,
                CreatedFullName = x.CreatedFullName,
                IsActive = x.SkillLevels.Any(),
            });
            int totalItem = await querySkill.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await querySkill
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<SkillModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
