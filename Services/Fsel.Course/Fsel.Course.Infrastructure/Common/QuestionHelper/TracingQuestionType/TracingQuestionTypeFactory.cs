// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.QuestionHelper.TracingQuestionType
{
    using Fsel.Course.Infrastructure.Common.QuestionHelper.TracingQuestionType.Handler;
    using Fsel.Course.Infrastructure.Common.QuestionHelper.TracingQuestionType.Interface;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models;
    using Microsoft.Extensions.DependencyInjection;

    public class TracingQuestionTypeFactory
    {
        private readonly IServiceProvider _sp;
        public TracingQuestionTypeFactory(IServiceProvider sp) => _sp = sp;

        public ITracingQuestionType Create(QuestionResultQueueModel request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var handlerType = request.Type switch
            {
                EnumQuestionResultType.Video => typeof(TracingQuestionTypeVideo),
                EnumQuestionResultType.HomeWork => typeof(TracingQuestionTypeHomeWork),
                //EnumQuestionResultType.Test => typeof(QuestionResultTypeTest),
                _ => throw new NotSupportedException($"Unsupported {request.Type}")
            };

            return (ITracingQuestionType)ActivatorUtilities.CreateInstance(_sp, handlerType, request);
        }
    }
}
