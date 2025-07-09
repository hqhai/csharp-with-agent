// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Sections;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.MockTestAiSettingQuery
{
    public class GetMockTestAiSettingQuery : IRequest<MethodResult<IList<MockTestAISettingModel>>>
    {
        public Guid Id { get; set; }
    }

    public class GetMockTestAiSettingQueryHandler : IRequestHandler<GetMockTestAiSettingQuery, MethodResult<IList<MockTestAISettingModel>>>
    {
        private readonly IMockTestAISettingRepository _mockTestAISettingRepository;
        private readonly IMapper _mapper;

        public GetMockTestAiSettingQueryHandler(IMockTestAISettingRepository mockTestAISettingRepository, IMapper mapper)
        {
            _mockTestAISettingRepository = mockTestAISettingRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<MockTestAISettingModel>>> Handle(GetMockTestAiSettingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<MockTestAISettingModel>> methodResult = new MethodResult<IList<MockTestAISettingModel>>();

            var mockTestAiSettings = await _mockTestAISettingRepository.Queryable.Include(x => x.MockTestAICriteriaSettings).Where(x => x.SectionId == request.Id).ToListAsync(cancellationToken);

            if (mockTestAiSettings == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestAiSettings));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<MockTestAISettingModel>>(mockTestAiSettings);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
