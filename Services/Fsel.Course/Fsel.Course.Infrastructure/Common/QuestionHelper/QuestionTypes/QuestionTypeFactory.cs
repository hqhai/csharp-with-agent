// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.QuestionHelper.QuestionTypes
{
    using Fsel.Course.Infrastructure.Common.QuestionHelper.QuestionTypes.Handler;
    using Fsel.Course.Infrastructure.Common.QuestionHelper.QuestionTypes.Interface;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.Extensions.DependencyInjection;

    public class QuestionTypeFactory
    {
        private readonly IServiceProvider _sp;
        public QuestionTypeFactory(IServiceProvider sp) => _sp = sp;

        public IQuestionType Create(QuestionResultQueueModel request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var handlerType = request.Type switch
            {
                EnumQuestionResultType.Video => typeof(QuestionTypeVideoHandler),
                EnumQuestionResultType.HomeWork => typeof(QuestionTypeHomeWorkHandler),
                //EnumQuestionResultType.Test => typeof(QuestionResultTypeTest),
                _ => throw new NotSupportedException($"Unsupported {request.Type}")
            };

            return (IQuestionType)ActivatorUtilities.CreateInstance(_sp, handlerType, request);
        }
    }
}
