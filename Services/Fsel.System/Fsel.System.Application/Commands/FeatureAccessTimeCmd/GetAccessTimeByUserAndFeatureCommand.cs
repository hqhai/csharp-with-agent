// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.FeatureAccessTimeCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Collections.Generic;
    using MediatR;

    public class GetAccessTimeByUserAndFeatureCommand : IRequest<MethodResult<IList<FeatureAccessTimeModel>>>
    {
        public IList<Guid>? UserIds { get; set; }

        public IList<EnumFeature>? EnumFeatures{ get; set; }
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
            MethodResult<IList<FeatureAccessTimeModel>> methodResult = new MethodResult<IList<FeatureAccessTimeModel>>();

            var featureAccessTimes = _featureAccessTimeRepository.Queryable.Where(x => request.UserIds.Contains(x.CreatedUserId) && request.EnumFeatures.Contains(x.EnumFeature));

            methodResult.Result = _mapper.Map<IList<FeatureAccessTimeModel>>(featureAccessTimes);
            return methodResult;
        }
    }
}
