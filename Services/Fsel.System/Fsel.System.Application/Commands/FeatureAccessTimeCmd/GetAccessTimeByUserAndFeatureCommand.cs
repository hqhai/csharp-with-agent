// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.FeatureAccessTimeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Collections.Generic;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetAccessTimeByUserAndFeatureCommand : IRequest<MethodResult<IList<FeatureAccessTimeModel>>>
    {
        public IList<Guid>? UserIds { get; set; }

        public IList<EnumFeature>? Features { get; set; }
    }

    public class GetAccessTimeByUserAndFeatureCommandHandler : IRequestHandler<GetAccessTimeByUserAndFeatureCommand, MethodResult<IList<FeatureAccessTimeModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;

        public GetAccessTimeByUserAndFeatureCommandHandler(IMapper mapper, IFeatureAccessTimeRepository featureAccessTimeRepository)
        {
            _mapper = mapper;
            _featureAccessTimeRepository = featureAccessTimeRepository;
        }

        public async Task<MethodResult<IList<FeatureAccessTimeModel>>> Handle(GetAccessTimeByUserAndFeatureCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.UserIds);
            ArgumentNullException.ThrowIfNull(request.Features);
            MethodResult<IList<FeatureAccessTimeModel>> methodResult = new MethodResult<IList<FeatureAccessTimeModel>>();

            var featureAccessTimes = await _featureAccessTimeRepository.ReadQueryable
                                                                       .Where(x => request.UserIds.Contains(x.CreatedUserId) && request.Features.Contains(x.EnumFeature))
                                                                       .ToListAsync(cancellationToken);

            if (featureAccessTimes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(featureAccessTimes));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<FeatureAccessTimeModel>>(featureAccessTimes);
            return methodResult;
        }
    }
}
