// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FselRatingQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Constants;
    using Fsel.System.Application.Commands.FselRatingCmd;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class CheckRatingAppQuery : IRequest<MethodResult<bool>>
    {
        public string? DeviceCode { get; set; }
    }

    public class CheckRatingAppQueryHandler : IRequestHandler<CheckRatingAppQuery, MethodResult<bool>>
    {
        private readonly IFselRatingRepository _fselRatingRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IMediator _mediator;
        private readonly AuthContext _authContext;

        public CheckRatingAppQueryHandler(IFselRatingRepository fselRatingRepository
            , IMapper mapper
            , IUserService userService
            , AuthContext authContext
            , IMediator mediator)
        {
            _fselRatingRepository = fselRatingRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(CheckRatingAppQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<bool> methodResult = new MethodResult<bool>();

            bool isValid = true;
            bool doesRecordExist = await _fselRatingRepository.Queryable
                .AnyAsync(x => x.DeviceCode == request.DeviceCode || x.CreatedUserId == _authContext.CurrentUserId, cancellationToken);

            if (!doesRecordExist)
            {
                await _mediator.Send(new CreateFselRatingCommand
                {
                    DeviceCode = request.DeviceCode,
                    IsRating = false,
                }, cancellationToken).ConfigureAwait(false);
            }

            DateTime dateThreshold = DateTime.UtcNow.AddDays(-FselRatingValue.DelayDateSendingRate).Date;

            int recordCount = await _fselRatingRepository.Queryable
                .CountAsync(x => x.DeviceCode == request.DeviceCode || x.CreatedUserId == _authContext.CurrentUserId, cancellationToken);



            isValid = recordCount <= FselRatingValue.MoreThanOneDevice &&
                           await _fselRatingRepository.Queryable
                               .AnyAsync(x =>
                                   x.DeviceCode == request.DeviceCode &&
                                   x.AmountRating >= 0 &&
                                   (x.UpdatedDate.HasValue ? x.UpdatedDate.Value.Date : x.CreatedDate.Date) <= dateThreshold,
                                   cancellationToken);

            methodResult.Result = isValid;
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
