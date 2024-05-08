// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Commands.SendEmailCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.SimpleEmail;
    using Amazon.SimpleEmail.Model;
    using Fsel.Common.ActionResults;
    using Fsel.Sender.Domain.Models.Commands;
    using Fsel.Sender.Domain.ValueSettings;
    using MediatR;
    using MimeKit;

    public class SendEmailWithAttachmentsCommand : SendEmailCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class SendEmailWithAttachmentsCommandHandler : IRequestHandler<SendEmailWithAttachmentsCommand, MethodResult<bool>>
    {
        private readonly AppSetting _appSetting;

        public SendEmailWithAttachmentsCommandHandler(AppSetting appSetting)
        {
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(SendEmailWithAttachmentsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            await SendEmails(request, _appSetting);
            return methodResult;
        }

        private static BodyBuilder GetMessageBody(SendEmailCommandModel model)
        {
            var body = new BodyBuilder()
            {
                HtmlBody = model.Content,
            };

            model.Attachments.ForEach(file =>
            {
                var stream = file.OpenReadStream();
                body.Attachments.Add(file.FileName, stream);
            });

            return body;
        }

        private static MimeMessage GetMessage(SendEmailCommandModel model, AppSetting appSetting)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(string.Empty, appSetting.Smtp?.From));

            model.ToEmails.ForEach(email =>
            {
                message.To.Add(new MailboxAddress(string.Empty, email));
            });

            message.Subject = model.Subject;
            message.Body = GetMessageBody(model).ToMessageBody();
            return message;
        }

        private static MemoryStream GetMessageStream(SendEmailCommandModel model, AppSetting appSetting)
        {
            using (var message = GetMessage(model, appSetting))
            {
                var stream = new MemoryStream();
                message.WriteTo(stream);
                return stream;
            }
        }

        private static async Task SendEmails(SendEmailCommandModel model, AppSetting appSetting)
        {
            using (var client = new AmazonSimpleEmailServiceClient(appSetting.Smtp?.AwsAccessKeyId, appSetting.Smtp?.AwsSecretAccessKey, RegionEndpoint.APSoutheast1))
            {
                var sendRequest = new SendRawEmailRequest { RawMessage = new RawMessage(GetMessageStream(model, appSetting)) };
                try
                {
                    var response = await client.SendRawEmailAsync(sendRequest);
                    Console.WriteLine("The email was sent successfully.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("The email was not sent.");
                    Console.WriteLine("Error message: " + e.Message);
                }
            }
        }
    }
}
