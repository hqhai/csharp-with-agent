// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.RubyQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common.RubyHelpers;
    using Fsel.Course.Lms.Application.Services.RubyService;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RenderRubyCommand : IRequest<MethodResult<object>>
    {
        public string HostType { get; set; }
        public Guid HostId { get; set; }
        public string FieldKey { get; set; }
        public string BaseText { get; set; }
    }

    public class RenderRubyCommandHandler : IRequestHandler<RenderRubyCommand, MethodResult<object>>
    {
        private readonly IRubyScopeRepository _rubyScopeRepository;
        private readonly IRubyService _rubyService;

        public RenderRubyCommandHandler(IRubyScopeRepository rubyScopeRepository, IRubyService rubyService)
        {
            _rubyScopeRepository = rubyScopeRepository;
            _rubyService = rubyService;
        }

        public async Task<MethodResult<object>> Handle(RenderRubyCommand request, CancellationToken cancellationToken)
        {
            var nfc = RubyTextNormalization.ToNfc(request.BaseText);
            MethodResult<object> methodResult = new MethodResult<object>();
            var scope = await _rubyScopeRepository.Queryable.Include(x => x.RubyAnnotations)
                .FirstOrDefaultAsync(x => x.HostType == request.HostType && x.HostId == request.HostId && x.FieldKey == request.FieldKey, cancellationToken);
            if (scope != null && scope.Text != RubyTextNormalization.Sha256Hex(nfc))
            {
                foreach (var item in scope.RubyAnnotations.Where(x => !x.IsDeleted))
                {
                    var newStart = _rubyService.TryReAnchor(nfc, item);
                    if (newStart.HasValue) 
                    {
                        item.StartGraphemeIndex = newStart.Value;
                    }
                }

                (scope.Text, scope.BaseLengthGraphemes) = _rubyService.Snapshot(nfc);
                await _rubyScopeRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }

            var rubies = scope?.RubyAnnotations ?? Enumerable.Empty<RubyAnnotation>();

            methodResult.Result = _rubyService.RenderHtml(nfc, rubies);
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
