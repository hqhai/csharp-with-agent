// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ExtraPracticeCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartExtraPracticeCommand : IRequest<MethodResult<ExtraPracticeModel>>
    {
        public Guid ExtraPracticeId { get; set; }
    }

    public class StartExtraPracticeCommandHandler : IRequestHandler<StartExtraPracticeCommand, MethodResult<ExtraPracticeModel>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ExtraPracticeConverter _extraPracticeConverter;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;

        public StartExtraPracticeCommandHandler(IExtraPracticeRepository extraPracticeRepository
            , AuthContext authContext
            , IUserService userService
            , ExtraPracticeConverter extraPracticeConverter
            , IExtraPracticeResultRepository extraPracticeResultRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _authContext = authContext;
            _userService = userService;
            _extraPracticeConverter = extraPracticeConverter;
            _extraPracticeResultRepository = extraPracticeResultRepository;
        }

        public async Task<MethodResult<ExtraPracticeModel>> Handle(StartExtraPracticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExtraPracticeModel>();

            var extraPractice = await _extraPracticeRepository.GetByIdAsync(request.ExtraPracticeId);
            if (extraPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(extraPractice));
                return methodResult;
            }

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ExtraPracticeId == request.ExtraPracticeId, cancellationToken);
            if (extraPracticeResult == null)
            {
                var a = await GetCorrectTotal(extraPractice, studentId ?? default);
                extraPractice.ExtraPracticeResults.Add(new ExtraPracticeResult
                {
                    StudentId = studentId ?? default,
                    CorrectTotal = a.Item1,
                    ExtraPracticeExerciseResults = a.Item2 != null ? a.Item2 : new List<ExtraPracticeExerciseResult>()
                });
                _extraPracticeRepository.Update(extraPractice);
                await _extraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await _extraPracticeConverter.SwitchExtraPractice(extraPractice, studentId ?? default);
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
                    (correctTotal, extraPracticeExerciseResults) = await GetCorrectTotalExercise(extraPractice.Id);
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
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x!.ExerciseQuestions)
                                .ThenInclude(x => x.Question)
                                .FirstOrDefaultAsync(x => x.Id == id);
            if (extraPractice != null)
            {
                var correctTotal = extraPractice.ExtraPracticeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                IList<ExtraPracticeExerciseResult> extraPracticeExerciseResults = extraPractice.ExtraPracticeExercises.OrderBy(x => x.CreatedDate)
                   .Select((x, index) => new ExtraPracticeExerciseResult
                   {
                       CorrectTotal = x.Exercise!.ExerciseQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                       CourseSkill = x.Exercise.CourseSkill,
                       ExtraPracticeExerciseId = x.Id,
                       StudentId = studentId,
                       Status = index == 0 ? EnumResultStatus.New : EnumResultStatus.Unfinished
                   }).ToList();
                return (correctTotal, extraPracticeExerciseResults);
            }
            return (0, null);
        }

        private async Task<(int, IList<ExtraPracticeExerciseResult>?)> GetCorrectTotalExercise(Guid id)
        {
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x!.ExerciseQuestions)
                                .ThenInclude(x => x.Question)
                                .FirstOrDefaultAsync(x => x.Id == id);
            if (extraPractice != null)
            {
                var correctTotal = extraPractice.ExtraPracticeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                return (correctTotal, null);
            }
            return (0, null);
        }

        private async Task<(int, IList<ExtraPracticeExerciseResult>?)> GetCorrectTotalMockTest(Guid id)
        {
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x!.ExerciseQuestions)
                                .ThenInclude(x => x.Question)
                                .FirstOrDefaultAsync(x => x.Id == id);
            if (extraPractice != null)
            {
                var correctTotal = extraPractice.ExtraPracticeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                return (correctTotal, null);
            }
            return (0, null);
        }

        private async Task<(int, IList<ExtraPracticeExerciseResult>?)> GetCorrectTotalInteractiveVideo(Guid id)
        {
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.Video)
                                .ThenInclude(x => x!.VideoTimeCodes)
                                .ThenInclude(x => x.TimeCodeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x!.ExerciseQuestions)
                                .ThenInclude(x => x.Question)
                                .FirstOrDefaultAsync(x => x.Id == id);
            if (extraPractice != null)
            {
                var correctTotal = extraPractice.ExtraPracticeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                return (correctTotal, null);
            }
            return (0, null);
        }

        private async Task<(int, IList<ExtraPracticeExerciseResult>?)> GetCorrectTotalBook(Guid id, Guid studentId)
        {
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeChapters)
                                .ThenInclude(x => x.ExtraPracticeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x!.ExerciseQuestions)
                                .ThenInclude(x => x.Question)
                                .FirstOrDefaultAsync(x => x.Id == id);
            if (extraPractice != null)
            {
                var correctTotal = extraPractice.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises).OrderBy(x => x.CreatedDate)
                                                                        .Select(x => x.Exercise)
                                                                        .SelectMany(x => x!.ExerciseQuestions)
                                                                        .Select(x => x.Question)
                                                                        .Sum(x => x!.CorrectTotal);
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
                return (correctTotal, extraPracticeExerciseResults);
            }
            return (0, null);
        }
    }
}
