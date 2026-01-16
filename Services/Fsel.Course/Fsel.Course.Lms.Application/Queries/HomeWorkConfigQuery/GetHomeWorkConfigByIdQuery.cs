// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkConfigQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetHomeWorkConfigByIdQuery : IRequest<MethodResult<HomeWorkConfigModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetHomeWorkConfigByIdQueryHandler : IRequestHandler<GetHomeWorkConfigByIdQuery, MethodResult<HomeWorkConfigModel>>
    {
        private readonly IHomeWorkConfigRepository _homeWorkConfigRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly ISkillRepository _skillRepository;

        public GetHomeWorkConfigByIdQueryHandler(IHomeWorkConfigRepository homeWorkConfigRepository, IHomeWorkRepository homeWorkRepository, ICategoryRepository categoryRepository, ILevelRepository levelRepository, ISkillRepository skillRepository)
        {
            _homeWorkConfigRepository = homeWorkConfigRepository;
            _homeWorkRepository = homeWorkRepository;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<HomeWorkConfigModel>> Handle(GetHomeWorkConfigByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<HomeWorkConfigModel>();

            var query = await (from hc in _homeWorkConfigRepository.Queryable
                               join h in _homeWorkRepository.Queryable on hc.HomeWorkId equals h.Id
                               join p in _categoryRepository.Queryable.Include(n => n.CategoryParent) on h.ProgramId equals p.Id
                               join s in _skillRepository.Queryable on h.SkillId equals s.Id
                               join l in _levelRepository.Queryable on h.LevelId equals l.Id
                               where hc.Id == request.Id
                               select new HomeWorkConfigModel()
                               {
                                   Id = hc.Id,
                                   CreatedUserId = hc.CreatedUserId,
                                   CreatedFullName = hc.CreatedFullName,
                                   HomeWorkName = h.Name,
                                   CreatedDate = hc.CreatedDate,
                                   CurriculumId = hc.CurriculumId,
                                   EndDate = hc.EndDate,
                                   NumberRetry = hc.NumberRetry,
                                   StartDate = hc.StartDate,
                                   HomeWorkId = hc.HomeWorkId,
                                   CourseLevel = h.CourseLevel,
                                   CourseSkill = h.CourseSkill,
                                   Program = p.Name,
                                   ProgramId = p.Id,
                                   Level = l.Name,
                                   LevelId = l.Id,
                                   Skill = s.Name,
                                   SkillId = s.Id,
                                   Subject = p.CategoryParent != null ? p.CategoryParent.Name : null,
                               }).FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = query;
            return methodResult;
        }
    }
}
