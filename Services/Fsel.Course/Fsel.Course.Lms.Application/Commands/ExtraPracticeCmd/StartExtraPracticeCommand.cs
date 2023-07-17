// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ExtraPracticeCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
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
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticeNotExist));
                return methodResult;
            }

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ExtraPracticeId == request.ExtraPracticeId, cancellationToken);
            if (extraPracticeResult == null)
            {
                extraPractice.ExtraPracticeResults.Add(new ExtraPracticeResult
                {
                    StudentId = studentId ?? default,
                    CorrectTotal = await GetCorrectTotal(extraPractice)
                });
                _extraPracticeRepository.Update(extraPractice);
                await _extraPracticeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await _extraPracticeConverter.SwitchExtraPractice(extraPractice, studentId ?? default);
            return methodResult;
        }

        private async Task<int> GetCorrectTotal(ExtraPractice extraPractice)
        {
            int correctTotal = 0;
            switch (extraPractice.Type)
            {
                case EnumExtraPracticeType.VideoEmbed:
                    correctTotal = await GetCorrectTotalVideoEmbed(extraPractice.Id);
                    break;

                case EnumExtraPracticeType.MockTest:
                    correctTotal = await GetCorrectTotalMockTest(extraPractice.Id);
                    break;

                case EnumExtraPracticeType.Exercise:
                    correctTotal = await GetCorrectTotalVideoEmbed(extraPractice.Id);
                    break;

                case EnumExtraPracticeType.InteractiveVideo:
                    correctTotal = await GetCorrectTotalInteractiveVideo(extraPractice.Id);
                    break;

                case EnumExtraPracticeType.Book:
                    correctTotal = await GetCorrectTotalBook(extraPractice.Id);
                    break;

                case EnumExtraPracticeType.Articles:
                    correctTotal = 0;
                    break;
            }
            return correctTotal;
        }

        private async Task<int> GetCorrectTotalVideoEmbed(Guid id)
        {
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x!.ExerciseQuestions)
                                .ThenInclude(x => x.Question)
                                .FirstOrDefaultAsync(x => x.Id == id);
            if (extraPractice != null)
            {
                var correctCount = extraPractice.ExtraPracticeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                return correctCount;
            }
            return 0;
        }

        private async Task<int> GetCorrectTotalMockTest(Guid id)
        {
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x!.ExerciseQuestions)
                                .ThenInclude(x => x.Question)
                                .FirstOrDefaultAsync(x => x.Id == id);
            if (extraPractice != null)
            {
                var correctCount = extraPractice.ExtraPracticeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                return correctCount;
            }
            return 0;
        }

        private async Task<int> GetCorrectTotalInteractiveVideo(Guid id)
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
                var correctCount = extraPractice.ExtraPracticeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                return correctCount;
            }
            return 0;
        }

        private async Task<int> GetCorrectTotalBook(Guid id)
        {
            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeChapters)
                                .ThenInclude(x => x.ExtraPracticeExercises)
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x!.ExerciseQuestions)
                                .ThenInclude(x => x.Question)
                                .FirstOrDefaultAsync(x => x.Id == id);
            if (extraPractice != null)
            {
                var correctCount = extraPractice.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises)
                                                                        .Select(x => x.Exercise)
                                                                        .SelectMany(x => x!.ExerciseQuestions)
                                                                        .Select(x => x.Question)
                                                                        .Sum(x => x!.CorrectTotal);
                return correctCount;
            }
            return 0;
        }
    }
}
