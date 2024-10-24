// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.FselRatingCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.FselRatings;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;
    public class CreateFselRatingCommand : CreateFselRatingCommandModel, IRequest<MethodResult<FselRatingModel>>
    {
    }

    public class CreateFselRatingCommandHandler : IRequestHandler<CreateFselRatingCommand, MethodResult<FselRatingModel>>
    {
        private readonly IFselRatingRepository _fselRatingRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        public CreateFselRatingCommandHandler(IFselRatingRepository fselRatingRepository, IMapper mapper, AuthContext authContext)
        {
            _fselRatingRepository = fselRatingRepository;
            _mapper = mapper;
            _authContext = authContext;
        }

        public async Task<MethodResult<FselRatingModel>> Handle(CreateFselRatingCommand request, CancellationToken cancellationToken)
        {
            MethodResult<FselRatingModel> methodResult = new MethodResult<FselRatingModel>();
            var userRating = await _fselRatingRepository.Queryable.FirstOrDefaultAsync(x => x.CreatedUserId == _authContext.CurrentUserId && x.DeviceCode == x.DeviceCode && !x.IsRating && x.AmountRating > 0, cancellationToken);
            bool isUpdate = false;
            FselRating newFselRating = new FselRating();

            var checkExists = await _fselRatingRepository.Queryable.AnyAsync(x => x.CreatedUserId == _authContext.CurrentUserId || x.DeviceCode == x.DeviceCode, cancellationToken);
            if (checkExists && userRating == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumErrorFselRating.DuplicateUserInSameDevice));
                return methodResult;
            }


            if (userRating == null)
            {
                newFselRating = _mapper.Map<FselRating>(request);
                newFselRating.AmountRating = MaxSendingRateApp;
            }
            else
            {
                userRating.AmountRating = userRating.AmountRating - 1;
                newFselRating = _mapper.Map(request, userRating);
                isUpdate = true;
            }


            await _fselRatingRepository.ExecuteTransactionAsync(async () =>
            {
                if (!isUpdate)
                {
                    _fselRatingRepository.Add(newFselRating);
                }
                else
                {
                    _fselRatingRepository.Update(newFselRating);
                }

                await _fselRatingRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<FselRatingModel>(newFselRating);
                return methodResult;
            });
            return methodResult;
        }
    }
}
