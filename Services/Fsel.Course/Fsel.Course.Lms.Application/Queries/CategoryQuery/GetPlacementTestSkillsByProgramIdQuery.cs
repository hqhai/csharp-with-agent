// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CategoryQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using MediatR;

    public class GetPlacementTestSkillsByProgramIdQuery : IRequest<MethodResult<IList<SkillModel>>>
    {
        public Guid ProgramId { get; set; }
    }

    public class GetPlacementTestSkillsByProgramIdQueryHandler : IRequestHandler<GetPlacementTestSkillsByProgramIdQuery, MethodResult<IList<SkillModel>>>
    {
        private readonly ISkillCachingService _skillCachingService;

        public GetPlacementTestSkillsByProgramIdQueryHandler(ISkillCachingService skillCachingService)
        {
            _skillCachingService = skillCachingService;
        }

        public async Task<MethodResult<IList<SkillModel>>> Handle(GetPlacementTestSkillsByProgramIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SkillModel>>();

            methodResult.Result = await _skillCachingService.GetSkillsPtByProgramIdAsync(request.ProgramId, cancellationToken);
            return methodResult;
        }
    }
}
