// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.RubyScopeCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.RubyScope;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common.RubyHelpers;
    using Fsel.Course.Lms.Application.Services.RubyService;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateRubyScopeCommand : CreateRubyScopeCommandModel, IRequest<MethodResult<RubyScopeModel>>
    {
    }

    public class RubyDocumentCommandHandle : IRequestHandler<CreateRubyScopeCommand, MethodResult<RubyScopeModel>>
    {
        private readonly IRubyScopeRepository _rubyScopeRepository;
        private readonly IMapper _mapper;
        private readonly IRubyService _rubyService;

        public RubyDocumentCommandHandle(IRubyScopeRepository rubyScopeRepository, IMapper mapper, IRubyService rubyService)
        {
            _rubyScopeRepository = rubyScopeRepository;
            _mapper = mapper;
            _rubyService = rubyService;
        }

        public async Task<MethodResult<RubyScopeModel>> Handle(CreateRubyScopeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<RubyScopeModel>();

            RubyScope rubyScope = _mapper.Map<RubyScope>(request);

            if (rubyScope == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var nfc = RubyTextNormalization.ToNfc(rubyScope.Text);
            (rubyScope.Text, rubyScope.BaseLengthGraphemes) = _rubyService.Snapshot(nfc);

            await _rubyScopeRepository.ExecuteTransactionAsync(async () =>
            {
                rubyScope = _rubyScopeRepository.Add(rubyScope);
                await _rubyScopeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<RubyScopeModel>(rubyScope);

                return methodResult;
            });

            return methodResult;
        }
    }
}
