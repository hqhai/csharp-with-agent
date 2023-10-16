// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUserByIdQuery : IRequest<MethodResult<HumanModel>>
    {
        public Guid? Id { get; set; }
    }
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, MethodResult<HumanModel>>
    {
        private readonly IHumanRepository _humanRepository;
        private readonly IMapper _mapper;
        public GetUserByIdQueryHandler(IHumanRepository humanRepository, IMapper mapper)
        {
            _humanRepository = humanRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<HumanModel>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HumanModel> methodResult = new MethodResult<HumanModel>();

            var human = await _humanRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == request.Id, cancellationToken);
            if (human == null)
            {
                return methodResult;
            }
            methodResult.Result = _mapper.Map<HumanModel>(human);
            return methodResult;
        }
    }
}
