// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ExtraPracticeCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartExtraPracticeCommand : IRequest<MethodResult<ExtraPracticeResultModel>>
    {
        public Guid ExtraPracticeId { get; set; }
    }

    public class StartExtraPracticeCommandHandler : IRequestHandler<StartExtraPracticeCommand, MethodResult<ExtraPracticeResultModel>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;

        public StartExtraPracticeCommandHandler(IExtraPracticeRepository extraPracticeRepository
            , IMapper mapper
            , AuthContext authContext
            , IUserService userService
            , IExtraPracticeResultRepository extraPracticeResultRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _extraPracticeResultRepository = extraPracticeResultRepository;
        }

        public async Task<MethodResult<ExtraPracticeResultModel>> Handle(StartExtraPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExtraPracticeResultModel>();

            var extraPractice = await _extraPracticeRepository.GetByIdAsync(request.ExtraPracticeId);
            if (extraPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(extraPractice));
                return methodResult;
            }

            var student = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ExtraPracticeId == request.ExtraPracticeId, cancellationToken);
            if (extraPracticeResult == null)
            {
                var a = await GetCorrectTotal(extraPractice, studentId ?? default);
                extraPracticeResult = new ExtraPracticeResult
                {
                    StudentId = studentId ?? default,
                    CorrectTotal = a.Item1,
                    Status = EnumResultStatus.New,
                    ExtraPracticeExerciseResults = a.Item2 != null ? a.Item2 : new List<ExtraPracticeExerciseResult>(),
                };
                extraPractice.ExtraPracticeResults.Add(extraPracticeResult);
                _extraPracticeRepository.Update(extraPractice);
                await _extraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<ExtraPracticeResultModel>(extraPracticeResult);
            return methodResult;
        }

        private async Task<(int, IList<ExtraPracticeExerciseResult>?)> GetCorrectTotal(ExtraPractice extraPractice, Guid studentId)
        {
            int correctTotal = 0;
            IList<ExtraPracticeExerciseResult>? extraPracticeExerciseResults = null;
            switch (extraPractice.Type)
            {
                case EnumExtraPracticeType.VideoEmbed:
                    (correctTotal, extraPracticeExerciseResults) = await GetCorrectTotalVideoEmbed(extraPractice.Id, studentId);
                    break;

                case EnumExtraPracticeType.MockTest:
                    (correctTotal, extraPracticeExerciseResults) = await GetCorrectTotalMockTest(extraPractice.Id);
                    break;

                case EnumExtraPracticeType.Exercise:
                    (correctTotal, extraPracticeExerciseResults) = await GetCorrectTotalExercise(extraPractice.Id, studentId);
                    break;

                case EnumExtraPracticeType.InteractiveVideo:
                    (correctTotal, extraPracticeExerciseResults) = await GetCorrectTotalInteractiveVideo(extraPractice.Id);
                    break;

                case EnumExtraPracticeType.Book:
                    (correctTotal, extraPracticeExerciseResults) = await GetCorrectTotalBook(extraPractice.Id, studentId);
                    break;

                case EnumExtraPracticeType.Articles:
                    (correctTotal, extraPracticeExerciseResults) = (0, null);
                    break;
            }
            return (correctTotal, extraPracticeExerciseResults);
        }

        private async Task<(int, IList<ExtraPracticeExerciseResult>?)> GetCorrectTotalVideoEmbed(Guid id, Guid studentId)
        {
            var extraPractice = await _extraPracticeRepository.GetIncludeByTypeVideoEmbedAsync(id);
            if (extraPractice != null)
            {
                IList<ExtraPracticeExerciseResult> extraPracticeExerciseResults = extraPractice.ExtraPracticeExercises.OrderBy(x => x.CreatedDate)
                   .Select((x, index) => new ExtraPracticeExerciseResult
                   {
                       CorrectTotal = x.Exercise!.ExerciseQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                       CourseSkill = x.Exercise.CourseSkill,
                       ExtraPracticeExerciseId = x.Id,
                       StudentId = studentId,
                       Status = index == 0 ? EnumResultStatus.New : EnumResultStatus.Unfinished
                   }).ToList();
                var correctTotal = extraPracticeExerciseResults.Sum(x => x!.CorrectTotal);
                return (correctTotal, extraPracticeExerciseResults);
            }
            return (0, null);
        }

        private async Task<(int, IList<ExtraPracticeExerciseResult>?)> GetCorrectTotalExercise(Guid id, Guid studentId)
        {
            var extraPractice = await _extraPracticeRepository.GetIncludeByTypeVideoEmbedAsync(id);
            if (extraPractice != null)
            {
                IList<ExtraPracticeExerciseResult> extraPracticeExerciseResults = extraPractice.ExtraPracticeExercises.OrderBy(x => x.CreatedDate)
                   .Select((x, index) => new ExtraPracticeExerciseResult
                   {
                       CorrectTotal = x.Exercise!.ExerciseQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                       CourseSkill = x.Exercise.CourseSkill,
                       ExtraPracticeExerciseId = x.Id,
                       StudentId = studentId,
                       Status = index == 0 ? EnumResultStatus.New : EnumResultStatus.Unfinished
                   }).ToList();
                var correctTotal = extraPracticeExerciseResults.Sum(x => x!.CorrectTotal);
                return (correctTotal, extraPracticeExerciseResults);
            }
            return (0, null);
        }

        private async Task<(int, IList<ExtraPracticeExerciseResult>?)> GetCorrectTotalMockTest(Guid id)
        {
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.MockTest)
                                .Include(x => x.PlacementTest)
                                .FirstOrDefaultAsync(x => x.Id == id);
            int correctTotal = 0;
            if (extraPractice != null && extraPractice.MockTest != null)
            {
                extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.MockTest)
                                                                        .ThenInclude(x => x!.MockTestSections)
                                                                        .ThenInclude(x => x.SectionGroup)
                                                                        .FirstOrDefaultAsync(x => x.Id == id);
                if (extraPractice != null && extraPractice.MockTest!.MockTestType == EnumMockTestType.SkillMockTest)
                {
                    var sectionGroup = extraPractice.MockTest.MockTestSections.Select(x => x.SectionGroup).FirstOrDefault();
                    if (sectionGroup != null)
                    {
                        switch (sectionGroup.CourseSkill)
                        {
                            case EnumCourseSkill.Reading:
                            case EnumCourseSkill.Listening:
                                correctTotal = 40;
                                break;

                            case EnumCourseSkill.Writing:
                                correctTotal = 36;
                                break;

                            case EnumCourseSkill.Speaking:
                                correctTotal = 36;
                                break;
                        }
                    }

                    return (correctTotal, null);
                }
                else
                {
                    correctTotal = 40 * 2 + 36 * 2;
                    return (correctTotal, null);
                }
            }
            else if (extraPractice != null && extraPractice.PlacementTest != null)
            {
                if (extraPractice.PlacementTest.PlacementTestLevel == EnumPlacementTestLevel.IELTS)
                {
                    correctTotal = 40 * 2;
                }
                else
                {
                    extraPractice = await _extraPracticeRepository.GetIncludeByPlacementTestAsync(id);

                    correctTotal = extraPractice!.PlacementTest!.PlacementTestSections
                                .Select(x => x.SectionGroup)
                                .SelectMany(x => x!.Sections)
                                .SelectMany(x => x.SectionQuestions)
                                .Select(x => x.Question)
                                .Sum(x => x!.CorrectTotal);
                }

                return (correctTotal, null);
            }
            return (0, null);
        }

        private async Task<(int, IList<ExtraPracticeExerciseResult>?)> GetCorrectTotalInteractiveVideo(Guid id)
        {
            var extraPractice = await _extraPracticeRepository.GetIncludeByTypeInteractiveVideoAsync(id);
            if (extraPractice != null && extraPractice.Video != null)
            {
                var correctTotal = extraPractice.Video.VideoTimeCodes
                               .SelectMany(x => x.TimeCodeExercises)
                               .Select(x => x.Exercise)
                               .SelectMany(x => x!.ExerciseQuestions)
                               .Select(x => x.Question)
                               .Sum(x => x!.CorrectTotal);
                return (correctTotal, null);
            }
            return (0, null);
        }

        private async Task<(int, IList<ExtraPracticeExerciseResult>?)> GetCorrectTotalBook(Guid id, Guid studentId)
        {
            var extraPractice = await _extraPracticeRepository.GetIncludeByTypeBookAsync(id);
            if (extraPractice != null)
            {
                IList<ExtraPracticeExerciseResult> extraPracticeExerciseResults = extraPractice.ExtraPracticeChapters
                   .SelectMany(x => x.ExtraPracticeExercises).OrderBy(x => x.CreatedDate)
                   .Select((x, index) => new ExtraPracticeExerciseResult
                   {
                       CorrectTotal = x.Exercise!.ExerciseQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                       CourseSkill = x.Exercise.CourseSkill,
                       ExtraPracticeExerciseId = x.Id,
                       StudentId = studentId,
                       Status = index == 0 ? EnumResultStatus.New : EnumResultStatus.Unfinished
                   }).ToList();
                var correctTotal = extraPracticeExerciseResults.Sum(x => x.CorrectTotal);
                return (correctTotal, extraPracticeExerciseResults);
            }
            return (0, null);
        }
    }
}
