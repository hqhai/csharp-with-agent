// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetExtraPracticeDetailQuery : IRequest<MethodResult<ExtraPracticeModel>>
    {
        public Guid ExtraPracticeId { get; set; }
    }

    public class GetExtraPracticeDetailQueryHandler : IRequestHandler<GetExtraPracticeDetailQuery, MethodResult<ExtraPracticeModel>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetExtraPracticeDetailQueryHandler(IExtraPracticeRepository extraPracticeRepository,
            IExtraPracticeResultRepository extraPracticeResultRepository,
            IMockTestRepository mockTestRepository,
            IVideoRepository videoRepository,
            AuthContext authContext,
            IUserService userService)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _mockTestRepository = mockTestRepository;
            _videoRepository = videoRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<ExtraPracticeModel>> Handle(GetExtraPracticeDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeModel> methodResult = new MethodResult<ExtraPracticeModel>();
            ExtraPracticeModel extraPracticeModel = new ExtraPracticeModel();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var extraPractice = await _extraPracticeRepository.GetByIdAsync(request.ExtraPracticeId);
            if (extraPractice != null)
            {
                extraPracticeModel = await SwitchExtraPractice(extraPractice, studentId);
            }
            methodResult.Result = extraPracticeModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<ExtraPracticeModel> GetExtraPracticeModel(Guid id, Guid extraPracticeResultId)
        {
            var extraPracticeModel = new ExtraPracticeModel();
            var extraPractice = await _extraPracticeRepository.Queryable
                            .Include(x => x.ExtraPracticeResults)
                            .FirstOrDefaultAsync(x => x.Id == id);
            if (extraPractice != null)
            {
                extraPracticeModel = new ExtraPracticeModel
                {
                    Id = extraPractice.Id,
                    Abstract = extraPractice.Abstract,
                    Author = extraPractice.Author,
                    Code = extraPractice.Code,
                    CourseLevel = extraPractice.CourseLevel,
                    BookFilePath = extraPractice.BookFilePath,
                    BookCoverPath = extraPractice.BookCoverPath,
                    BookBackgroundPath = extraPractice.BookBackgroundPath,
                    ImagePath = extraPractice.ImagePath,
                    InstructionContent = extraPractice.InstructionContent,
                    IsActive = extraPractice.IsActive,
                    Name = extraPractice.Name,
                    Type = extraPractice.Type,
                    VideoLink = extraPractice.VideoLink,
                    ExtraPracticeResult = extraPractice.ExtraPracticeResults.Where(m => m.Id == extraPracticeResultId)
                    .Select(x => new ExtraPraticeResultModel
                    {
                        Id = x.Id,
                        CorrectCount = x.CorrectCount,
                        CorrectTotal = x.CorrectTotal,
                        SkillScores = x.SkillScores,
                        Percent = x.Percent,
                        Status = x.Status,
                        StudentId = x.StudentId,
                        ExtraPracticeId = x.ExtraPracticeId
                    }).FirstOrDefault()
                };
            }

            return extraPracticeModel;
        }
        public async Task<VideoModel> GetVideoModel(Guid id)
        {
            var videoModel = new VideoModel();
            var video = await _videoRepository.Queryable.Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                                     .Include(i => i.VideoTimeCodes.Where(x => !x.IsDeleted))
                                     .ThenInclude(x => x.TimeCodeExercises)
                                     .ThenInclude(x => x.Exercise)
                                     .ThenInclude(x => x.ExerciseQuestions)
                                     .ThenInclude(x => x.Question)
                                .Include(i => i.VideoResults.Where(x => !x.IsDeleted))
                                .Where(x => x.Id == id)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();
            if (video != null)
            {
                videoModel = new VideoModel
                {
                    Id = video.Id,
                    Name = video.Name,
                    VideoFilePath = video.VideoFilePath,
                    IsActive = video.LessonVideos.Any(),
                    TeacherId = video.TeacherId,
                    CourseLevel = video.CourseLevel,
                    SubFilePath = video.SubFilePath,
                    Type = video.Type,
                    VideoTimeCodes = video.VideoTimeCodes.OrderBy(x => x!.DisplayTime).Select(x => new VideoTimeCodeModel
                    {
                        Id = x.Id,
                        TotalCount = x.TimeCodeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(m => m.Question).Count(),
                        DisplayTime = x.DisplayTime,
                        ExecutionTime = x.ExecutionTime,
                        TimeCodeType = x.TimeCodeType,
                        VideoId = x.VideoId,
                    }).ToList(),
                };
            }
            return videoModel;
        }
        public async Task<ExtraPracticeModel> GetExtraPracticeChapterModel(Guid id, Guid extraPracticeResultId)
        {
            var extraPracticeModel = new ExtraPracticeModel();
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeResults.Where(y => !y.IsDeleted))
                                                                  .Include(x => x.ExtraPracticeChapters.Where(y => !y.IsDeleted))
                                                                      .ThenInclude(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                                      .ThenInclude(x => x.ExtraPracticeExerciseResults)
                                                                      .AsNoTracking()
                                                                  .FirstOrDefaultAsync(x => x.Id == id);
            if (extraPractice != null)
            {
                extraPracticeModel = new ExtraPracticeModel
                {
                    Id = extraPractice!.Id,
                    Abstract = extraPractice.Abstract,
                    Author = extraPractice.Author,
                    Code = extraPractice.Code,
                    CourseLevel = extraPractice.CourseLevel,
                    BookBackgroundPath = extraPractice.BookBackgroundPath,
                    BookCoverPath = extraPractice.BookCoverPath,
                    BookFilePath = extraPractice.BookFilePath,
                    ImagePath = extraPractice.ImagePath,
                    InstructionContent = extraPractice.InstructionContent,
                    IsActive = extraPractice.IsActive,
                    Name = extraPractice.Name,
                    Type = extraPractice.Type,
                    VideoLink = extraPractice.VideoLink,

                    AccessCount = extraPractice.ExtraPracticeResults.Count,

                    ChapterCount = extraPractice.ExtraPracticeChapters.Count(x => x.ExtraPracticeExercises.All(e => e.ExtraPracticeExerciseResults.Any(r => r.Status == EnumResultStatus.Done || r.ExecuteCount > 0))),
                    ChapterTotal = extraPractice.ExtraPracticeChapters.Count,

                    QuizzesCount = extraPractice.ExtraPracticeChapters.SelectMany(c => c.ExtraPracticeExercises)
                                                                  .SelectMany(e => e.ExtraPracticeExerciseResults)
                                                                  .Count(r => r.Status == EnumResultStatus.Done || r.ExecuteCount > 0),
                    QuizzesTotal = extraPractice.ExtraPracticeChapters.SelectMany(c => c.ExtraPracticeExercises).Count(),
                    ExtraPracticeChapters = extraPractice.ExtraPracticeChapters.OrderBy(x => x!.PageNumber).Select(x => new ExtraPracticeChapterModel
                    {
                        Id = x.Id,
                        Description = x.Description,
                        Name = x.Name,
                        ExtraPracticeId = x.ExtraPracticeId,
                        PageNumber = x.PageNumber,
                        IsStatus = x.ExtraPracticeExercises.SelectMany(x => x.ExtraPracticeExerciseResults).All(x => x.Status == EnumResultStatus.Done || x.ExecuteCount > 0),
                    }).ToList(),
                    ExtraPracticeResult = extraPractice.ExtraPracticeResults.Where(x => x.Id == extraPracticeResultId).Select(x => new ExtraPraticeResultModel
                    {
                        Id = x.Id,
                        CorrectCount = x.CorrectCount,
                        CorrectTotal = x.CorrectTotal,
                        SkillScores = x.SkillScores,
                        Percent = x.Percent,
                        Status = x.Status,
                        StudentId = x.StudentId,
                        ExtraPracticeId = x.ExtraPracticeId
                    }).FirstOrDefault()
                };
            }
            return extraPracticeModel;
        }

        public async Task<MockTestModel> GetMockTestModel(Guid id)
        {
            var extraPractice = await _mockTestRepository.Queryable.Include(x => x!.MockTestSections)
                                                                    .ThenInclude(x => x.SectionGroup)
                                                                    .ThenInclude(x => x.Sections)
                                                                    .ThenInclude(x => x.SectionGroup)
                                                                    .ThenInclude(x => x.SectionGroup)
                                                                    .FirstOrDefaultAsync(x => x.Id == id);
            var mockTestModel = new MockTestModel
            {
                Id = extraPractice.MockTest!.Id,
                Name = extraPractice.MockTest.Name,
                MockTestType = extraPractice.MockTest.MockTestType,
                CourseType = extraPractice.MockTest.CourseType,
                CreatedDate = extraPractice.MockTest.CreatedDate,
                CreatedFullName = extraPractice.MockTest.CreatedFullName,
                CreatedUserId = extraPractice.MockTest.CreatedUserId,
                IsActive = extraPractice.MockTest.UnitSkillMockTests.Any() || extraPractice.MockTest.CourseUnitMockTests.Any(),
                ExecutionTime = extraPractice.
            };
            return mockTestModel;
        }

        public async Task<ExtraPracticeModel> SwitchExtraPractice(ExtraPractice? extraPractice, Guid studentId)
        {
            ArgumentNullException.ThrowIfNull(extraPractice);
            ExtraPracticeModel extraPracticeModel = new ExtraPracticeModel();
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ExtraPracticeId == extraPractice.Id);
            if (extraPracticeResult != null)
            {
                extraPracticeModel = await GetExtraPracticeModel(extraPractice.Id, extraPracticeResult.Id);
                switch (extraPractice.Type)
                {
                    case EnumExtraPracticeType.Book:
                        extraPracticeModel = await GetExtraPracticeChapterModel(extraPractice.Id, extraPracticeResult.Id);
                        break;

                    case EnumExtraPracticeType.Exercise:
                        extraPracticeModel = await GetExtraPracticeInExercise(extraPractice.Id, studentId);
                        break;

                    case EnumExtraPracticeType.InteractiveVideo:
                        extraPracticeModel.Video = await GetVideoModel(extraPractice.Id);
                        break;

                    case EnumExtraPracticeType.VideoEmbed:
                        extraPracticeModel = await GetExtraPracticeInExercise(extraPractice.Id, studentId);
                        break;

                    case EnumExtraPracticeType.Articles:
                        break;

                    case EnumExtraPracticeType.MockTest:
                        if (extraPractice.MockTestId != null)
                        {
                            extraPracticeModel = await GetExtraPracticeInMockTest(extraPractice.Id, studentId);
                        }
                        else
                        {
                            extraPracticeModel = await GetExtraPracticeInPlacementTest(extraPractice.Id, studentId);
                        }
                        break;

                    default:
                        break;
                }
            }

            return extraPracticeModel;
        }
    }
}
