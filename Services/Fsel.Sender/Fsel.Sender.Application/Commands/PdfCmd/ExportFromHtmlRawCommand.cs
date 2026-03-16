// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Commands.PdfCmd
{
    using System.Globalization;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Sender.Application.Services;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class ExportFromHtmlRawCommand : ExportPdfByTemplateCommandModel, IRequest<MethodResult<byte[]>>
    {
    }

    public class ExportFromHtmlRawQueryHandler : IRequestHandler<ExportFromHtmlRawCommand, MethodResult<byte[]>>
    {
        public async Task<MethodResult<byte[]>> Handle(ExportFromHtmlRawCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<byte[]>();

            if (request.Params == null)
            {
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

            var handleResult = await PdfHelper.ConvertHtmlToPdf(body, option =>
            {
                option.Landscape = request.Landscape;

                option.MarginOptions = new PuppeteerSharp.Media.MarginOptions()
                {
                    Top = request.Top,
                    Bottom = request.Bottom,
                    Right = request.Right,
                    Left = request.Left,
                };
            });

            handleResult.Result = handleResult.Result;
            return handleResult;
        }
    }
}
