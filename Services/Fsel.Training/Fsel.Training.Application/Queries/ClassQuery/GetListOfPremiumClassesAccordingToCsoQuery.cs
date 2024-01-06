// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using Fsel.Common.ActionResults;
    using MediatR;
    using Fsel.Training.Domain.Models.EntityModels;
    using System.Threading.Tasks;
    using System.Threading;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Application.Services.OrderServices;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using AutoMapper;
    using Fsel.Core.Base;

    public class GetListOfPremiumClassesAccordingToCsoQuery : IRequest<MethodResult<IList<ClassModel>>>
    {
    }

    public class GetListOfPremiumClassesAccordingToCsoQueryHandler : IRequestHandler<GetListOfPremiumClassesAccordingToCsoQuery, MethodResult<IList<ClassModel>>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetListOfPremiumClassesAccordingToCsoQueryHandler(IClassRepository classRepository, IOrderService orderService, IMapper mapper, AuthContext authContext)
        {
            _classRepository = classRepository;
            _orderService = orderService;
            _mapper = mapper;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<ClassModel>>> Handle(GetListOfPremiumClassesAccordingToCsoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ClassModel>>();

            var packagesResult = await _orderService.GetPackages();
            var idPackagePremium = packagesResult.Content?.Result?.FirstOrDefault(p => p.Code == EnumPackageCode.PREMIUM)?.Id;

            var classes = await _classRepository.Queryable.Where(p => p.Status == EnumClassStatus.Active && p.PackageId == idPackagePremium && p.CsoId == _authContext.CurrentUserId).ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<ClassModel>>(classes);
            return methodResult;
        }
    }
}
