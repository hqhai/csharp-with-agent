// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.RubyTextCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Fsel.Course.Domain.Models.CommandModels.RubyText;

    public class UpdateRubyTextCommand : UpdateRubyTextCommandModel, IRequest<MethodResult<RubyTextModel>>
    {
    }

    public class UpdateRubyTextCommandHanlder : IRequestHandler<UpdateRubyTextCommand, MethodResult<RubyTextModel>>
    {
        private readonly IRubyTextRepository _rubyTextRepository;
        private readonly IMapper _mapper;

        public UpdateRubyTextCommandHanlder(IRubyTextRepository rubyTextRepository, IMapper mapper)
        {
            _rubyTextRepository = rubyTextRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<RubyTextModel>> Handle(UpdateRubyTextCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<RubyTextModel> methodResult = new MethodResult<RubyTextModel>();

            var rubyText = await _rubyTextRepository.GetByIdAsync(request.Id);

            if (rubyText == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(rubyText));
                return methodResult;
            }

            _mapper.Map(request, rubyText);

            if (!rubyText.IsValid())
            {
                methodResult.AddErrorBadRequest(rubyText.ErrorMessages);
                return methodResult;
            }

            await _rubyTextRepository.ExecuteTransactionAsync(async () =>
            {
                rubyText = _rubyTextRepository.Update(rubyText);
                await _rubyTextRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<RubyTextModel>(rubyText);
                return methodResult;
            });

            return methodResult;
        }
    }
}
