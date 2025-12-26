// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.RubyQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.QueryModels.Ruby;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RubyResponseQuery : IRequest<MethodResult<RubyResponseModel>>
    {
        public Guid ObjectId { get; set; }
    }

    public class RubyResponseQueryHandler : IRequestHandler<RubyResponseQuery, MethodResult<RubyResponseModel>>
    {
        private readonly IRubyAnnotationRepository _rubyAnnotationRepository;
        private readonly IRubyScopeRepository _rubyScopeRepository;

        public RubyResponseQueryHandler(IRubyAnnotationRepository rubyAnnotationRepository,
            IRubyScopeRepository rubyScopeRepository)
        {
            _rubyAnnotationRepository = rubyAnnotationRepository;
            _rubyScopeRepository = rubyScopeRepository;
        }

        public async Task<MethodResult<RubyResponseModel>> Handle(RubyResponseQuery request, CancellationToken cancellationToken)
        {
            MethodResult<RubyResponseModel> methodResult = new MethodResult<RubyResponseModel>();
            ArgumentNullException.ThrowIfNull(request);

            var scope = await _rubyScopeRepository.Queryable
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ObjectId == request.ObjectId, cancellationToken);

            if (scope == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(scope));
                return methodResult;
            }

            var resultResponr = new RubyResponseModel();
            resultResponr.ObjectId = request.ObjectId;
            resultResponr.Annotaions = await _rubyAnnotationRepository.Queryable
                .AsNoTracking()
                .Where(x => x.RubyScopeId == scope.Id)
                .Select(x => new AnnotaionResponse
                {
                    Text = x.SelectedText,
                    Phonetic = x.TextNote,
                    Lang = x.LanguageType,
                }).ToListAsync(cancellationToken);

            methodResult.Result = resultResponr;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
