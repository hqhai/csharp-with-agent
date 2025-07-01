// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TestConfigSectionCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities.TestConfig;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteTestConfigSectionCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteTestConfigSectionCommandHandler : IRequestHandler<DeleteTestConfigSectionCommand, MethodResult<bool>>
    {
        private readonly ITestConfigSectionRepository _testConfigSectionRepository;
        private readonly ITestConfigSectionQuestionRepository _testConfigSectionQuestionRepository;
        private readonly IQuestionRepository _questionRepository;

        public DeleteTestConfigSectionCommandHandler(ITestConfigSectionRepository testConfigSectionRepository
            , ITestConfigSectionQuestionRepository testConfigSectionQuestionRepository
            , IQuestionRepository questionRepository)
        {
            _testConfigSectionRepository = testConfigSectionRepository;
            _testConfigSectionQuestionRepository = testConfigSectionQuestionRepository;
            _questionRepository = questionRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteTestConfigSectionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(request);
                MethodResult<bool> methodResult = new MethodResult<bool>();

                var rootSection = await _testConfigSectionRepository.GetIncludeByIdAsync(request.Id);
                if (rootSection == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                    return methodResult;
                }

                await _testConfigSectionRepository.ExecuteTransactionAsync(async () =>
                {
                    // 1. Lấy toàn bộ section con theo cây
                    var allSections = await _testConfigSectionRepository
                        .Queryable
                        .Where(x => x.TestConfigId == rootSection.TestConfigId)
                        .ToListAsync(cancellationToken);

                    var sectionIdsToDelete = GetDescendantSectionIds(rootSection.Id, allSections);
                    sectionIdsToDelete.Add(rootSection.Id);

                    // 2. Lấy và xóa tất cả Question thuộc các Section này
                    var sectionQuestionsToDelete = await _testConfigSectionQuestionRepository
                        .Queryable
                        .Where(q => sectionIdsToDelete.Contains(q.TestConfigSectionId))
                        .ToListAsync(cancellationToken);
                    if (sectionQuestionsToDelete.Any())
                    {
                        await _testConfigSectionQuestionRepository.DeleteListAsync(sectionQuestionsToDelete);

                        //3.Lấy và xóa tất cả Question thuộc các Section này
                        var questionsToDelete = await _questionRepository
                            .Queryable
                            .Where(q => sectionQuestionsToDelete.Select(z => z.QuestionId).Contains(q.Id))
                            .ToListAsync(cancellationToken);

                        if (questionsToDelete.Any())
                        {
                            await _questionRepository.DeleteListAsync(questionsToDelete);
                        }
                    }
                    // 3. Xóa các TestConfigSection
                    var sectionsToDelete = allSections.Where(s => sectionIdsToDelete.Contains(s.Id)).ToList();
                    await _testConfigSectionRepository.DeleteListAsync(sectionsToDelete);

                    // 4. Lưu thay đổi
                    await _testConfigSectionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = true;
                    return methodResult;
                });
                return methodResult;
            }
            catch (Exception e)
            {
                throw;
            }

        }

        /// <summary>
        /// Đệ quy tìm tất cả section con từ 1 node gốc
        /// </summary>
        private List<Guid> GetDescendantSectionIds(Guid parentId, List<TestConfigSection> allSections)
        {
            var result = new List<Guid>();
            var children = allSections.Where(s => s.ParentId == parentId).ToList();
            foreach (var child in children)
            {
                result.Add(child.Id);
                result.AddRange(GetDescendantSectionIds(child.Id, allSections)); // recursive
            }
            return result;
        }
    }
}
