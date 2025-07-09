// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ArchiveCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
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
                    await ArchiveCourses(request.Ids, cancellationToken);
                    break;

                case nameof(Domain.Entities.Unit):
                    await ArchiveUnits(request.Ids, cancellationToken);
                    break;

                case nameof(Lesson):
                    await ArchiveLessons(request.Ids, cancellationToken);
                    break;

                case nameof(HomeWork):
                    await ArchiveHomeWorks(request.Ids, cancellationToken);
                    break;

                case nameof(Video):
                    await ArchiveVideos(request.Ids, cancellationToken);
                    break;

                case nameof(ExtraPractice):
                    await ArchiveExtraPractices(request.Ids, cancellationToken);
                    break;

                case nameof(MockTest):
                    await ArchiveMockTests(request.Ids, cancellationToken);
                    break;

                case nameof(FinalTest):
                    await ArchiveFinalTests(request.Ids, cancellationToken);
                    break;

                case nameof(PlacementTest):
                    await ArchivePlacementTests(request.Ids, cancellationToken);
                    break;

                case nameof(Test):
                    await ArchiveTests(request.Ids, cancellationToken);
                    break;

                default:
                    break;
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<VoidMethodResult> ArchiveCourses(IList<Guid> ids, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var courses = await _courseRepository.Queryable.WhereBulkContains(ids, x => x.Id).ToListAsync(cancellationToken);
            if (courses == null || !courses.Any())
            {
                return methodResult;
            }
            courses.ForEach(p =>
            {
                if (!p.IsArchive && p.Status == EnumCourseStatus.Active)
                {
                    p.Status = EnumCourseStatus.InActive;
                }
                p.IsArchive = !p.IsArchive;
            });
            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                await _courseRepository.BulkUpdateList(courses);
                return methodResult;
            });
            return methodResult;
        }

        private async Task<VoidMethodResult> ArchiveUnits(IList<Guid> ids, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var units = await _unitRepository.Queryable.WhereBulkContains(ids, x => x.Id).ToListAsync(cancellationToken);
            if (units == null || !units.Any())
            {
                return methodResult;
            }
            units.ForEach(p => { p.IsArchive = !p.IsArchive; });
            await _unitRepository.ExecuteTransactionAsync(async () =>
            {
                await _unitRepository.BulkUpdateList(units);
                return methodResult;
            });
            return methodResult;
        }

        private async Task<VoidMethodResult> ArchiveLessons(IList<Guid> ids, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var lessons = await _lessonRepository.Queryable.WhereBulkContains(ids, x => x.Id).ToListAsync(cancellationToken);
            if (lessons == null || !lessons.Any())
            {
                return methodResult;
            }
            lessons.ForEach(p => { p.IsArchive = !p.IsArchive; });
            await _lessonRepository.ExecuteTransactionAsync(async () =>
            {
                await _lessonRepository.BulkUpdateList(lessons);
                return methodResult;
            });
            return methodResult;
        }

        private async Task<VoidMethodResult> ArchiveVideos(IList<Guid> ids, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var videos = await _videoRepository.Queryable.WhereBulkContains(ids, x => x.Id).ToListAsync(cancellationToken);
            if (videos == null || !videos.Any())
            {
                return methodResult;
            }
            videos.ForEach(p => { p.IsArchive = !p.IsArchive; });
            await _videoRepository.ExecuteTransactionAsync(async () =>
            {
                await _videoRepository.BulkUpdateList(videos);
                return methodResult;
            });
            return methodResult;
        }

        private async Task<VoidMethodResult> ArchiveHomeWorks(IList<Guid> ids, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var homeWorks = await _homeWorkRepository.Queryable.WhereBulkContains(ids, x => x.Id).ToListAsync(cancellationToken);
            if (homeWorks == null || !homeWorks.Any())
            {
                return methodResult;
            }
            homeWorks.ForEach(p => { p.IsArchive = !p.IsArchive; });
            await _homeWorkRepository.ExecuteTransactionAsync(async () =>
            {
                await _homeWorkRepository.BulkUpdateList(homeWorks);
                return methodResult;
            });
            return methodResult;
        }

        private async Task<VoidMethodResult> ArchiveExtraPractices(IList<Guid> ids, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var extraPractices = await _extraPracticeRepository.Queryable.WhereBulkContains(ids, x => x.Id).ToListAsync(cancellationToken);
            if (extraPractices == null || !extraPractices.Any())
            {
                return methodResult;
            }
            extraPractices.ForEach(p => { p.IsArchive = !p.IsArchive; });
            await _extraPracticeRepository.ExecuteTransactionAsync(async () =>
            {
                await _extraPracticeRepository.BulkUpdateList(extraPractices);
                return methodResult;
            });
            return methodResult;
        }

        private async Task<VoidMethodResult> ArchiveMockTests(IList<Guid> ids, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var mockTests = await _mockTestRepository.Queryable.WhereBulkContains(ids, x => x.Id).ToListAsync(cancellationToken);
            if (mockTests == null || !mockTests.Any())
            {
                return methodResult;
            }
            mockTests.ForEach(p => { p.IsArchive = !p.IsArchive; });
            await _mockTestRepository.ExecuteTransactionAsync(async () =>
            {
                await _mockTestRepository.BulkUpdateList(mockTests);
                return methodResult;
            });
            return methodResult;
        }

        private async Task<VoidMethodResult> ArchiveFinalTests(IList<Guid> ids, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var finalTests = await _finalTestRepository.Queryable.WhereBulkContains(ids, x => x.Id).ToListAsync(cancellationToken);
            if (finalTests == null || !finalTests.Any())
            {
                return methodResult;
            }
            finalTests.ForEach(p => { p.IsArchive = !p.IsArchive; });
            await _finalTestRepository.ExecuteTransactionAsync(async () =>
            {
                await _finalTestRepository.BulkUpdateList(finalTests);
                return methodResult;
            });
            return methodResult;
        }

        private async Task<VoidMethodResult> ArchivePlacementTests(IList<Guid> ids, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var placementTests = await _placementTestRepository.Queryable.WhereBulkContains(ids, x => x.Id).ToListAsync(cancellationToken);
            if (placementTests == null || !placementTests.Any())
            {
                return methodResult;
            }
            placementTests.ForEach(p => { p.IsArchive = !p.IsArchive; });
            await _placementTestRepository.ExecuteTransactionAsync(async () =>
            {
                await _placementTestRepository.BulkUpdateList(placementTests);
                return methodResult;
            });
            return methodResult;
        }

        private async Task<VoidMethodResult> ArchiveTests(IList<Guid> ids, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var tests = await _testRepository.Queryable.WhereBulkContains(ids, x => x.Id).ToListAsync(cancellationToken);
            if (tests == null || !tests.Any())
            {
                return methodResult;
            }
            tests.ForEach(p => { p.IsArchive = !p.IsArchive; });
            await _testRepository.ExecuteTransactionAsync(async () =>
            {
                await _testRepository.BulkUpdateList(tests);
                return methodResult;
            });
            return methodResult;
        }
    }
}
