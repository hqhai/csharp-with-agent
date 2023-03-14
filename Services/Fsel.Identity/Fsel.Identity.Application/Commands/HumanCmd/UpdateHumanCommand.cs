// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.HumanCmd
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Models.CommandModels.Humans;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;

    public class UpdateHumanCommand : UpdateHumanCommandModel, IRequest<MethodResult<HumanModel>>
    {
    }

    public class UpdateHumanCommandHandler : IRequestHandler<UpdateHumanCommand, MethodResult<HumanModel>>
    {
        private readonly IMapper _mapper;

        public UpdateHumanCommandHandler(
            IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<MethodResult<HumanModel>> Handle(UpdateHumanCommand request, CancellationToken cancellationToken)
        {
            MethodResult<HumanModel> methodResult = new MethodResult<HumanModel>();
            return methodResult;
        }
    }
}
