// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Sender.Domain.Models.Commands;
using Fsel.Sender.Domain.ValueSettings;
using Fsel.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Sender.Application.Commands.SendEmailCmd
{
    public class SendEmailByTemplateCommand : SendEmailByTemplateCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendEmailByTemplateCommandHandler : IRequestHandler<SendEmailByTemplateCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;

        public SendEmailByTemplateCommandHandler(IMediator mediator, AppSetting appSetting)
        {
            _mediator = mediator;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(SendEmailByTemplateCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            if (request.Template == null || request.Params == null)
            {
                methodResult.Result = false;
                return methodResult;
            }

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(CultureInfo.InvariantCulture, SenderSettings.TemplateFileName, request.Template.ToString()));
            using StreamReader streamReader = new StreamReader(path);
            var body = await streamReader.ReadToEndAsync(cancellationToken);

            var @params = ObjectHelper.GetDictionary(request.Params);
            @params.ForEach(item =>
            {
                body = body.Replace($"[{item.Key}]", item.Value, StringComparison.CurrentCultureIgnoreCase);
            });

            methodResult = await _mediator.Send(new SendEmailCommand
            {
                Subject = request.Subject,
                ToEmails = request.ToEmails,
                BccEmails = request.BccEmails,
                CcEmails = request.CcEmails,
                Content = body,
                IsCCEmail = request.IsCCEmail,
                IsCCEmailDefault = request.IsCCEmailDefault,
                Template = request.Template,
                Receivers = request.Receivers
            }, cancellationToken);

            #endregion Validation

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
