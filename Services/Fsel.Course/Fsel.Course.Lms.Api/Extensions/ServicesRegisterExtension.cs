// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Api.Extensions
{
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.ContinuePTHandlers;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LoadPTTestResultsHandler;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.TestRequestHandler;

    public static class ServicesRegisterExtension
    {
        public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IFlowService, FlowService>();
            services.AddScoped<ITestService, TestService>();
            return services;
        }

        public static IServiceCollection RegisterTestRequestHandlers(this IServiceCollection services)
        {
            if (!services.Any(x => x.ServiceType == typeof(IFlowService)))
            {
                services.RegisterApplicationServices();
            }

            services.AddTransient<ITestRequestChainFactory, TestRequestChainFactory>();
            services.AddTransient<ITestRequestCreateAnswerHandler, TestRequestCreateAnswerHandler>();
            services.AddTransient<ITestRequestGetCommonInfoHandler, TestRequestGetCommonInfoHandler>();
            services.AddTransient<ITestRequestGetTestDataHandler, TestRequestGetTestDataHandler>();
            services.AddTransient<ITestRequestStartNewModuleHandler, TestRequestStartNewModuleHandler>();
            services.AddTransient<ITestRequestSubmitHandler, TestRequestSubmitHandler>();
            services.AddTransient<ITestRequestValidateHandler, TestRequestValidateHandler>();

            return services;
        }

        public static IServiceCollection RegisterPTHandlers(this IServiceCollection services)
        {
            if (!services.Any(x => x.ServiceType == typeof(ITestRequestChainFactory)))
            {
                services.RegisterTestRequestHandlers();
            }

            services.AddTransient<ILoadPTResultChainFactory, LoadPTResultChainFactory>();
            services.AddTransient<ILoadQuestionDataHandler, LoadQuestionDataHandler>();
            services.AddTransient<ILoadTestExcerciseResultHandler, LoadTestExcerciseResultHandler>();
            services.AddTransient<ILoadTestGroupResultHandler, LoadTestGroupResultHandler>();
            services.AddTransient<ILoadTestResultHandler, LoadTestResultHandler>();
            services.AddTransient<ILoadTestSkillResultHandler, LoadTestSkillResultHandler>();

            services.AddTransient<IContinutePTChainFactory, ContinutePTChainFactory>();
            services.AddTransient<IGetCurrentStateHandler, GetCurrentStateHandler>();
            services.AddTransient<IGetTestDataHandler, GetTestDataHandler>();
            services.AddTransient<IInitForNextTestHandler, InitForNextTestHandler>();

            return services;
        }
    }
}
