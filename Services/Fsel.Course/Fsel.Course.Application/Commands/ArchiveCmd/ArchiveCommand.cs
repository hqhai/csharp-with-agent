// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.ArchiveCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Course.Domain.Entities;
    using Microsoft.AspNetCore.Http;
    using Fsel.Shared.Enums;

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

        public ArchiveCommandHandler(ICourseRepository courseRepository, ILessonRepository lessonRepository, IUnitRepository unitRepository, IHomeWorkRepository homeWorkRepository, IExtraPracticeRepository extraPracticeRepository, IMockTestRepository mockTestRepository, IFinalTestRepository finalTestRepository, IPlacementTestRepository placementTestRepository, IVideoRepository videoRepository)
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
        }

        public async Task<MethodResult<bool>> Handle(ArchiveCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.ObjectName) || request.Ids == null)
            {
                return methodResult;
            }
            else if (request.ObjectName == nameof(Course))
            {
                await ArchiveCourses(request.Ids, cancellationToken);
            }
            else if (request.ObjectName == nameof(Domain.Entities.Unit))
            {
                await ArchiveUnits(request.Ids, cancellationToken);
            }
            else if (request.ObjectName == nameof(Lesson))
            {
                await ArchiveLessons(request.Ids, cancellationToken);
            }
            else if (request.ObjectName == nameof(HomeWork))
            {
                await ArchiveHomeWorks(request.Ids, cancellationToken);
            }
            else if (request.ObjectName == nameof(Video))
            {
                await ArchiveVideos(request.Ids, cancellationToken);
            }
            else if (request.ObjectName == nameof(ExtraPractice))
            {
                await ArchiveExtraPractices(request.Ids, cancellationToken);
            }
            else if (request.ObjectName == nameof(MockTest))
            {
                await ArchiveMockTests(request.Ids, cancellationToken);
            }
            else if (request.ObjectName == nameof(FinalTest))
            {
                await ArchiveFinalTests(request.Ids, cancellationToken);
            }
            else if (request.ObjectName == nameof(PlacementTest))
            {
                await ArchivePlacementTests(request.Ids, cancellationToken);
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task ArchiveCourses(IList<Guid> ids, CancellationToken cancellationToken)
        {
            var courses = await _courseRepository.Queryable.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
            if (courses != null)
            {
                courses.ForEach(p =>
                {
                    if (!p.IsArchive && p.Status == EnumCourseStatus.Active)
                    {
                        p.Status = EnumCourseStatus.InActive;
                    }
                    p.IsArchive = !p.IsArchive;
                });
                _courseRepository.UpdateList(courses);
                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ArchiveUnits(IList<Guid> ids, CancellationToken cancellationToken)
        {
            var units = await _unitRepository.Queryable.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
            if (units != null)
            {
                units.ForEach(p => { p.IsArchive = !p.IsArchive; });
                _unitRepository.UpdateList(units);
                await _unitRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ArchiveLessons(IList<Guid> ids, CancellationToken cancellationToken)
        {
            var lessons = await _lessonRepository.Queryable.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
            if (lessons != null)
            {
                lessons.ForEach(p => { p.IsArchive = !p.IsArchive; });
                _lessonRepository.UpdateList(lessons);
                await _lessonRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ArchiveVideos(IList<Guid> ids, CancellationToken cancellationToken)
        {
            var videos = await _videoRepository.Queryable.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
            if (videos != null)
            {
                videos.ForEach(p => { p.IsArchive = !p.IsArchive; });
                _videoRepository.UpdateList(videos);
                await _videoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ArchiveHomeWorks(IList<Guid> ids, CancellationToken cancellationToken)
        {
            var homeWorks = await _homeWorkRepository.Queryable.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
            if (homeWorks != null)
            {
                homeWorks.ForEach(p => { p.IsArchive = !p.IsArchive; });
                _homeWorkRepository.UpdateList(homeWorks);
                await _homeWorkRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ArchiveExtraPractices(IList<Guid> ids, CancellationToken cancellationToken)
        {
            var extraPractices = await _extraPracticeRepository.Queryable.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
            if (extraPractices != null)
            {
                extraPractices.ForEach(p => { p.IsArchive = !p.IsArchive; });
                _extraPracticeRepository.UpdateList(extraPractices);
                await _extraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ArchiveMockTests(IList<Guid> ids, CancellationToken cancellationToken)
        {
            var mockTests = await _mockTestRepository.Queryable.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
            if (mockTests != null)
            {
                mockTests.ForEach(p => { p.IsArchive = !p.IsArchive; });
                _mockTestRepository.UpdateList(mockTests);
                await _mockTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ArchiveFinalTests(IList<Guid> ids, CancellationToken cancellationToken)
        {
            var finalTests = await _finalTestRepository.Queryable.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
            if (finalTests != null)
            {
                finalTests.ForEach(p => { p.IsArchive = !p.IsArchive; });
                _finalTestRepository.UpdateList(finalTests);
                await _finalTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task ArchivePlacementTests(IList<Guid> ids, CancellationToken cancellationToken)
        {
            var placementTests = await _placementTestRepository.Queryable.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
            if (placementTests != null)
            {
                placementTests.ForEach(p => { p.IsArchive = !p.IsArchive; });
                _placementTestRepository.UpdateList(placementTests);
                await _placementTestRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
