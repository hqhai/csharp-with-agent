// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.SkillQuery
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSkillByLevelQuery : IRequest<MethodResult<IList<SkillModel>>>
    {
        public Guid LevelId { get; set; }
    }

    public class GetSkillByLevelQueryHandler : IRequestHandler<GetSkillByLevelQuery, MethodResult<IList<SkillModel>>>
    {
        private readonly ISkillRepository _skillRepository;
        private readonly IMapper _mapper;

        public GetSkillByLevelQueryHandler(ISkillRepository skillRepository,
                                           IMapper mapper)
        {
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<SkillModel>>> Handle(GetSkillByLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SkillModel>> methodResult = new MethodResult<IList<SkillModel>>();

            var skills = await _skillRepository.Queryable
                                               .Where(x => x.SkillLevels.Any(c => c.LevelId == request.LevelId))
                                               .AsNoTracking()
                                               .ToListAsync(cancellationToken);
            if (skills == null || !skills.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(skills));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<SkillModel>>(skills);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
