// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Commands.SendSMSCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Sender.Domain.Models.Commands;
    using Fsel.Sender.Domain.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;

    public class SendSMSCommand : SendSMSCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendSMSCommandHandler : IRequestHandler<SendSMSCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;

        public SendSMSCommandHandler(IMediator mediator, AppSetting appSetting)
        {
            _mediator = mediator;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(SendSMSCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var brand = _appSetting.SMSConfig?.UseBrand;

            if (!brand.HasValue)
            {
                return methodResult;
            }

            if (brand.Value == EnumSMSBrand.IRIS)
            {
                await _mediator.Send(new SendSMSByIRISCommand()
                {
                    PhoneNumbers = request.PhoneNumbers,
                    Content = request.Content,
                    Template = request.Template,
                    Params = request.Params,
                    IsCheckDuplicate = request.IsCheckDuplicate,
                    Priority = request.Priority,
                }, cancellationToken);
            }
            else if (brand.Value == EnumSMSBrand.GAPIT)
            {
                await _mediator.Send(new SendSMSByGAPITCommand()
                {
                    PhoneNumbers = request.PhoneNumbers,
                    Content = request.Content,
                    Template = request.Template,
                    Params = request.Params,
                    IsCheckDuplicate = request.IsCheckDuplicate,
                    Priority = request.Priority,
                }, cancellationToken);
            }
            return methodResult;
        }
    }
}
