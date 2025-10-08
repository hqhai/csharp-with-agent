// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.ContinuePTHandlers;
    using MediatR;

    public class ContinuePTCommand : IRequest<MethodResult<PTStateModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class ContinuePTCommandHandler : IRequestHandler<ContinuePTCommand, MethodResult<PTStateModel>>
    {
        private readonly IContinutePTChainFactory _continutePTChainFactory;

        public ContinuePTCommandHandler(IContinutePTChainFactory continutePTChainFactory)
        {
            _continutePTChainFactory = continutePTChainFactory;
        }

        public async Task<MethodResult<PTStateModel>> Handle(ContinuePTCommand request, CancellationToken cancellationToken)
        {
            var chainHandlers = _continutePTChainFactory.GeContinutePTChainHandler();
            if (chainHandlers == null)
            {
                return new MethodResult<PTStateModel>
                {
                    StatusCode = 400
                };
            }

            var context = new ContinuePTTestContext
            {
                StudentId = request.StudentId,
            };

            await chainHandlers.Handle(context);

            return new MethodResult<PTStateModel>
            {
                Result = context.PTState
            };
        }
    }
}
