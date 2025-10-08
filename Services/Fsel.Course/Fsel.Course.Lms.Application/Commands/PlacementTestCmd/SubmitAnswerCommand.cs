// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.TestRequestHandler;
    using MediatR;

    public class SubmitAnswerCommand : IRequest<MethodResult<PTStateModel>>
    {
        public Guid StudentId { get; set; }

        public Guid SectionResultId { get; set; }

        public IList<TestAnswerQuestionModel>? Answers { get; set; }

        public bool IsSubmit { get; set; }
    }

    public class SubmitAnswerCommandHandler : IRequestHandler<SubmitAnswerCommand, MethodResult<PTStateModel>>
    {
        private readonly ITestRequestChainFactory _testRequestChainFactory;

        public SubmitAnswerCommandHandler(ITestRequestChainFactory testRequestChainFactory)
        {
            _testRequestChainFactory = testRequestChainFactory;
        }

        public async Task<MethodResult<PTStateModel>> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
        {
            var chainHandlers = _testRequestChainFactory.GetTestSubmitRequestChainHandlers();

            if (chainHandlers == null)
            {
                return new MethodResult<PTStateModel>
                {
                    StatusCode = 400
                };
            }

            var context = new TestRequestContext
            {
                TestRequestCommand = new SubmitAnswerCommandModel
                {
                    Answers = request.Answers,
                    SectionResultId = request.SectionResultId,
                    StudentId = request.StudentId,
                    IsSubmit = request.IsSubmit
                }
            };

            await chainHandlers.Handle(context);

            if (context.MethodResult.ErrorMessages.Count == 0)
            {
                context.MethodResult.Result = context.PTState;
            }

            return context.MethodResult;
        }
    }
}
