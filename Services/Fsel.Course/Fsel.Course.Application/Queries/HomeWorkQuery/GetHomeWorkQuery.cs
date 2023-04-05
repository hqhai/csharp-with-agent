// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.HomeWorkQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetHomeWorkQuery : IRequest<MethodResult<HomeWorkModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetHomeWorkQueryHandler : IRequestHandler<GetHomeWorkQuery, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;

        public GetHomeWorkQueryHandler(IMapper mapper, IHomeWorkRepository homeWorkRepository)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(GetHomeWorkQuery request, CancellationToken cancellationToken)
        {
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();
            ArgumentNullException.ThrowIfNull(request);
            var homeWork = await _homeWorkRepository.GetByIdAsync(request.Id);
            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(
                    nameof(EnumHomeWorkErrorCode.HomeWorksNull),
                    nameof(request.Id), request.Id);
                return methodResult;
            }
            var homeWorkModel = _mapper.Map<HomeWorkModel>(homeWork);
            methodResult.Result = homeWorkModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
