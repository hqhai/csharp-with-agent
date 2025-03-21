// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TokenHistoryCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Application.Services.UserServices.Models;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.TokenHistorys;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.EntityModels.Configs;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class CreateTokenHistoryCommand : CreateTokenHistoryCommandModel, IRequest<MethodResult<IList<TokenHistoryModel>>>
    {
    }

    public class CreateTokenHistoryCommandHandler : IRequestHandler<CreateTokenHistoryCommand, MethodResult<IList<TokenHistoryModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ITokenHistoryRepository _tokenHistoryRepository;
        private readonly IUserService _userService;
        private readonly ITokenConfigRepository _tokenConfigRepository;

        public CreateTokenHistoryCommandHandler(IMapper mapper, ITokenHistoryRepository tokenHistoryRepository, IUserService userService, ITokenConfigRepository tokenConfigRepository)
        {
            _mapper = mapper;
            _tokenHistoryRepository = tokenHistoryRepository;
            _userService = userService;
            _tokenConfigRepository = tokenConfigRepository;
        }

        public async Task<MethodResult<IList<TokenHistoryModel>>> Handle(CreateTokenHistoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TokenHistoryModel>> methodResult = new MethodResult<IList<TokenHistoryModel>>();

            if (request.TokenHistorys == null || request.TokenHistorys.Count == 0)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(request.TokenHistorys.Select(x => x.UserId).FirstOrDefault());
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var userId = student.Human?.UserId ?? Guid.Empty;
            var listEventCode = request.TokenHistorys.Where(x => !string.IsNullOrEmpty(x.EventCode)).Select(x => x.EventCode!).ToList();
            if (listEventCode.Any())
            {
                var tokenHistoryStudents = await _tokenHistoryRepository.Queryable.Where(x => x.UserId == userId && x.Feature == EnumTokenFeature.FselEvent).ToListAsync(cancellationToken);
                var tokenHistoryEvent = tokenHistoryStudents.FirstOrDefault(x => !string.IsNullOrEmpty(x.ConfigData?.EventCode) && listEventCode.Contains(x.ConfigData.EventCode));
                if (tokenHistoryEvent != null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(tokenHistoryEvent.ConfigData.EventCode), tokenHistoryEvent.ConfigData?.EventCode);
                    return methodResult;
                }
            }

            var numberOfToken = student.NumberOfToken;

            var tokenHistorys = new List<TokenHistory>();
            var tokenConfigs = await _tokenConfigRepository.Queryable.Where(x => request.TokenHistorys.Select(x => x.Feature).Contains(x.Feature) && request.TokenHistorys.Select(x => x.Mission).Contains(x.Mission)).ToListAsync(cancellationToken);
            foreach (var item in request.TokenHistorys)
            {
                TokenHistory tokenHistory = _mapper.Map<TokenHistory>(item);
                tokenHistory.InitialToken = numberOfToken;
                if (!string.IsNullOrEmpty(item.EventCode))
                {
                    tokenHistory.ConfigData = new ConfigDataToken
                    {
                        EventCode = item.EventCode
                    };
                }
                if (item.Feature != EnumTokenFeature.MarketPlace)
                {
                    var tokenConfig = tokenConfigs.FirstOrDefault(x => x.Feature == item.Feature && x.Mission == item.Mission);
                    if (tokenConfig != null)
                    {
                        tokenHistory.TokenConfigId = tokenConfig.Id;
                        if (!tokenHistory.IsValid())
                        {
                            methodResult.AddErrorBadRequest(tokenHistory.ErrorMessages);
                            return methodResult;
                        }
                    }
                }
                tokenHistory.Translations = item.TokenHistoryTranslations?.Select(x => new TokenHistoryTranslation
                {
                    Language = x.Language,
                    Config = x.Config
                }).ToList() ?? new List<TokenHistoryTranslation>();

                numberOfToken = tokenHistory.RemainToken;
                tokenHistorys.Add(tokenHistory);
            }

            await _userService.UpdateStudentByTokenAsync(new UpdateStudentByTokenModel
            {
                NumberOfToken = (long)request.TokenHistorys.Select(x =>
                {
                    x.VolatileToken = x.Type == EnumTokenHistoryType.Recevived ? x.VolatileToken : -x.VolatileToken;
                    return x;
                }).Sum(x => x.VolatileToken),
                StudentId = student.Id
            }).ConfigureAwait(false);

            await _tokenHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                await _tokenHistoryRepository.AddList(tokenHistorys);
                await _tokenHistoryRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<IList<TokenHistoryModel>>(tokenHistorys);
                return methodResult;
            });
            return methodResult;
        }
    }
}
