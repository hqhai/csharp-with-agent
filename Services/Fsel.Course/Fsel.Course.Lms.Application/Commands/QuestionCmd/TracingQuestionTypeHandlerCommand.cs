// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.QuestionCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Infrastructure.Common.QuestionHelper.TracingQuestionType;
    using Fsel.Shared.Models;
    using MediatR;

    public class TracingQuestionTypeHandlerCommand : QuestionResultQueueModel, IRequest<MethodResult<bool>>
    {
    }

    public class TracingQuestionTypeHandlerCommandHandler : IRequestHandler<TracingQuestionTypeHandlerCommand, MethodResult<bool>>
    {
        private readonly TracingQuestionTypeFactory _questionResultTypeFactory;

        public TracingQuestionTypeHandlerCommandHandler(TracingQuestionTypeFactory questionResultTypeFactory)
        {
            _questionResultTypeFactory = questionResultTypeFactory;
        }

        public async Task<MethodResult<bool>> Handle(TracingQuestionTypeHandlerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var handler = _questionResultTypeFactory.Create(request);
            var result = await handler.ExecuteAsync(cancellationToken);
            if (!result.IsOK)
            {
                methodResult.AddErrorBadRequest(result.ErrorMessages);
                return methodResult;
            }

            methodResult.Result = true;
            return methodResult;
        }
    }
}
