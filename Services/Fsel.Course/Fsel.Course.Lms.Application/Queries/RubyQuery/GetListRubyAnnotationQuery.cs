// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.RubyQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Fsel.Core.Base.BaseModels;
    using System.Threading.Tasks;
    using System.Threading;
    using Microsoft.EntityFrameworkCore;

    public class GetListRubyAnnotationQuery : BaseQueryModel, IRequest<MethodResult<object>>
    {
    }

    public class GetListRubyAnnotationQueryHandler : IRequestHandler<GetListRubyAnnotationQuery, MethodResult<object>>
    {
        private readonly IRubyAnnotationRepository _rubyAnnotationRepository;

        public GetListRubyAnnotationQueryHandler(IRubyAnnotationRepository rubyAnnotationRepository)
        {
            _rubyAnnotationRepository = rubyAnnotationRepository;
        }

        public async Task<MethodResult<object>> Handle(GetListRubyAnnotationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<object>();
            var rubyTexts = await _rubyAnnotationRepository.Queryable
                .OrderBy(x => x.CreatedDate)
                .Select(x => new
                {
                    BaseText = x.SelectedText,
                    x.Phonetic,
                }).ToListAsync(cancellationToken);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = rubyTexts;
            return methodResult;
        }
    }
}
