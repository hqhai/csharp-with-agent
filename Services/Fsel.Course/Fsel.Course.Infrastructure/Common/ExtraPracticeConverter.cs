// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPractices;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPractiveChapters;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class ExtraPracticeConverter
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly VideoConverter _videoConverter;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly SectionConverter _sectionConverter;
        private readonly IExtraPracticeExerciseRepository _extraPracticeExerciseRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;

        public ExtraPracticeConverter(IExtraPracticeRepository extraPracticeRepository, IMapper mapper
            , VideoConverter videoConverter
            , IMockTestRepository mockTestRepository
            , IVideoRepository videoRepository
            , SectionConverter sectionConverter
            , IExtraPracticeExerciseRepository extraPracticeExerciseRepository
            , IExerciseRepository exerciseRepository
            , IExtraPracticeResultRepository extraPracticeResultRepository
            , IQuestionRepository questionRepository
            , QuestionTypeConverter questionTypeConverter
            )
        {
            _extraPracticeRepository = extraPracticeRepository;
            _mapper = mapper;
            _videoConverter = videoConverter;
            _mockTestRepository = mockTestRepository;
            _sectionConverter = sectionConverter;
            _videoRepository = videoRepository;
            _sectionConverter = sectionConverter;
            _extraPracticeExerciseRepository = extraPracticeExerciseRepository;
            _exerciseRepository = exerciseRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _questionRepository = questionRepository;
            _questionTypeConverter = questionTypeConverter;
        }

        public VoidMethodResult AddExtraPracticeChapterExercise(dynamic extraPracticeChapters, IList<CreateExtraPracticeChapterCommandModel>? exercisePracticeChapters)
        {
            ArgumentNullException.ThrowIfNull(exercisePracticeChapters);
            VoidMethodResult methodResult = new VoidMethodResult();
            foreach (var item in exercisePracticeChapters)
            {
                ExtraPracticeChapter extraPracticeChapter = _mapper.Map<ExtraPracticeChapter>(item);
                if (item == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeChapterErrorCode.ExtraPracticeChapterNull));
                    return methodResult;
                }
                if (item.Exercises != null && item.Exercises.Count > 0)
                {
                    foreach (var exercise in item.Exercises)
                    {
                        if (exercise == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExerciseNull));
                            return methodResult;
                        }
                        Exercise excerciseNew = _mapper.Map<Exercise>(exercise);
                        var method = _videoConverter.AddQuestionToExercise(excerciseNew, exercise);
                        if (!method.IsOK)
                        {
                            methodResult.AddErrorBadRequest(method.ErrorMessages);
                            return methodResult;
                        }
                        extraPracticeChapter.ExtraPracticeExercises.Add(new ExtraPracticeExercise { Exercise = excerciseNew });
                    }
                    extraPracticeChapters.Add(extraPracticeChapter);
                }
            }
            return methodResult;
        }

        public VoidMethodResult AddExerciseToExtraPractice(dynamic extraPracticeExercises, IList<CreateExerciseCommandModel>? exercises)
        {
            ArgumentNullException.ThrowIfNull(exercises);
            VoidMethodResult methodResult = new VoidMethodResult();
            if (exercises == null || exercises.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExercisesNull));
                return methodResult;
            }
            foreach (var exercise in exercises)
            {
                if (exercise == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExerciseNull));
                    return methodResult;
                }
                Exercise excerciseNew = _mapper.Map<Exercise>(exercise);
                var method = _videoConverter.AddQuestionToExercise(excerciseNew, exercise);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                extraPracticeExercises.Add(new ExtraPracticeExercise { Exercise = excerciseNew });
            }
            return methodResult;
        }

        public async Task<VoidMethodResult> CreateExtraPractice(dynamic extraPractice, CreateExtraPracticeCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();
            if (await _extraPracticeRepository.Queryable.AnyAsync(x => x.Code == request.Code))
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.CodeAlreadyExists), nameof(request.Code), request.Code);
                return methodResult;
            }
            List<ExtraPracticeExercise> extraPracticeExercises = new List<ExtraPracticeExercise>();
            List<ExtraPracticeChapter> extraPracticeChapters = new List<ExtraPracticeChapter>();
            if (request.Type == EnumExtraPracticeType.Book)
            {
                if (request.ExtraPracticeChapters == null || request.ExtraPracticeChapters.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeChapterErrorCode.ExtraPracticeChaptersNull));
                    return methodResult;
                }
                var method = AddExtraPracticeChapterExercise(extraPracticeChapters, request.ExtraPracticeChapters);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
            }
            else if (request.Type == EnumExtraPracticeType.VideoEmbed || request.Type == EnumExtraPracticeType.Exercise)
            {
                if (request.Exercises == null || request.Exercises.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExercisesNull));
                    return methodResult;
                }
                var method = AddExerciseToExtraPractice(extraPracticeExercises, request.Exercises);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
            }
            else if (request.Type == EnumExtraPracticeType.InteractiveVideo)
            {
                if (request.Video == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNull));
                    return methodResult;
                }
                Video video = _mapper.Map<Video>(request.Video);
                var method = await _videoConverter.CreateTimeCodeToVideo(video, request.Video);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                extraPractice.Video = video;
            }
            else if (request.Type == EnumExtraPracticeType.MockTest)
            {
                if (request.MockTestId == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestIdNotNull));
                    return methodResult;
                }
                var isCheck = await _mockTestRepository.Queryable.AnyAsync(x => x.Id == request.MockTestId) && !(await _extraPracticeRepository.Queryable.AnyAsync(x => x.MockTestId == request.MockTestId));
                if (isCheck)
                {
                    extraPractice.MockTestId = request.MockTestId;
                }
                else
                {
                    methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestAlreadyExistToExtraPractice));
                    return methodResult;
                }
            }
            if (extraPracticeExercises.Count > 0)
            {
                extraPractice.ExtraPracticeExercises = extraPracticeExercises;
            }
            else if (extraPracticeChapters.Count > 0)
            {
                extraPractice.ExtraPracticeChapters = extraPracticeChapters;
            }

            return methodResult;
        }

        public async Task<VoidMethodResult> UpdateExtraPractice(dynamic extraPractice, UpdateExtraPracticeCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();
            if (await _extraPracticeRepository.Queryable.AnyAsync(x => x.Id != request.Id && x.Code == request.Code))
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.CodeAlreadyExists), nameof(request.Code), request.Code);
                return methodResult;
            }
            List<ExtraPracticeExercise> extraPracticeExercises = new List<ExtraPracticeExercise>();
            List<ExtraPracticeChapter> extraPracticeChapters = new List<ExtraPracticeChapter>();
            if (request.Type == EnumExtraPracticeType.Book && request.ExtraPracticeChapters != null && request.ExtraPracticeChapters.Count > 0)
            {
                var method = AddExtraPracticeChapterExercise(extraPracticeChapters, request.ExtraPracticeChapters);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
            }
            else if (request.Type == EnumExtraPracticeType.VideoEmbed || request.Type == EnumExtraPracticeType.Exercise)
            {
                var method = AddExerciseToExtraPractice(extraPracticeExercises, request.Exercises);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
            }
            else if (request.Type == EnumExtraPracticeType.InteractiveVideo)
            {
                var video = await _videoRepository.GetIncludeByIdAsync(request.VideoId ?? Guid.Empty);
                if (request.Video == null || request.VideoId == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNull));
                    return methodResult;
                }
                if (video == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNotExist), nameof(request.Id), request.Id);
                    return methodResult;
                }
                request.Video.Id = video.Id;
                _mapper.Map(request.Video, video);
                var methodUpdate = await _videoConverter.UpdateTimeCodeToVideo(video, request.Video);
                if (!methodUpdate.IsOK)
                {
                    methodResult.AddErrorBadRequest(methodUpdate.ErrorMessages);
                    return methodResult;
                }
                extraPractice.Video = video;
            }
            else if (request.Type == EnumExtraPracticeType.MockTest)
            {
                if (request.MockTestId == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.MockTestIdNotNull));
                    return methodResult;
                }
                if (await _mockTestRepository.Queryable.AnyAsync(x => x.Id == request.MockTestId))
                {
                    extraPractice.MockTestId = request.MockTestId;
                }
            }
            _mapper.Map(request, extraPractice);
            if (extraPracticeExercises.Count > 0)
            {
                extraPractice.ExtraPracticeExercises.Clear();
                extraPractice.ExtraPracticeExercises = extraPracticeExercises;
            }
            else if (extraPracticeChapters.Count > 0)
            {
                extraPractice.ExtraPracticeChapters = extraPracticeChapters;
            }

            return methodResult;
        }

        public async Task<VoidMethodResult> DeleteExtraPractice(ExtraPractice? extraPractice, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPractice);
            VoidMethodResult methodResult = new VoidMethodResult();
            IList<ExtraPracticeExercise> extraPracticeExercises = new List<ExtraPracticeExercise>();
            IList<Exercise> exercises = new List<Exercise>();
            IList<Question> questions = new List<Question>();
            if (extraPractice.ExtraPracticeExercises != null && extraPractice.ExtraPracticeExercises.Count > 0)
            {
                extraPracticeExercises = extraPractice.ExtraPracticeExercises.ToList();
                extraPractice.ExtraPracticeExercises.Clear();
            }
            else if (extraPractice.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises).ToList().Count > 0)
            {
                extraPracticeExercises = extraPractice.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises).ToList();
                extraPractice.ExtraPracticeChapters.Clear();
            }
            else if (extraPractice.Video != null)
            {
                exercises = extraPractice.Video.VideoTimeCodes.SelectMany(x => x.TimeCodeExercises).Select(x => x.Exercise ?? new Exercise()).ToList();
                questions = exercises.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question ?? new Question()).ToList();
            }
            if (extraPracticeExercises != null && extraPracticeExercises.Count > 0)
            {
                exercises = extraPracticeExercises.Select(x => x.Exercise ?? new Exercise()).ToList();
                questions = exercises.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question ?? new Question()).ToList();
            }
            if (extraPracticeExercises != null)
            {
                foreach (var item in extraPracticeExercises)
                {
                    await _extraPracticeExerciseRepository.DeleteAsync(item);
                }
                await _extraPracticeExerciseRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            if (exercises != null)
            {
                foreach (var item in exercises)
                {
                    await _exerciseRepository.DeleteAsync(item);
                }
                await _exerciseRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var item in questions)
                {
                    await _questionRepository.DeleteAsync(item);
                }
                await _questionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return methodResult;
        }

        public async Task<ExtraPracticeModel> GetExtraPracticeInChapter(Guid id, Guid studentId)
        {
            var extraPractice = await _extraPracticeRepository.Queryable
                                                                  .Include(x => x.ExtraPracticeResults.Where(y => !y.IsDeleted))
                                                                  .Include(x => x.ExtraPracticeChapters.Where(y => !y.IsDeleted))
                                                                      .ThenInclude(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                                      .ThenInclude(x => x.Exercise)
                                                                      .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                                                                      .ThenInclude(x => x.Question)
                                                                      .ThenInclude(x => x!.ExtraPracticeAnswers.Where(y => !y.IsDeleted))
                                                                  .Include(x => x.ExtraPracticeChapters.Where(y => !y.IsDeleted))
                                                                      .ThenInclude(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                                      .ThenInclude(x => x.ExtraPracticeExerciseResults)
                                                                      .AsNoTracking()
                                                                  .FirstOrDefaultAsync(x => x.Id == id);
            var extraPracticeResult = extraPractice != null ? extraPractice.ExtraPracticeResults.FirstOrDefault(y => y.StudentId == studentId) : null;
            var checkDone = extraPractice != null && extraPracticeResult != null && extraPracticeResult.Status == EnumResultStatus.Done;
            var extraPracticeModel = new ExtraPracticeModel
            {
                Id = extraPractice!.Id,
                Abstract = extraPractice.Abstract,
                Author = extraPractice.Author,
                Code = extraPractice.Code,
                CourseLevel = extraPractice.CourseLevel,
                BookBackgroundPath = extraPractice.BookBackgroundPath,
                BookCoverPath = extraPractice.BookCoverPath,
                BookFilePath = extraPractice.BookFilePath,
                InstructionContent = extraPractice.InstructionContent,
                IsActive = extraPractice.IsActive,
                Name = extraPractice.Name,
                Type = extraPractice.Type,
                VideoLink = extraPractice.VideoLink,
                AccessCount = extraPractice.ExtraPracticeResults.Count,
                ChapterCount = extraPractice.ExtraPracticeChapters.Count(x => x.ExtraPracticeExercises.All(e => e.ExtraPracticeExerciseResults.Any(r => r.Status == EnumResultStatus.Done))),
                ChapterTotal = extraPractice.ExtraPracticeChapters.Count,
                QuizzesCount = extraPractice.ExtraPracticeChapters.SelectMany(c => c.ExtraPracticeExercises)
                                                                  .SelectMany(e => e.ExtraPracticeExerciseResults)
                                                                  .Count(r => r.Status == EnumResultStatus.Done),
                QuizzesTotal = extraPractice.ExtraPracticeChapters.SelectMany(c => c.ExtraPracticeExercises)
                                                                  .Count(),
                ExtraPracticeChapters = extraPractice.ExtraPracticeChapters.OrderBy(x => x!.PageNumber).Select(x => new ExtraPracticeChapterModel
                {
                    Id = x.Id,
                    Description = x.Description,
                    Name = x.Name,
                    ExtraPracticeId = x.ExtraPracticeId,
                    PageNumber = x.PageNumber,
                    ExtraPracticeExercises = x.ExtraPracticeExercises.OrderBy(x => x!.CreatedDate).Select(x => new ExtraPracticeExerciseModel
                    {
                        Id = x.Id,
                        CreatedDate = x.CreatedDate,
                        TotalCount = x.Exercise!.ExerciseQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                        Exercise = new ExerciseModel
                        {
                            Id = x.Exercise!.Id,
                            MediaPost = x.Exercise.MediaPost,
                            CourseSkill = x.Exercise.CourseSkill,
                            Questions = x.Exercise.ExerciseQuestions.Select(x => x.Question).OrderBy(x => x!.CreatedDate).Select(m => new QuestionModel()
                            {
                                Id = m!.Id,
                                QuestionType = m!.QuestionType,
                                CorrectTotal = m!.CorrectTotal,
                                Explanation = m!.Explanation,
                                Ungraded = m!.Ungraded,
                                Config = _questionTypeConverter.QuestionTypeConverterObject(m!.Config, m!.QuestionType, isDisableAnswers: true).Item1,
                                ResultAnswer = _mapper.Map<AnswerModel>(m.ExtraPracticeAnswers!.FirstOrDefault())
                            }).ToList(),
                        },
                        ExtraPracticeExerciseResult = x.ExtraPracticeExerciseResults.Where(x=>x.StudentId == studentId && x.ExtraPracticeExerciseId == ).Select(x => new ExtraPracticeExerciseResultModel
                        {
                            Id = x.Id,
                            CorrectCount = x.CorrectCount,
                            CorrectTotal = x.CorrectTotal,
                            CourseSkill = x.CourseSkill,
                            Percent = x.Percent,
                            Status = x.Status,
                            StudentId = x.StudentId,
                            ExtraPracticeExerciseId = x.ExtraPracticeExerciseId,
                        }).FirstOrDefault()
                    }).ToList(),
                }).ToList(),
                ExtraPracticeResult = extraPracticeResult != null ? new ExtraPraticeResultModel
                {
                    Id = extraPracticeResult.Id,
                    CorrectCount = extraPracticeResult.CorrectCount,
                    CorrectTotal = extraPracticeResult.CorrectTotal,
                    SkillScores = extraPracticeResult.SkillScores,
                    Percent = extraPracticeResult.Percent,
                    Status = extraPracticeResult.Status,
                    StudentId = extraPracticeResult.StudentId,
                    ExtraPracticeId = extraPracticeResult.ExtraPracticeId
                } : null
            };
            return extraPracticeModel;
        }

        public async Task<ExtraPracticeModel> GetExtraPracticeInVideo(Guid id, Guid studentId)
        {
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ExtraPracticeId == id);
            var extraPractice = await _extraPracticeRepository.Queryable
                                                                  .Include(x => x.ExtraPracticeResults.Where(y => !y.IsDeleted))
                                                                  .Include(x => x.Video)
                                                                        .ThenInclude(x => x!.LessonVideos.Where(x => !x.IsDeleted))
                                                                  .Include(x => x.Video)
                                                                        .ThenInclude(i => i!.VideoTimeCodes.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.TimeCodeExercises.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.Exercise)
                                                                        .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.Question)
                                                                        .ThenInclude(x => x!.ExtraPracticeAnswers.Where(y => !y.IsDeleted))
                                                                  .Where(x => x.Id == id)
                                                                        .AsNoTracking()
                                                                  .FirstOrDefaultAsync();
            var checkDone = extraPractice != null && extraPracticeResult != null && extraPracticeResult.Status == EnumResultStatus.Done;
            var extraPracticeModel = new ExtraPracticeModel
            {
                Id = extraPractice!.Id,
                Abstract = extraPractice.Abstract,
                Author = extraPractice.Author,
                Code = extraPractice.Code,
                CourseLevel = extraPractice.CourseLevel,
                BookBackgroundPath = extraPractice.BookBackgroundPath,
                BookCoverPath = extraPractice.BookCoverPath,
                BookFilePath = extraPractice.BookFilePath,
                InstructionContent = extraPractice.InstructionContent,
                IsActive = extraPractice.IsActive,
                Name = extraPractice.Name,
                Type = extraPractice.Type,
                VideoLink = extraPractice.VideoLink,
                Video = new VideoModel
                {
                    Id = extraPractice!.Video!.Id,
                    Name = extraPractice.Video.Name,
                    VideoFilePath = extraPractice.Video.VideoFilePath,
                    IsActive = extraPractice.Video.LessonVideos.Any(),
                    TeacherId = extraPractice.Video.TeacherId,
                    CourseLevel = extraPractice.Video.CourseLevel,
                    VideoTimeCodes = extraPractice.Video.VideoTimeCodes.Where(x => !x.IsDeleted).OrderBy(x => x!.CreatedDate).Select(x => new VideoTimeCodeModel
                    {
                        Id = x.Id,
                        DisplayTime = x.DisplayTime,
                        ExecutionTime = x.ExecutionTime,
                        TimeCodeType = x.TimeCodeType,
                        VideoId = x.VideoId,
                        Exercises = x.TimeCodeExercises.Where(n => n.Exercise != null && !n.IsDeleted).Select(n => n.Exercise).OrderBy(x => x!.CreatedDate).Select(n => new ExerciseModel
                        {
                            Id = n!.Id,
                            MediaPost = n.MediaPost,
                            CourseSkill = n.CourseSkill,
                            Questions = n.ExerciseQuestions.Where(m => m.Question != null).Select(x => x.Question).OrderBy(x => x!.CreatedDate).Select(m => new QuestionModel()
                            {
                                Id = m!.Id,
                                QuestionType = m!.QuestionType,
                                CorrectTotal = m!.CorrectTotal,
                                Explanation = m!.Explanation,
                                Ungraded = m!.Ungraded,
                                Config = _questionTypeConverter.QuestionTypeConverterObject(m!.Config, m!.QuestionType, isDisableAnswers: true).Item1,
                                ResultAnswer = _mapper.Map<AnswerModel>(m.ExtraPracticeAnswers!.FirstOrDefault())
                            }).ToList(),
                        }).ToList(),
                    }).ToList(),
                },
                ExtraPracticeResult = extraPracticeResult != null ? new ExtraPraticeResultModel
                {
                    Id = extraPracticeResult.Id,
                    CorrectCount = extraPracticeResult.CorrectCount,
                    CorrectTotal = extraPracticeResult.CorrectTotal,
                    SkillScores = extraPracticeResult.SkillScores,
                    Percent = extraPracticeResult.Percent,
                    Status = extraPracticeResult.Status,
                    StudentId = extraPracticeResult.StudentId,
                    ExtraPracticeId = extraPracticeResult.ExtraPracticeId
                } : null
            };
            return extraPracticeModel;
        }

        public async Task<ExtraPracticeModel> GetExtraPracticeInExercise(Guid id, Guid studentId)
        {
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ExtraPracticeId == id);
            var extraPractice = await _extraPracticeRepository.Queryable
                                                                  .Include(x => x.ExtraPracticeResults.Where(y => !y.IsDeleted))
                                                                  .Include(x => x!.ExtraPracticeExercises.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.Exercise)
                                                                        .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.Question)
                                                                        .ThenInclude(x => x!.ExtraPracticeAnswers.Where(x => !x.IsDeleted))
                                                                 .Include(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                                      .ThenInclude(x => x.ExtraPracticeExerciseResults.Where(y => !y.IsDeleted && y.StudentId == studentId))
                                                                   .Where(x => x.Id == id)
                                                                        .AsNoTracking()
                                                                  .FirstOrDefaultAsync();
            var checkDone = extraPractice != null && extraPracticeResult != null && extraPracticeResult.Status == EnumResultStatus.Done;
            var extraPracticeModel = new ExtraPracticeModel
            {
                Id = extraPractice!.Id,
                Abstract = extraPractice.Abstract,
                Author = extraPractice.Author,
                Code = extraPractice.Code,
                CourseLevel = extraPractice.CourseLevel,
                BookFilePath = extraPractice.BookFilePath,
                BookCoverPath = extraPractice.BookCoverPath,
                BookBackgroundPath = extraPractice.BookBackgroundPath,
                InstructionContent = extraPractice.InstructionContent,
                IsActive = extraPractice.IsActive,
                Name = extraPractice.Name,
                Type = extraPractice.Type,
                VideoLink = extraPractice.VideoLink,
                ExtraPracticeExercises = extraPractice.ExtraPracticeExercises.OrderBy(x => x!.CreatedDate).Select(x => new ExtraPracticeExerciseModel
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    TotalCount = x.Exercise!.ExerciseQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                    Exercise = new ExerciseModel
                    {
                        Id = x.Exercise!.Id,
                        MediaPost = x.Exercise.MediaPost,
                        CourseSkill = x.Exercise.CourseSkill,
                        Questions = x.Exercise.ExerciseQuestions.Select(x => x.Question).OrderBy(x => x!.CreatedDate).Select(m => new QuestionModel()
                        {
                            Id = m!.Id,
                            QuestionType = m!.QuestionType,
                            CorrectTotal = m!.CorrectTotal,
                            Explanation = m!.Explanation,
                            Ungraded = m!.Ungraded,
                            Config = _questionTypeConverter.QuestionTypeConverterObject(m!.Config, m!.QuestionType, isDisableAnswers: true).Item1,
                            ResultAnswer = _mapper.Map<AnswerModel>(m.ExtraPracticeAnswers!.FirstOrDefault())
                        }).ToList(),
                    },
                    ExtraPracticeExerciseResult = x.ExtraPracticeExerciseResults.Select(x => new ExtraPracticeExerciseResultModel
                    {
                        Id = x.Id,
                        CorrectCount = x.CorrectCount,
                        CorrectTotal = x.CorrectTotal,
                        CourseSkill = x.CourseSkill,
                        Percent = x.Percent,
                        Status = x.Status,
                        StudentId = x.StudentId,
                        ExtraPracticeExerciseId = x.ExtraPracticeExerciseId,
                    }).FirstOrDefault()
                }).ToList(),
                ExtraPracticeResult = extraPractice.ExtraPracticeResults.Where(m => !m.IsDeleted && m.StudentId == studentId).Select(x => new ExtraPraticeResultModel
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
            return extraPracticeModel;
        }

        public async Task<ExtraPracticeModel> GetExtraPracticeInPlacementTest(Guid id, Guid studentId)
        {
            ExtraPracticeModel extraPracticeModel = new ExtraPracticeModel();
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.PlacementTest).FirstOrDefaultAsync(x => x.Id == id);
            if (extraPractice!.PlacementTest!.Level == EnumPlacementTestLevel.IELTS)
            {
                extraPracticeModel = await GetExtraPracticeInPlacementTestIelst(id, studentId);
            }
            else
            {
                extraPracticeModel = await GetExtraPracticeInPlacementTestAcademic(id, studentId);
            }
            return extraPracticeModel;
        }

        public async Task<ExtraPracticeModel> GetExtraPracticeInPlacementTestIelst(Guid id, Guid studentId)
        {
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ExtraPracticeId == id);
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeResults.Where(x => !x.IsDeleted))
                                                                  .Include(x => x.PlacementTest)
                                                                  .ThenInclude(x => x!.PlacementTestSections.Where(x => !x.IsDeleted))
                                                                  .ThenInclude(x => x.SectionGroup)
                                                                  .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                                  .ThenInclude(x => x.SectionParts.Where(x => !x.IsDeleted))
                                                                  .ThenInclude(x => x.SectionQuestions.Where(x => !x.IsDeleted))
                                                                  .ThenInclude(x => x.Question)
                                                                  .ThenInclude(x => x!.ExtraPracticeAnswers.Where(x => x.ExtraPracticeResultId == extraPracticeResult!.Id && !x.IsDeleted))
                                                                  .AsNoTracking()
                                                                  .FirstOrDefaultAsync(x => x.Id == id);

            var extraPracticeModel = new ExtraPracticeModel
            {
                Id = extraPractice!.Id,
                Abstract = extraPractice.Abstract,
                Author = extraPractice.Author,
                Code = extraPractice.Code,
                CourseLevel = extraPractice.CourseLevel,
                BookFilePath = extraPractice.BookFilePath,
                BookCoverPath = extraPractice.BookCoverPath,
                BookBackgroundPath = extraPractice.BookBackgroundPath,
                InstructionContent = extraPractice.InstructionContent,
                IsActive = extraPractice.IsActive,
                Name = extraPractice.Name,
                Type = extraPractice.Type,
                VideoLink = extraPractice.VideoLink,
                PlacementTest = new PlacementTestModel
                {
                    Id = extraPractice.PlacementTest!.Id,
                    Name = extraPractice.PlacementTest.Name,
                    Level = extraPractice.PlacementTest.Level,
                    CreatedDate = extraPractice.PlacementTest.CreatedDate,
                    IsActive = extraPractice.PlacementTest.IsActive,
                    SectionGroups = extraPractice.PlacementTest.PlacementTestSections.Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate).Select(x => new SectionGroupModel
                    {
                        Id = x!.Id,
                        ExecutionTime = x!.ExecutionTime,
                        CourseSkill = x.CourseSkill,
                        TotalQuestion = x!.Sections.SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).Select(x => x.Question).Count(),
                        Sections = x.Sections.OrderBy(x => x.DisplayOrder).Select(x => new SectionModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            MediaPost = x.MediaPost,
                            TargetWord = x.TargetWord,
                            DisplayOrder = x.DisplayOrder,
                            VideoFilePath = x.VideoFilePath,
                            SectionParts = x.SectionParts.OrderBy(x => x!.CreatedDate).Select(x => new SectionPartModel
                            {
                                Id = x.Id,
                                PartName = x.PartName,
                                SectionId = x.SectionId,
                                Questions = x.SectionQuestions.Select(x => x.Question).OrderBy(x => x!.CreatedDate).Select(m => new QuestionModel
                                {
                                    Id = m!.Id,
                                    QuestionType = m!.QuestionType,
                                    CorrectTotal = m!.CorrectTotal,
                                    Explanation = m!.Explanation,
                                    Ungraded = m!.Ungraded,
                                    Config = _questionTypeConverter.QuestionTypeConverterObject(m!.Config, m!.QuestionType, isDisableAnswers: true).Item1,
                                    ResultAnswer = _mapper.Map<AnswerModel>(m.ExtraPracticeAnswers!.FirstOrDefault())
                                }).ToList(),
                            }).ToList(),
                        }).ToList(),
                    }).ToList(),
                },
                ExtraPracticeResult = extraPractice.ExtraPracticeResults.Where(m => m.StudentId == studentId).Select(x => new ExtraPraticeResultModel
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
            return extraPracticeModel;
        }

        public async Task<ExtraPracticeModel> GetExtraPracticeInPlacementTestAcademic(Guid id, Guid studentId)
        {
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ExtraPracticeId == id);

            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeResults.Where(x => !x.IsDeleted))
                                                                  .Include(x => x.PlacementTest)
                                                                  .ThenInclude(x => x!.PlacementTestSections.Where(x => !x.IsDeleted))
                                                                  .ThenInclude(x => x.SectionGroup)
                                                                  .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                                  .ThenInclude(x => x.SectionQuestions.Where(x => !x.IsDeleted))
                                                                  .ThenInclude(x => x.Question)
                                                                  .ThenInclude(x => x!.ExtraPracticeAnswers.Where(x => x.ExtraPracticeResultId == extraPracticeResult!.Id && !x.IsDeleted))
                                                                  .Where(x => x.Id == id)
                                                                  .AsNoTracking()
                                                                  .FirstOrDefaultAsync();

            var extraPracticeModel = new ExtraPracticeModel
            {
                Id = extraPractice!.Id,
                Abstract = extraPractice.Abstract,
                Author = extraPractice.Author,
                Code = extraPractice.Code,
                CourseLevel = extraPractice.CourseLevel,
                BookFilePath = extraPractice.BookFilePath,
                BookCoverPath = extraPractice.BookCoverPath,
                BookBackgroundPath = extraPractice.BookBackgroundPath,
                InstructionContent = extraPractice.InstructionContent,
                IsActive = extraPractice.IsActive,
                Name = extraPractice.Name,
                Type = extraPractice.Type,
                VideoLink = extraPractice.VideoLink,
                PlacementTest = new PlacementTestModel
                {
                    Id = extraPractice.PlacementTest!.Id,
                    Name = extraPractice.PlacementTest.Name,
                    Level = extraPractice.PlacementTest.Level,
                    CreatedDate = extraPractice.PlacementTest.CreatedDate,
                    IsActive = extraPractice.PlacementTest.IsActive,
                    SectionGroups = extraPractice.PlacementTest.PlacementTestSections.Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate).Select(x => new SectionGroupModel
                    {
                        Id = x!.Id,
                        ExecutionTime = x!.ExecutionTime,
                        CourseSkill = x.CourseSkill,
                        TotalQuestion = x!.Sections.SelectMany(x => x.SectionQuestions).Select(x => x.Question).Count(),
                        Sections = x.Sections.OrderBy(x => x!.DisplayOrder).Select(x => new SectionModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            MediaPost = x.MediaPost,
                            VideoFilePath = x.VideoFilePath,
                            DisplayOrder = x.DisplayOrder,
                            TargetWord = x.TargetWord,
                            Questions = x.SectionQuestions.Select(x => x.Question).OrderBy(x => x!.CreatedDate).Select(m => new QuestionModel
                            {
                                Id = m!.Id,
                                QuestionType = m!.QuestionType,
                                CorrectTotal = m!.CorrectTotal,
                                Explanation = m!.Explanation,
                                Ungraded = m!.Ungraded,
                                Config = _questionTypeConverter.QuestionTypeConverterObject(m!.Config, m!.QuestionType, isDisableAnswers: true).Item1,
                                ResultAnswer = _mapper.Map<AnswerModel>(m.ExtraPracticeAnswers.FirstOrDefault())
                            }).ToList(),
                        }).ToList(),
                    }).ToList(),
                },
                ExtraPracticeResult = extraPractice.ExtraPracticeResults.Where(m => !m.IsDeleted && m.StudentId == studentId).Select(x => new ExtraPraticeResultModel
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
            return extraPracticeModel;
        }

        public async Task<ExtraPracticeModel> GetExtraPracticeInMockTest(Guid id, Guid studentId)
        {
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ExtraPracticeId == id);
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.MockTest)
                                                                        .ThenInclude(x => x!.MockTestSections)
                                                                        .ThenInclude(x => x.SectionGroup)
                                                                        .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.SectionParts.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.SectionQuestions.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.Question)
                                                                        .ThenInclude(x => x!.ExtraPracticeAnswers.Where(x => x.ExtraPracticeResultId == extraPracticeResult!.Id))
                                                                   .Include(x => x.MockTest)
                                                                        .ThenInclude(x => x!.MockTestSections.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.SectionGroup)
                                                                        .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.SectionTimeCodes.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.ExtraPracticeAnswers.Where(x => x.ExtraPracticeResultId == extraPracticeResult!.Id))
                                                                  .Include(x => x.MockTest)
                                                                        .ThenInclude(x => x!.MockTestSections.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.SectionGroup)
                                                                        .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                                        .ThenInclude(x => x.ExtraPracticeAnswers.Where(x => x.ExtraPracticeResultId == extraPracticeResult!.Id))
                                                                  .Include(x => x!.ExtraPracticeResults)
                                                                  .Where(x => x.Id == id)
                                                                        .AsNoTracking()
                                                                  .FirstOrDefaultAsync();
            var extraPracticeModel = new ExtraPracticeModel
            {
                Id = extraPractice!.Id,
                Abstract = extraPractice.Abstract,
                Author = extraPractice.Author,
                Code = extraPractice.Code,
                CourseLevel = extraPractice.CourseLevel,
                BookFilePath = extraPractice.BookFilePath,
                BookCoverPath = extraPractice.BookCoverPath,
                BookBackgroundPath = extraPractice.BookBackgroundPath,
                InstructionContent = extraPractice.InstructionContent,
                IsActive = extraPractice.IsActive,
                Name = extraPractice.Name,
                Type = extraPractice.Type,
                VideoLink = extraPractice.VideoLink,
                MockTest = new MockTestModel
                {
                    Id = extraPractice.MockTest!.Id,
                    Name = extraPractice.MockTest.Name,
                    MockTestType = extraPractice.MockTest.MockTestType,
                    CourseType = extraPractice.MockTest.CourseType,
                    CreatedDate = extraPractice.MockTest.CreatedDate,
                    CreatedFullName = extraPractice.MockTest.CreatedFullName,
                    CreatedUserId = extraPractice.MockTest.CreatedUserId,
                    IsActive = extraPractice.MockTest.UnitSkillMockTests.Any() || extraPractice.MockTest.CourseUnitMockTests.Any(),
                    SectionGroups = extraPractice.MockTest.MockTestSections.Where(x => x.SectionGroup != null)
                         .Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate)
                         .Select(x => _sectionConverter.GetSectionGroupModel(x, true)).ToList(),
                },
                ExtraPracticeResult = extraPractice.ExtraPracticeResults.Where(m => m.StudentId == studentId).Select(x => new ExtraPraticeResultModel
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
            return extraPracticeModel;
        }

        public async Task<ExtraPracticeModel> GetExtraPracticeInArticles(Guid id, Guid studentId)
        {
            var extraPractice = await _extraPracticeRepository.Queryable
                .Include(x => x.ExtraPracticeResults.Where(y => y.StudentId == studentId)).FirstOrDefaultAsync(x => x.Id == id);

            var extraPracticeModel = new ExtraPracticeModel
            {
                Id = extraPractice!.Id,
                Abstract = extraPractice.Abstract,
                Author = extraPractice.Author,
                Code = extraPractice.Code,
                CourseLevel = extraPractice.CourseLevel,
                BookFilePath = extraPractice.BookFilePath,
                BookCoverPath = extraPractice.BookCoverPath,
                BookBackgroundPath = extraPractice.BookBackgroundPath,
                InstructionContent = extraPractice.InstructionContent,
                IsActive = extraPractice.IsActive,
                Name = extraPractice.Name,
                Type = extraPractice.Type,
                VideoLink = extraPractice.VideoLink,
                ExtraPracticeResult = extraPractice.ExtraPracticeResults.Where(m => !m.IsDeleted).Select(x => new ExtraPraticeResultModel
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
            return extraPracticeModel;
        }

        public async Task<ExtraPracticeModel> SwitchExtraPractice(ExtraPractice? extraPractice, Guid studentId)
        {
            ArgumentNullException.ThrowIfNull(extraPractice);
            ExtraPracticeModel extraPracticeModel = new ExtraPracticeModel();
            switch (extraPractice.Type)
            {
                case EnumExtraPracticeType.Book:
                    extraPracticeModel = await GetExtraPracticeInChapter(extraPractice.Id, studentId);
                    break;

                case EnumExtraPracticeType.Exercise:
                    extraPracticeModel = await GetExtraPracticeInExercise(extraPractice.Id, studentId);
                    break;

                case EnumExtraPracticeType.InteractiveVideo:
                    extraPracticeModel = await GetExtraPracticeInVideo(extraPractice.Id, studentId);
                    break;

                case EnumExtraPracticeType.VideoEmbed:
                    extraPracticeModel = await GetExtraPracticeInExercise(extraPractice.Id, studentId);
                    break;

                case EnumExtraPracticeType.Articles:
                    extraPracticeModel = await GetExtraPracticeInArticles(extraPractice.Id, studentId);
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
            return extraPracticeModel;
        }
    }
}
