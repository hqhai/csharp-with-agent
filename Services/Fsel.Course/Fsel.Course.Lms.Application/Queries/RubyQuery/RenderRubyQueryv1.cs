// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.RubyQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.Ruby;
    using Fsel.Course.Infrastructure.Common.RubyHelpers;
    using Fsel.Course.Lms.Application.Services.RubyService;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RenderRubyQueryv1 : RubyRenderModel, IRequest<MethodResult<object>>
    {
    }

    public class RenderRubyQueryv1Handler : IRequestHandler<RenderRubyQueryv1, MethodResult<object>>
    {
        private readonly IRubyScopeRepository _rubyScopeRepository;
        private readonly IRubyService _rubyService;

        public RenderRubyQueryv1Handler(IRubyScopeRepository rubyScopeRepository, IRubyService rubyService)
        {
            _rubyScopeRepository = rubyScopeRepository;
            _rubyService = rubyService;
        }

        public async Task<MethodResult<object>> Handle(RenderRubyQueryv1 request, CancellationToken cancellationToken)
        {
            var (found, nfc) = await _rubyScopeRepository.TryGetBaseTextAsync(request.HostType, request.HostId, request.FieldKey, cancellationToken);
            MethodResult<object> methodResult = new MethodResult<object>();

            if (!found || string.IsNullOrEmpty(nfc))
            {
                methodResult.AddErrorBadRequest("Scope not found");
                return methodResult;
            }

            var baseNfc = RubyTextNormalization.ToNfc(nfc!);

            var scope = await _rubyScopeRepository.Queryable.Include(x => x.RubyAnnotations)
                .FirstOrDefaultAsync(x => x.HostType == request.HostType && x.HostId == request.HostId && x.FieldKey == request.FieldKey, cancellationToken);
            if (scope == null || scope.RubyAnnotations.All(r => r.IsDeleted))
            {
                var rawHtml = _rubyService.RenderHtml(baseNfc, Enumerable.Empty<RubyAnnotation>());

                methodResult.Result = rawHtml;
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            }

            foreach (var item in scope.RubyAnnotations.Where(x => !x.IsDeleted))
            {
                var newStart = _rubyService.TryReAnchor(baseNfc, item);
                if (newStart.HasValue)
                {
                    item.StartGraphemeIndex = newStart.Value;
                }
            }

            (scope.Text, scope.BaseLengthGraphemes) = _rubyService.Snapshot(nfc);
            await _rubyScopeRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            var rubies = scope?.RubyAnnotations ?? Enumerable.Empty<RubyAnnotation>();
            var htmlResult = _rubyService.RenderHtml(baseNfc, rubies);

            methodResult.Result = htmlResult;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
