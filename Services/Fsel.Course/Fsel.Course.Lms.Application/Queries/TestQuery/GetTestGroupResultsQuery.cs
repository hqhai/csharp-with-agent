// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.TestQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetTestGroupResultsQuery : IRequest<MethodResult<IList<TestGroupResultModels>>>
    {
        public Guid? UserId { get; set; }
    }

    public class GetTestGroupResultsQueryHandler : IRequestHandler<GetTestGroupResultsQuery, MethodResult<IList<TestGroupResultModels>>>
    {
        private readonly ITestRepository _testRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly ITestResultRepository _testResultRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly ISkillRepository _skillRepository;
        private readonly ILevelRepository _levelRepository;

        public GetTestGroupResultsQueryHandler(ITestRepository testRepository, ITestGroupResultRepository testGroupResultRepository, ITestResultRepository testResultRepository, ICategoryRepository categoryRepository, IUserService userService, AuthContext authContext, ISkillRepository skillRepository, ILevelRepository levelRepository)
        {
            _testRepository = testRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _testResultRepository = testResultRepository;
            _categoryRepository = categoryRepository;
            _userService = userService;
            _authContext = authContext;
            _skillRepository = skillRepository;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<IList<TestGroupResultModels>>> Handle(GetTestGroupResultsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<TestGroupResultModels>>();

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(request.UserId ?? _authContext.CurrentUserId);
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.UserId), request.UserId ?? _authContext.CurrentUserId);
                return methodResult;
            }

            var query = await (from tg in _testGroupResultRepository.ReadQueryable
                               join tr in _testResultRepository.ReadQueryable on tg.Id equals tr.TestGroupResultId
                               join p in _categoryRepository.ReadQueryable on tg.ProgramId equals p.Id
                               where tg.StudentId == student.Id && (tg.Status == EnumResultStatus.Done || tg.Status == EnumResultStatus.Process) && tg.TestType == EnumTestType.PlacementTest
                               select new
                               {
                                   TestGroupResult = tg,
                                   TestResult = tr,
                                   Program = p,
                                   Subject = p.CategoryParent
                               }).ToListAsync(cancellationToken);

            if (query == null)
            {
                return methodResult;
            }

            var skills = await _skillRepository.ReadQueryable.ToListAsync(cancellationToken);
            var levels = await _levelRepository.ReadQueryable.ToListAsync(cancellationToken);

            var queryGroups = query.GroupBy(p => p.TestGroupResult).ToList();

            var models = new List<TestGroupResultModels>();

            foreach (var group in queryGroups)
            {
                var data = group.FirstOrDefault();
                var currentLevel = levels.FirstOrDefault(p => p.Id == group.Key.CurrentLevelId);
                var suggestLevel = levels.FirstOrDefault(p => p.Id == group.Key.EmailLevelId);
                var test = new TestGroupResultModels()
                {
                    TestGroupResultId = group.Key.Id,
                    CreatedDate = group.Key.CreatedDate,
                    UpdatedDate = group.Key.UpdatedDate,
                    SubjectId = data?.Subject.Id,
                    SubjectName = data?.Subject.Name,
                    CurrentLevelId = currentLevel?.Id,
                    CurrentLevelName = currentLevel?.Name,
                    SuggestLevelId = suggestLevel?.Id,
                    SuggestLevelName = suggestLevel?.Name
                };

                var moduleModels = new List<TestGroupResultModel>();

                foreach (var module in group)
                {
                    if (module.TestResult.SkillScores != null)
                    {
                        var skillModels = new List<TestGroupResultDetailModel>();

                        foreach (var item in module.TestResult.SkillScores)
                        {
                            var skill = skills.FirstOrDefault(p => p.Id == item.SkillId);
                            skillModels.Add(new TestGroupResultDetailModel()
                            {
                                SkillId = skill?.Id,
                                SkillName = skill?.Name,
                                Percent = item.Percent
                            });
                        }

                        var moduleModel = new TestGroupResultModel()
                        {
                            Modules = skillModels,
                            Overall = NumberHelper.CalculateAverage(skillModels.Select(p => (long)(p.Percent ?? 0)).ToList())
                        };

                        moduleModels.Add(moduleModel);
                    }
                }

                test.Modules = moduleModels;

                models.Add(test);
            }

            methodResult.Result = models.OrderByDescending(p => p.UpdatedDate ?? p.CreatedDate).ToList();
            return methodResult;
        }
    }
}
