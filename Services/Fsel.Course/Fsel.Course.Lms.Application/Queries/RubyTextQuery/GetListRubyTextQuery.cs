// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.RubyTextQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Fsel.Core.Base.BaseModels;
    using System.Threading.Tasks;
    using System.Threading;
    using Microsoft.EntityFrameworkCore;

    public class GetListRubyTextQuery : BaseQueryModel, IRequest<MethodResult<object>>
    {
    }

    public class GetListRubyTextQueryHandler : IRequestHandler<GetListRubyTextQuery, MethodResult<object>>
    {
        private readonly IRubyTextRepository _rubyTextRepository;

        public GetListRubyTextQueryHandler(IRubyTextRepository rubyTextRepository)
        {
            _rubyTextRepository = rubyTextRepository;
        }

        public async Task<MethodResult<object>> Handle(GetListRubyTextQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();
            var rubyTexts = await _rubyTextRepository.Queryable
                .OrderBy(x => x.CreatedDate)
                .Select(x => new
                {
                    BaseText = x.BaseText,
                    Phonetic = x.Phonetic,
                }).ToListAsync(cancellationToken);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = rubyTexts;
            return methodResult;
        }
    }
}
