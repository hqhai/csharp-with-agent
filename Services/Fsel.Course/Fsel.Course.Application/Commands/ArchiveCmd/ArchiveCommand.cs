// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ArchiveCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ArchiveCommand : IRequest<MethodResult<bool>>
    {
        public string? ObjectName { get; set; }
        public IList<Guid>? Ids { get; set; }
    }

    public class ArchiveCommandHandler : IRequestHandler<ArchiveCommand, MethodResult<bool>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly ITestRepository _testRepository;

        public ArchiveCommandHandler(ICourseRepository courseRepository,
            ILessonRepository lessonRepository,
            IUnitRepository unitRepository,
            IHomeWorkRepository homeWorkRepository,
            IExtraPracticeRepository extraPracticeRepository,
            IMockTestRepository mockTestRepository,
            IFinalTestRepository finalTestRepository,
            IPlacementTestRepository placementTestRepository,
            IVideoRepository videoRepository,
            ITestRepository testRepository)
        {
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _unitRepository = unitRepository;
            _homeWorkRepository = homeWorkRepository;
            _extraPracticeRepository = extraPracticeRepository;
            _mockTestRepository = mockTestRepository;
            _finalTestRepository = finalTestRepository;
            _placementTestRepository = placementTestRepository;
            _videoRepository = videoRepository;
            _testRepository = testRepository;
        }

        public async Task<MethodResult<bool>> Handle(ArchiveCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            if (string.IsNullOrEmpty(request.ObjectName) || (request.Ids == null || !request.Ids.Any()))
            {
                return methodResult;
            }
            switch (request.ObjectName)
            {
                case nameof(Course):
                    await ArchiveEntitiesAsync(
                        repository: _courseRepository,
                        ids: request.Ids,
                        toggleArchiveAction: p =>
                        {
                            if (!p.IsArchive && p.Status == EnumCourseStatus.Active)
                            {
                                p.Status = EnumCourseStatus.InActive;
                            }
                            p.IsArchive = !p.IsArchive;
                        },
                        cancellationToken: cancellationToken
                    );
                    break;

                case nameof(Domain.Entities.Unit):
                    await ArchiveEntitiesAsync(
                        repository: _unitRepository,
                        ids: request.Ids,
                        toggleArchiveAction: p => p.IsArchive = !p.IsArchive,
                        cancellationToken: cancellationToken
                    );
                    break;

                case nameof(Lesson):
                    await ArchiveEntitiesAsync(
                         repository: _lessonRepository,
                         ids: request.Ids,
                         toggleArchiveAction: p => p.IsArchive = !p.IsArchive,
                         cancellationToken: cancellationToken
                    );
                    break;

                case nameof(HomeWork):
                    await ArchiveEntitiesAsync(
                        repository: _homeWorkRepository,
                        ids: request.Ids,
                        toggleArchiveAction: p => p.IsArchive = !p.IsArchive,
                        cancellationToken: cancellationToken
                    );
                    break;

                case nameof(Video):
                    await ArchiveEntitiesAsync(
                        repository: _videoRepository,
                        ids: request.Ids,
                        toggleArchiveAction: p => p.IsArchive = !p.IsArchive,
                        cancellationToken: cancellationToken
                    );
                    break;

                case nameof(ExtraPractice):
                    await ArchiveEntitiesAsync(
                        repository: _extraPracticeRepository,
                        ids: request.Ids,
                        toggleArchiveAction: p => p.IsArchive = !p.IsArchive,
                        cancellationToken: cancellationToken
                    );
                    break;

                case nameof(MockTest):
                    await ArchiveEntitiesAsync(
                        repository: _mockTestRepository,
                        ids: request.Ids,
                        toggleArchiveAction: p => p.IsArchive = !p.IsArchive,
                        cancellationToken: cancellationToken
                    );
                    break;

                case nameof(FinalTest):
                    await ArchiveEntitiesAsync(
                        repository: _finalTestRepository,
                        ids: request.Ids,
                        toggleArchiveAction: p => p.IsArchive = !p.IsArchive,
                        cancellationToken: cancellationToken
                    );
                    break;

                case nameof(PlacementTest):
                    await ArchiveEntitiesAsync(
                        repository: _placementTestRepository,
                        ids: request.Ids,
                        toggleArchiveAction: p => p.IsArchive = !p.IsArchive,
                        cancellationToken: cancellationToken
                    );
                    break;

                case nameof(Test):
                    await ArchiveEntitiesAsync(
                        repository: _testRepository,
                        ids: request.Ids,
                        toggleArchiveAction: p => p.IsArchive = !p.IsArchive,
                        cancellationToken: cancellationToken
                    );
                    break;

                default:
                    break;
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static async Task<VoidMethodResult> ArchiveEntitiesAsync<T>(IRepository<T> repository,
                                                                    IList<Guid> ids,
                                                                    List<Func<T, bool>>? additionalConditions = null,
                                                                    Func<T, bool>? toggleArchiveCondition = null,
                                                                    Action<T>? toggleArchiveAction = null,
                                                                    CancellationToken cancellationToken = default
                                                                ) where T : Entity
        {
            var methodResult = new VoidMethodResult();
            var query = repository.Queryable.WhereBulkContains(ids, x => x.Id);
            // Áp dụng thêm điều kiện nếu có
            if (additionalConditions != null)
            {
                foreach (var condition in additionalConditions)
                {
                    query = query.Where(condition).AsQueryable();
                }
            }
            var entities = await query.ToListAsync(cancellationToken);
            if (!entities.Any())
            {
                return methodResult;
            }

            // Thực hiện toggle archive
            foreach (var entity in entities)
            {
                if (toggleArchiveCondition == null || toggleArchiveCondition(entity))
                {
                    toggleArchiveAction?.Invoke(entity);
                }
            }
            await repository.ExecuteTransactionAsync(async () =>
            {
                await repository.BulkUpdateList(entities);
                return methodResult;
            });

            return methodResult;
        }
    }
}
