// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.SkillQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSkillQuery : IRequest<MethodResult<SkillModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetSkillQueryHandler : IRequestHandler<GetSkillQuery, MethodResult<SkillModel>>
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IMapper _mapper;

        public GetSkillQueryHandler(ISkillRepository skillRepository,
            IMapper mapper)
        {
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SkillModel>> Handle(GetSkillQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SkillModel> methodResult = new MethodResult<SkillModel>();
            var skill = await _skillRepository.Queryable.Where(x => x.Id == request.Id).Select(x => new
            {
                Skill = _mapper.Map<SkillModel>(x),
                IsActive = x.SkillLevels.Any(),
            }).FirstOrDefaultAsync(cancellationToken);

            if (skill == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            var skillModel = skill.Skill;
            skillModel.IsActive = skill.IsActive;
            methodResult.Result = skillModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
