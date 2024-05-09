// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.SendMail
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.SenderServices;
    using Fsel.System.Application.Services.StorageServices;
    using global::System.Globalization;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Refit;

    public class SendMailPreInterviewCommand : IRequest<VoidMethodResult>
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public IFormFile? Video { get; set; }
        public IFormFile? Lesson { get; set; }
        public IFormFile? Activity { get; set; }
    }

    public class SendMailPreInterviewCommandHandler : IRequestHandler<SendMailPreInterviewCommand, VoidMethodResult>
    {
        private readonly IStorageService _storageService;
        private readonly ISenderService _senderService;

        private const string TemplateForRecruitment = "<!doctype html>\r\n<html>\r\n<body style=\"font-family: Arial, Helvetica, sans-serif;\">\r\n  You have received a pre-interview task submission.<br>\r\n  Role: Five-Star in-School teacher<br>\r\n  Name: {0} <br>\r\n  Email: {1} <br>\r\n  Task 1 - Video introduction: {2} <br>\r\n  Task 2 - Lesson plan evaluation: {3} <br>\r\n  Task 3 - Activity setup: {4} <br>\r\n</body>\r\n</html>\r\n";
        private const string TemplateForCustomer = "<!doctype html>\r\n<html>\r\n<body style=\"font-family:Arial, Helvetica, sans-serif;\">\r\n  Thank you for submitting your pre-interview tasks for the role of Five-Star in-School teacher. We will review your tasks, and get back to you soon.\r\n<br><br>Best regards,<br>\r\nAtlantic Five-Star English\r\n</body>\r\n</html>\r\n";

        private const string MailRecruitment = "recruitment@atlantic.edu.vn";

        public SendMailPreInterviewCommandHandler(IStorageService storageService, ISenderService senderService)
        {
            _storageService = storageService;
            _senderService = senderService;
        }

        public async Task<VoidMethodResult> Handle(SendMailPreInterviewCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new VoidMethodResult();

            if (request.Video == null || request.Lesson == null || request.Activity == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Name) || !request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            List<Task<string?>> tasks = new List<Task<string?>>();

            var videoLinkTask = UpLoadFile(request.Video);
            tasks.Add(videoLinkTask!);

            var lessonLinkTask = UpLoadFile(request.Lesson);
            tasks.Add(lessonLinkTask!);

            var activityLinkTask = UpLoadFile(request.Activity);
            tasks.Add(activityLinkTask!);

            await Task.WhenAll(tasks);
            var videoLink = videoLinkTask.Result;

            if (string.IsNullOrEmpty(videoLink))
            {
                methodResult.AddErrorBadRequest("Upload file Video error");
                return methodResult;
            }
            var lessonLink = lessonLinkTask.Result;
            if (string.IsNullOrEmpty(lessonLink))
            {
                methodResult.AddErrorBadRequest("Upload file Lesson error");
                return methodResult;
            }
            var activityLink = activityLinkTask.Result;
            if (string.IsNullOrEmpty(activityLink))
            {
                methodResult.AddErrorBadRequest("Upload file Activity error");
                return methodResult;
            }
            var content = string.Format(CultureInfo.InvariantCulture, TemplateForRecruitment, request.Name, request.Email, videoLink, lessonLink, activityLink);

            await _senderService.SendEmailUsingSMTP(new SendEmailCommandModel
            {
                ToEmails = new List<string> { MailRecruitment },
                Content = content,
                Subject = string.Format(CultureInfo.InvariantCulture, "Pre-interview task submission - {0}", request.Name)
            });

            await _senderService.SendEmailUsingSMTP(new SendEmailCommandModel
            {
                ToEmails = new List<string> { request.Email },
                Content = TemplateForCustomer,
                Subject = "Atlantic Careers - Pre-interview task submitted successfully"
            });

            return methodResult;
        }

        private async Task<string?> UpLoadFile(IFormFile file)
        {
            var stream = file.OpenReadStream();
            var streamPart = new StreamPart(stream, file.FileName, file.ContentType);

            var uploadFileResult = await _storageService.UpLoadFile(EnumFolderType.Files, EnumBucketType.Recruitment, streamPart);

            if (uploadFileResult.IsSuccessStatusCode)
            {
                return uploadFileResult.Content?.Result;
            }

            return string.Empty;
        }
    }
}
