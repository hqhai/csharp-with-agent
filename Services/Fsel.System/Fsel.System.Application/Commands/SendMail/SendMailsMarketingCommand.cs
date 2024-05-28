// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.SendMail
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Services.GoogleSheetServices;
    using Fsel.System.Application.Services.SenderServices;
    using Fsel.System.Infrastructure.ValueSettings;
    using global::System.IO;
    using global::System.Text;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Refit;

    public class SendMailsMarketingCommand : IRequest<MethodResult<bool>>
    {
        public string? EmailTest { get; set; }
        public IFormFile? Template { get; set; }
        public string? Subject { get; set; }
        public IList<IFormFile>? Attachments { get; set; }
    }

    public class SendMailsMarketingCommandHandler : IRequestHandler<SendMailsMarketingCommand, MethodResult<bool>>
    {
        private readonly IGoogleSheetService _googleSheetService;
        private readonly ISenderService _senderService;
        private readonly AppSetting _appSetting;

        public SendMailsMarketingCommandHandler(ISenderService senderService, AppSetting appSetting)
        {
            _googleSheetService = new GoogleSheetService(ResourceSettings.I18NCredentialsFilePath);
            _senderService = senderService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(SendMailsMarketingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var spreadSheetId = _appSetting.GoogleSheetConfig?.MailMarketingSpreadSheetId;
            var sheet = _appSetting.GoogleSheetConfig?.MailMarketingSheet;

            if (string.IsNullOrEmpty(spreadSheetId) || string.IsNullOrEmpty(sheet))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            string fileContent = string.Empty;

            if (request.Template != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await request.Template.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (var streamReader = new StreamReader(memoryStream, Encoding.UTF8))
                    {
                        fileContent = await streamReader.ReadToEndAsync(cancellationToken);
                    }
                }
            }

            if (request.Attachments == null || !request.Attachments.Any())
            {
                methodResult.AddErrorBadRequest("No attachments provided.");
                return methodResult;
            }

            var model = new SendEmailCommandModel()
            {
                Subject = request.Subject,
                Content = fileContent,
                Attachments = request.Attachments
            };

            if (!string.IsNullOrEmpty(request.EmailTest))
            {
                model.ToEmails.Add(request.EmailTest);

                IList<StreamPart> streamParts = new List<StreamPart>();
                foreach (var file in request.Attachments)
                {
                    var stream = file.OpenReadStream();
                    var streamPart = new StreamPart(stream, file.FileName, file.ContentType);
                    streamParts.Add(streamPart);
                }
                var result = await _senderService.SendEmailWithAttachments(model.ToEmails, model.BccEmails, model.CcEmails, model.Subject, model.Content, streamParts);
                if (!result.IsSuccessStatusCode)
                {
                    methodResult.AddError(result.Error);
                    return methodResult;
                }
            }
            else
            {
                IList<IList<object>> data = _googleSheetService.ReadDataFromSheet(spreadSheetId, sheet);
                data.RemoveAt(0);

                var email = data.Where(p => p != null && p.FirstOrDefault() != null && !string.IsNullOrEmpty(p.FirstOrDefault()?.ToString())).Select(p => p.FirstOrDefault()!.ToString()!).ToList();

                email = email.Where(p => p.IsValidEmail()).ToList();
                if (email.Count > 0)
                {
                    foreach (var item in email)
                    {
                        IList<StreamPart> streamParts = new List<StreamPart>();
                        foreach (var file in request.Attachments)
                        {
                            var stream = file.OpenReadStream();
                            var streamPart = new StreamPart(stream, file.FileName, file.ContentType);
                            streamParts.Add(streamPart);
                        }

                        model.ToEmails = new List<string> { item };
                        var result = await _senderService.SendEmailWithAttachments(model.ToEmails, model.BccEmails, model.CcEmails, model.Subject, model.Content, streamParts);
                        if (!result.IsSuccessStatusCode)
                        {
                            methodResult.AddError(result.Error);
                            return methodResult;
                        }
                    }
                }
            }

            return methodResult;
        }
    }
}
