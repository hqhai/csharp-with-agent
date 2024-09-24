// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lcms.Application.Commands.ClassForumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.ProsodyCommandModel;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;


    public class CreateProsodyRangeCommand : CreateProsodyCommandModels, IRequest<MethodResult<ProsodyScoreModel>>
    {

    }

    public class CreateProsodyRangeCommandHandler : IRequestHandler<CreateProsodyRangeCommand, MethodResult<ProsodyScoreModel>>
    {
        private readonly IMediator mediator;
        private readonly IMapper _mapper;
        public CreateProsodyRangeCommandHandler(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            _mapper = mapper;
        }

        public async Task<MethodResult<ProsodyScoreModel>> Handle(CreateProsodyRangeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ProsodyScoreModel> methodResult = new MethodResult<ProsodyScoreModel>();

            IList<ProsodyScore> prosodyScores = new List<ProsodyScore>();


            return methodResult;
        }
    }
}
