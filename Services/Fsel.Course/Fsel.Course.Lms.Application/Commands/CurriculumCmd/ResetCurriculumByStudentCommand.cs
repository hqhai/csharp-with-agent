// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CurriculumCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ResetCurriculumByStudentCommand : IRequest<MethodResult<bool>>
    {
        public Guid? UserId { get; set; }
        public Guid? CourseId { get; set; }
    }

    public class ResetCurriculumByStudentCommandHandler : IRequestHandler<ResetCurriculumByStudentCommand, MethodResult<bool>>
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
        private readonly AuthContext _authContext;

        public ResetCurriculumByStudentCommandHandler(IUserService userService, ICourseResultRepository courseResultRepository, IPlacementTestResultRepository placementTestResultRepository, IFinalTestResultRepository finalTestResultRepository, IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository, IMockTestResultRepository mockTestResultRepository, IExtraPracticeResultRepository extraPracticeResultRepository, IClassForumResultRepository classForumResultRepository, IHomeWorkResultRepository homeWorkResultRepository, IVideoResultRepository videoResultRepository, ISectionGroupResultRepository sectionGroupResultRepository, IHomeWorkAnswerRepository homeWorkAnswerRepository, IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository, AuthContext authContext)
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
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(ResetCurriculumByStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var userId = request.UserId ?? _authContext.CurrentUserId;

            var studentResult = await _userService.GetStudentByUserIdAsync(userId);

            if (studentResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            student.CourseId = request.CourseId ?? student.CourseId;

            // delete final test answers
            var finalTestResults = await _finalTestResultRepository.Queryable.Include(x => x.SectionGroupResults)
                                                                  .Include(x => x.FinalTestAnswers)
                                                                  .Where(x => x.StudentId == student.Id && x.CourseId == student.CourseId)
                                                                  .ToListAsync(cancellationToken);
            if (finalTestResults.Count != 0)
            {
                var finalTestResultIds = finalTestResults.Select(x => x.Id).ToList();

                //delete SectionGroupResult
                var sectionGroupResult = await _sectionGroupResultRepository.Queryable.WhereBulkContains(finalTestResultIds, p => p.FinalTestResultId)
                                                                            .Where(x => x.StudentId == student.Id)
                                                                            .ToListAsync(cancellationToken);
                if (sectionGroupResult.Count != 0)
                {
                    await _sectionGroupResultRepository.DeleteListAsync(sectionGroupResult);
                    await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                }

                await _finalTestResultRepository.DeleteListAsync(finalTestResults);
                await _finalTestResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            // delete unit result
            var unitResult = await _unitResultRepository.Queryable
                                                        .Where(x => x.StudentId == student.Id && x.CourseId == student.CourseId)
                                                        .ToListAsync(cancellationToken);
            if (unitResult.Count != 0)
            {
                await _unitResultRepository.DeleteListAsync(unitResult);
                await _unitResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            // delete lesson result va lesson note
            var lessonResults = await _lessonResultRepository.Queryable
                                                            .Include(x => x.LessonNotes)
                                                            .Where(x => x.StudentId == student.Id && x.CourseId == student.CourseId)
                                                            .ToListAsync(cancellationToken);
            if (lessonResults.Count != 0)
            {
                // delete Video Result

                var lessonResultIds = lessonResults.Select(x => x.Id).ToList();

                var videoResults = await _videoResultRepository.Queryable.WhereBulkContains(lessonResultIds, p => p.LessonResultId)
                                                              .Where(x => x.StudentId == student.Id)
                                                              .ToListAsync(cancellationToken);
                if (videoResults.Count != 0)
                {
                    var videoTimeCodeAnswers = await _videoTimeCodeAnswerRepository.Queryable.WhereBulkContains(videoResults.Select(x => x.Id), x => x.VideoResultId).ToListAsync(cancellationToken);
                    if (videoTimeCodeAnswers.Any())
                    {
                        await _videoTimeCodeAnswerRepository.DeleteListAsync(videoTimeCodeAnswers);
                        await _videoTimeCodeAnswerRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                    }

                    var videoTimeCodeResults = await _videoTimeCodeResultRepository.Queryable.WhereBulkContains(videoResults.Select(x => x.Id), x => x.VideoResultId).ToListAsync(cancellationToken);
                    if (videoTimeCodeResults.Any())
                    {
                        await _videoTimeCodeResultRepository.DeleteListAsync(videoTimeCodeResults);
                        await _videoTimeCodeResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                    }

                    await _videoResultRepository.DeleteListAsync(videoResults);
                    await _videoResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                }

                // delete ClassForumResult
                var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumDetailResults).ThenInclude(x => x.ClassForumResultFiles)
                                                                        .Include(x => x.ClassForumResultFiles)
                                                                        .Include(x => x.ClassForumScores)
                                                                        .WhereBulkContains(lessonResultIds, p => p.LessonResultId)
                                                                        .Where(x => x.StudentId == student.Id)
                                                                        .ToListAsync(cancellationToken);
                if (classForumResult.Count != 0)
                {
                    await _classForumResultRepository.DeleteListAsync(classForumResult);
                    await _classForumResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                }

                // delete HomeworkResult
                var homeWorkResults = await _homeWorkResultRepository.Queryable.WhereBulkContains(lessonResultIds, p => p.LessonResultId).Where(x => x.StudentId == student.Id)
                                                                    .ToListAsync(cancellationToken);

                if (homeWorkResults.Count != 0)
                {
                    var homeWorkAnswers = await _homeWorkAnswerRepository.Queryable.WhereBulkContains(homeWorkResults.Select(x => x.Id), x => x.HomeWorkResultId).ToListAsync(cancellationToken);
                    if (homeWorkAnswers.Any())
                    {
                        await _homeWorkAnswerRepository.DeleteListAsync(homeWorkAnswers);
                        await _homeWorkAnswerRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                    }
                    await _homeWorkResultRepository.DeleteListAsync(homeWorkResults);
                    await _homeWorkResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                }

                await _lessonResultRepository.DeleteListAsync(lessonResults);
                await _lessonResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            // delete mock test result
            var mockTestResults = await _mockTestResultRepository.Queryable.Include(x => x.SectionGroupResults)
                                                          .Include(x => x.MockTestAnswers)
                                                          .Include(x => x.MockTestScores)
                                                          .Where(x => x.StudentId == student.Id && x.CourseId == student.CourseId)
                                                          .ToListAsync(cancellationToken);
            if (mockTestResults.Count != 0)
            {
                var mockTestResultIds = mockTestResults.Select(x => x.Id).ToList();

                //delete SectionGroupResult
                var sectionGroupResult = await _sectionGroupResultRepository.Queryable.WhereBulkContains(mockTestResultIds, p => p.MockTestResultId)
                                                                            .Where(x => x.StudentId == student.Id)
                                                                            .ToListAsync(cancellationToken);
                if (sectionGroupResult.Count != 0)
                {
                    await _sectionGroupResultRepository.DeleteListAsync(sectionGroupResult);
                    await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
                }

                await _mockTestResultRepository.DeleteListAsync(mockTestResults);
                await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            //delete course result
            var courseResult = await _courseResultRepository.Queryable
                                                            .Where(x => x.StudentId == student.Id && x.CourseId == student.CourseId)
                                                            .ToListAsync(cancellationToken);
            if (courseResult.Count != 0)
            {
                await _courseResultRepository.DeleteListAsync(courseResult);
                await _courseResultRepository.UnitOfWork.SaveChangesAsync(true, false, cancellationToken).ConfigureAwait(false);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
