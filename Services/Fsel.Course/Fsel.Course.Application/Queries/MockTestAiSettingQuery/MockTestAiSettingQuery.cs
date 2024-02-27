// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Sections;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.MockTestAiSettingQuery
{
    public class GetMockTestAiSettingQuery : IRequest<MethodResult<MockTestAISettingModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetMockTestAiSettingQueryHandler : IRequestHandler<GetMockTestAiSettingQuery, MethodResult<MockTestAISettingModel>>
    {
        private readonly IMockTestAISettingRepository _mockTestAISettingRepository;
        private readonly IMapper _mapper;

        public GetMockTestAiSettingQueryHandler(IMockTestAISettingRepository mockTestAISettingRepository, IMapper mapper)
        {
            _mockTestAISettingRepository = mockTestAISettingRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<MockTestAISettingModel>> Handle(GetMockTestAiSettingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestAISettingModel> methodResult = new MethodResult<MockTestAISettingModel>();

            var mockTestAiSetting = await _mockTestAISettingRepository.Queryable.FirstOrDefaultAsync(x => x.SectionId == request.Id, cancellationToken);

            if (mockTestAiSetting != null)
            {
                methodResult.Result = _mapper.Map<MockTestAISettingModel>(mockTestAiSetting);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
