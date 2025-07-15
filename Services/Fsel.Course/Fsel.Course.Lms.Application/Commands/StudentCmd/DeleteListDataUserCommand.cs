// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteListDataUserCommand : IRequest<MethodResult<bool>>
    {
        public Guid UserId { get; set; }
    }

    public class DeleteListDataUserCommandHandler : IRequestHandler<DeleteListDataUserCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;

        public DeleteListDataUserCommandHandler(IUserService userService
                                              , ICourseResultRepository courseResultRepository
                                              , IPlacementTestResultRepository placementTestResultRepository
                                              , IFinalTestResultRepository finalTestResultRepository
                                              , IUnitResultRepository unitResultRepository
                                              , ILessonResultRepository lessonResultRepository
                                              , IMockTestResultRepository mockTestResultRepository
                                              , IExtraPracticeResultRepository extraPracticeResultRepository
                                              , IClassForumResultRepository classForumResultRepository
                                              , IHomeWorkResultRepository homeWorkResultRepository
                                              , IVideoResultRepository videoResultRepository
                                              , ISectionGroupResultRepository sectionGroupResultRepository
                                              , IHomeWorkAnswerRepository homeWorkAnswerRepository
                                              , IVideoTimeCodeResultRepository videoTimeCodeResultRepository
                                              , IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _videoResultRepository = videoResultRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteListDataUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(request.UserId);

            if (studentResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var studentId = studentResult.Content?.Result?.Id;

            //delete course result
            var courseResult = await _courseResultRepository.Queryable
                                                            .Where(x => x.StudentId == studentId && x.IsDeleted != true)
                                                            .ToListAsync(cancellationToken);
            if (courseResult.Count != 0)
            {
                await _courseResultRepository.DeleteListAsync(courseResult);
                await _courseResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            // delete placement test result va placement test answers
            var placementTestResult = await _placementTestResultRepository.Queryable.Include(x => x.SectionGroupResults)
                                                                          .Include(x => x.PlacementTestAnswers)
                                                                          .Where(x => x.StudentId == studentId && x.IsDeleted != true)
                                                                          .ToListAsync(cancellationToken);
            if (placementTestResult.Count != 0)
            {
                await _placementTestResultRepository.DeleteListAsync(placementTestResult);
                await _placementTestResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            // delete final test answers
            var finalTestResult = await _finalTestResultRepository.Queryable.Include(x => x.SectionGroupResults)
                                                                  .Include(x => x.FinalTestAnswers)
                                                                  .Where(x => x.StudentId == studentId && x.IsDeleted != true)
                                                                  .ToListAsync(cancellationToken);
            if (finalTestResult.Count != 0)
            {
                await _finalTestResultRepository.DeleteListAsync(finalTestResult);
                await _finalTestResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            // delete unit result
            var unitResult = await _unitResultRepository.Queryable
                                                        .Where(x => x.StudentId == studentId && x.IsDeleted != true)
                                                        .ToListAsync(cancellationToken);
            if (unitResult.Count != 0)
            {
                await _unitResultRepository.DeleteListAsync(unitResult);
                await _unitResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            // delete ClassForumResult
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumDetailResults).ThenInclude(x => x.ClassForumResultFiles)
                                                                    .Include(x => x.ClassForumResultFiles)
                                                                    .Include(x => x.ClassForumScores)
                                                                    .Include(x => x.ClassForumResultRandoms)
                                                                    .Where(x => x.StudentId == studentId && x.IsDeleted != true)
                                                                    .ToListAsync(cancellationToken);
            if (classForumResult.Count != 0)
            {
                await _classForumResultRepository.DeleteListAsync(classForumResult);
                await _classForumResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            // delete HomeworkResult
            var homeWorkResult = await _homeWorkResultRepository.Queryable.Where(x => x.StudentId == studentId && x.IsDeleted != true)
                                                                .ToListAsync(cancellationToken);

            if (homeWorkResult.Count != 0)
            {
                var homeWorkAnswers = await _homeWorkAnswerRepository.Queryable.Where(x => x.IsDeleted != true).WhereBulkContains(homeWorkResult.Select(x => x.Id), x => x.HomeWorkResultId).ToListAsync(cancellationToken);
                if (homeWorkAnswers.Any())
                {
                    await _homeWorkAnswerRepository.DeleteListAsync(homeWorkAnswers);
                    await _homeWorkAnswerRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                }
                await _homeWorkResultRepository.DeleteListAsync(homeWorkResult);
                await _homeWorkResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            // delete Video Result

            var videoResult = await _videoResultRepository.Queryable
                                                          .Where(x => x.StudentId == studentId && x.IsDeleted != true)
                                                          .ToListAsync(cancellationToken);
            if (videoResult.Count != 0)
            {
                var videoTimeCodeAnswers = await _videoTimeCodeAnswerRepository.Queryable.Where(x => !x.IsDeleted).WhereBulkContains(videoResult.Select(x => x.Id), x => x.VideoResultId).ToListAsync(cancellationToken);
                if (videoTimeCodeAnswers.Any())
                {
                    await _videoTimeCodeAnswerRepository.DeleteListAsync(videoTimeCodeAnswers);
                    await _videoTimeCodeAnswerRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                }

                var videoTimeCodeResults = await _videoTimeCodeResultRepository.Queryable.Where(x => !x.IsDeleted).WhereBulkContains(videoResult.Select(x => x.Id), x => x.VideoResultId).ToListAsync(cancellationToken);
                if (videoTimeCodeResults.Any())
                {
                    await _videoTimeCodeResultRepository.DeleteListAsync(videoTimeCodeResults);
                    await _videoTimeCodeResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                }

                await _videoResultRepository.DeleteListAsync(videoResult);
                await _videoResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            // delete lesson result va lesson note
            var lessonResult = await _lessonResultRepository.Queryable
                                                            .Include(x => x.LessonNotes)
                                                            .Where(x => x.StudentId == studentId && x.IsDeleted != true)
                                                            .ToListAsync(cancellationToken);
            if (lessonResult.Count != 0)
            {
                await _lessonResultRepository.DeleteListAsync(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            // delete mock test result
            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.SectionGroupResults)
                                                          .Include(x => x.MockTestAnswers)
                                                          .Include(x => x.MockTestScores)
                                                          .Where(x => x.StudentId == studentId && x.IsDeleted != true)
                                                          .ToListAsync(cancellationToken);
            if (mockTestResult.Count != 0)
            {
                await _mockTestResultRepository.DeleteListAsync(mockTestResult);
                await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            // delete ExtraPracticeResult
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable
                                                                          .Include(x => x.ExtraPracticeAnswers)
                                                                          .Include(x => x.ExtraPracticeExerciseResults)
                                                                          .Where(x => x.StudentId == studentId && x.IsDeleted != true)
                                                                          .ToListAsync(cancellationToken);
            if (extraPracticeResult.Count != 0)
            {
                await _extraPracticeResultRepository.DeleteListAsync(extraPracticeResult);
                await _extraPracticeResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            //delete SectionGroupResult
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable
                                                                        .Where(x => x.StudentId == studentId && x.IsDeleted != true)
                                                                        .ToListAsync(cancellationToken);
            if (sectionGroupResult.Count != 0)
            {
                await _sectionGroupResultRepository.DeleteListAsync(sectionGroupResult);
                await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
