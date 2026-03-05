// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using PuppeteerSharp;
    using PuppeteerSharp.Media;

    public static class PdfHelper
    {
        /// <summary>
        /// Call back phục vụ cho hàm xử lý nhiều view ra file zip
        /// </summary>
        /// <typeparam name="T">Kiểu model</typeparam>
        /// <param name="html">Chuỗi html trong mỗi lần xử lý view</param>
        /// <param name="model">Model</param>
        /// <returns></returns>
        private delegate Task HandleHtmlStringCallBack<T>(string html, T model);

        public static async Task<MethodResult<byte[]>> ConvertHtmlToPdf(string html, Action<PdfOptions>? configOption)
        {
            var methodResult = new MethodResult<byte[]>();

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
            });

            using var page = await browser.NewPageAsync();

            await page.SetContentAsync(html);

            var option = new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
            };

            configOption?.Invoke(option);

            var pdfBytes = await page.PdfDataAsync(option);

            await browser.DisposeAsync();

            methodResult.Result = pdfBytes;

            return methodResult;
        }

        public static async Task<MethodResult<byte[]>> ConvertViewToPdf<T>(T viewModel, string view, IRazorViewEngine viewEngine, IServiceProvider serviceProvider, ITempDataProvider tempDataProvider, Action<PdfOptions>? configOption)
        {
            var methodResult = new MethodResult<byte[]>();

            var html = await GetHtmlFromRazorView(viewModel, view, viewEngine, serviceProvider, tempDataProvider);

            if (!html.IsOK)
            {
                methodResult.AddErrorServer();
                return methodResult;
            }

            Console.WriteLine(html.Result);

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
            });

            using var page = await browser.NewPageAsync();

            // Thiết lập nội dung HTML cho trang
            await page.SetContentAsync(html.Result);

            var option = new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
            };

            configOption?.Invoke(option);

            var pdfBytes = await page.PdfDataAsync(option);

            await browser.DisposeAsync();

            methodResult.Result = pdfBytes;

            return methodResult;
        }

        private static async Task<MethodResult<string>> GetHtmlFromRazorView<T>(T model, string view, IRazorViewEngine viewEngine, IServiceProvider serviceProvider, ITempDataProvider tempDataProvider)
        {
            var methodResult = new MethodResult<string>();
            if (viewEngine == null || string.IsNullOrEmpty(view))
            {
                methodResult.AddErrorServer();
                return methodResult;
            }

            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };

            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new ActionDescriptor());

            var viewResult = viewEngine.GetView("", view, isMainPage: true);

            if (!viewResult.Success)
            {
                methodResult.AddErrorServer();
                return methodResult;
            }

            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model,
            };

            var tempData = new TempDataDictionary(actionContext.HttpContext, tempDataProvider);

            var sw = new StringWriter();

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);

            methodResult.Result = sw.ToString();

            await sw.DisposeAsync();

            return methodResult;
        }
    }
}
