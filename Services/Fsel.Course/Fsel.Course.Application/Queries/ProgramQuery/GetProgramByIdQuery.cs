// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ProgramQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetProgramByIdQuery : IRequest<MethodResult<ProgramModel>>
    {
        public Guid ProgramId { get; set; }
    }

    public class GetProgramByIdQueryHandler : IRequestHandler<GetProgramByIdQuery, MethodResult<ProgramModel>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private readonly ISkillLevelRepository _skillLevelRepository;

        public GetProgramByIdQueryHandler(ICategoryRepository categoryRepository,
                                          ILevelRepository levelRepository,
                                          ISkillLevelRepository skillLevelRepository)
        {
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
            _skillLevelRepository = skillLevelRepository;
        }

        public async Task<MethodResult<ProgramModel>> Handle(GetProgramByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ProgramModel> methodResult = new MethodResult<ProgramModel>();

            var program = await (from a in _categoryRepository.Queryable
                                 join b in _levelRepository.Queryable on a.Id equals b.ProgramId
                                 where a.Id == request.ProgramId
                                 group b by a into g
                                 select new ProgramModel
                                 {
                                     Id = g.Key.Id,
                                     Name = g.Key.Name,
                                     Code = g.Key.Code,
                                     Description = g.Key.Description,
                                     Status = g.Key.Status,
                                     Type = g.Key.Type,
                                     CreatedDate = g.Key.CreatedDate,
                                     CreatedFullName = g.Key.CreatedFullName,
                                     CreatedUserId = g.Key.CreatedUserId,
                                     UpdatedDate = g.Key.UpdatedDate,
                                     UpdatedFullName = g.Key.UpdatedFullName,
                                     UpdatedUserId = g.Key.UpdatedUserId,
                                     Levels = g.Select(b => new LevelModel
                                     {
                                         Id = b.Id,
                                         Name = b.Name,
                                         Code = b.Code,
                                         LevelOrder = b.LevelOrder,
                                         Description = b.Description,
                                         Skils = _skillLevelRepository.Queryable
                                                                      .Include(x => x.Skill)
                                                                      .Where(sl => sl.LevelId == b.Id)
                                                                      .Select(sl => new SkillViewModel
                                                                      {
                                                                          Id = sl.SkillId,
                                                                          Name = sl.Skill != null ? sl.Skill.Name : string.Empty
                                                                      }).ToList()
                                     }).ToList()
                                 }).FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = program;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
