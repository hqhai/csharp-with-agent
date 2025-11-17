// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CategoryQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Core.Base.Interfaces;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Domain.IRepositories;
    using Common.ActionResults;
    using Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services.ApplicationServices;

    public class GetProgramsMatchUserQuery : IRequest<MethodResult<List<ProgramModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetProgramsMatchUserQueryHandler : IRequestHandler<GetProgramsMatchUserQuery, MethodResult<List<ProgramModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly IRepository<TestGroupResult> _testGroupResult;
        private readonly ICategoryRepository _categoryRepository;


        public GetProgramsMatchUserQueryHandler(
            ICategoryService categoryService,
            ICategoryRepository categoryRepository,
            IRepository<TestGroupResult> testGroupResult,
            IMapper mapper)
        {
            _categoryService = categoryService;
            _categoryRepository = categoryRepository;
            _testGroupResult = testGroupResult;
            _mapper = mapper;
        }

        public async Task<MethodResult<List<ProgramModel>>> Handle(GetProgramsMatchUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var ptTestResult = await _testGroupResult.ReadQueryable.Where(x => x.StudentId == request.StudentId && x.TestType == EnumTestType.PlacementTest)
                .FirstOrDefaultAsync(cancellationToken);

            if (ptTestResult == null)
            {
                return new MethodResult<List<ProgramModel>>();
            }


            if (ptTestResult.Status == EnumResultStatus.ByPass)
            {
                var programs = await _categoryRepository.ReadQueryable
                    .Where(x => x.Id == ptTestResult.ProgramId)
                    .Include(x => x.Categorys)
                    .SelectMany(x => x.Categorys).ToListAsync(cancellationToken);

                var programsMatchLevel = programs.Where(x => x.Levels.Any(l => l.Id == ptTestResult.LevelId)).ToList();
                return new MethodResult<List<ProgramModel>> { Result = _mapper.Map<List<ProgramModel>>(programs), StatusCode = 200 };
            }

            if (ptTestResult.Status == EnumResultStatus.Done)
            {
                var programOfPtTest = await _categoryRepository.ReadQueryable
                    .Where(x => x.Id == ptTestResult.ProgramId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (programOfPtTest == null)
                {
                    return new MethodResult<List<ProgramModel>>();
                }

                var programs = await _categoryRepository.ReadQueryable
                    .Where(x => x.Id == programOfPtTest.ParentId)
                    .Include(x => x.Categorys)
                    .ThenInclude(x => x.Levels)
                    .SelectMany(x => x.Categorys).ToListAsync(cancellationToken);

                var programsMatchLevel = programs.Where(x => x.Levels.Any(l => l.Id == ptTestResult.CurrentLevelId)).ToList();
                return new MethodResult<List<ProgramModel>> { Result = _mapper.Map<List<ProgramModel>>(programsMatchLevel), StatusCode = 200 };
            }

            return new MethodResult<List<ProgramModel>>();
        }
    }
}
