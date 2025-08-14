// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.QuestionCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Infrastructure.Common.QuestionHelper.QuestionTypes;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class QuestionTypeHandlerCommand : QuestionResultQueueModel, IRequest<MethodResult<bool>>
    {
    }

    public class QuestionTypeHandlerCommandHandler : IRequestHandler<QuestionTypeHandlerCommand, MethodResult<bool>>
    {
        private readonly QuestionTypeFactory _questionTypeFactory;

        public QuestionTypeHandlerCommandHandler(QuestionTypeFactory questionTypeFactory)
        {
            _questionTypeFactory = questionTypeFactory;
        }

        public async Task<MethodResult<bool>> Handle(QuestionTypeHandlerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var handler = _questionTypeFactory.Create(request);
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
