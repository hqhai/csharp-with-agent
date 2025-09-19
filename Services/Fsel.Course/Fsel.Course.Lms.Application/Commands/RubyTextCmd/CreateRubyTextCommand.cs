// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.RubyTextCmd
{
    using AutoMapper;
    using Fsel.Course.Domain.Entities;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using EntityRubyText = Domain.Entities.RubyText;
    using Microsoft.AspNetCore.Http;
    using Fsel.Course.Domain.Models.CommandModels.RubyText;

    public class CreateRubyTextCommand : UpdateRubyTextCommandModel, IRequest<MethodResult<RubyTextModel>>
    {
    }

    public class RubyTextCommandHandler : IRequestHandler<CreateRubyTextCommand, MethodResult<RubyTextModel>>
    {
        private readonly IRubyTextRepository _rubyTextRepository;
        private readonly ILogger<CreateRubyTextCommand> _logger;
        private readonly IMapper _mapper;

        public RubyTextCommandHandler(IRubyTextRepository rubyTextRepository,
            ILogger<CreateRubyTextCommand> logger,
            IMapper mapper)
        {
            _rubyTextRepository = rubyTextRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<MethodResult<RubyTextModel>> Handle(CreateRubyTextCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<RubyTextModel> methodResult = new MethodResult<RubyTextModel>();

            var rubyText = _mapper.Map<EntityRubyText>(request);

            if (!rubyText.IsValid())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _rubyTextRepository.ExecuteTransactionAsync(async () =>
            {
                rubyText = _rubyTextRepository.Add(rubyText);
                await _rubyTextRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<RubyTextModel>(rubyText);
                return methodResult;
            });

            return methodResult;
        }
    }
}
