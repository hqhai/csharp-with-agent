// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
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
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetExerciseByExtraPracticeQuery : IRequest<MethodResult<IList<ExtraPracticeExerciseModel>>>
    {
        public Guid ExtraPracticeId { get; set; }
        public Guid? ExtraPracticeChapterId { get; set; }
    }

    public class GetExerciseByExtraPracticeQueryHandler : IRequestHandler<GetExerciseByExtraPracticeQuery, MethodResult<IList<ExtraPracticeExerciseModel>>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IUserService _userService;
        private readonly IExtraPracticeExerciseRepository _extraPracticeExerciseRepository;
        private readonly AuthContext _authContext;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;

        public GetExerciseByExtraPracticeQueryHandler(IExtraPracticeRepository extraPracticeRepository
            , IUserService userService
            , IExtraPracticeExerciseRepository extraPracticeExerciseRepository
            , AuthContext authContext
            , IExtraPracticeResultRepository extraPracticeResultRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _userService = userService;
            _extraPracticeExerciseRepository = extraPracticeExerciseRepository;
            _authContext = authContext;
            _extraPracticeResultRepository = extraPracticeResultRepository;
        }

        public async Task<MethodResult<IList<ExtraPracticeExerciseModel>>> Handle(GetExerciseByExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ExtraPracticeExerciseModel>>();
            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var extraPractice = await _extraPracticeRepository.GetByIdAsync(request.ExtraPracticeId);
            IList<ExtraPracticeExerciseModel> extraPracticeExerciseModels = await SwitchExtraPractice(extraPractice, studentId, request.ExtraPracticeChapterId);
            methodResult.Result = extraPracticeExerciseModels;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<IList<ExtraPracticeExerciseModel>> GetExtraPracticeExerciseTypeBook(Guid extraPracticeId, Guid? extraPracticeChapterId, Guid extraPracticeResultId)
        {
            var extraPracticeExerciseModels = new List<ExtraPracticeExerciseModel>();
            var extraPracticeExercises = await _extraPracticeExerciseRepository.Queryable.Include(x => x.Exercise)
                                     .Include(i => i.ExtraPracticeExerciseResults)
                                     .Where(x => x.ExtraPracticeChapterId == extraPracticeChapterId)
                                     .AsNoTracking()
                                     .ToListAsync();
            if (extraPracticeExercises != null && extraPracticeExercises.Count > 0)
            {
                extraPracticeExerciseModels = extraPracticeExercises.OrderBy(x => x.CreatedDate).Select(x => new ExtraPracticeExerciseModel
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    TotalCount = x.Exercise!.ExerciseQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                    Exercise = new ExerciseModel
                    {
                        Id = x.Exercise!.Id,
                        Name = x.Exercise.Name,
                        MediaPost = x.Exercise.MediaPost,
                        CourseSkill = x.Exercise.CourseSkill,
                    },
                    ExtraPracticeExerciseResult = x.ExtraPracticeExerciseResults.Where(y => y.ExtraPracticeResultId == extraPracticeResultId && y.ExtraPracticeExerciseId == x.Id)
                    .Select(x => new ExtraPracticeExerciseResultModel
                    {
                        Id = x.Id,
                        CorrectCount = x.CorrectCount,
                        CorrectTotal = x.CorrectTotal,
                        CourseSkill = x.CourseSkill,
                        ExecuteCount = x.ExecuteCount,
                        Percent = x.Percent,
                        Status = x.Status,
                        StudentId = x.StudentId,
                        ExtraPracticeExerciseId = x.ExtraPracticeExerciseId,
                    }).FirstOrDefault()
                }).ToList();
            }
            return extraPracticeExerciseModels;
        }

        public async Task<IList<ExtraPracticeExerciseModel>> GetExtraPracticeExerciseTypeVideoEmbed(Guid extraPracticeId, Guid extraPracticeResultId)
        {
            var extraPracticeExerciseModels = new List<ExtraPracticeExerciseModel>();
            var extraPracticeExercises = await _extraPracticeExerciseRepository.Queryable.Include(x => x.Exercise)
                                     .Include(i => i.ExtraPracticeExerciseResults)
                                     .Where(x => x.ExtraPracticeId == extraPracticeId)
                                     .AsNoTracking()
                                     .ToListAsync();
            if (extraPracticeExercises != null && extraPracticeExercises.Count > 0)
            {
                extraPracticeExerciseModels = extraPracticeExercises.Select(x => new ExtraPracticeExerciseModel
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    TotalCount = x.Exercise!.ExerciseQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                    Exercise = new ExerciseModel
                    {
                        Id = x.Exercise!.Id,
                        Name = x.Exercise.Name,
                        MediaPost = x.Exercise.MediaPost,
                        CourseSkill = x.Exercise.CourseSkill,
                    },
                    ExtraPracticeExerciseResult = x.ExtraPracticeExerciseResults.Where(y => y.ExtraPracticeResultId == extraPracticeResultId && y.ExtraPracticeExerciseId == x.Id)
                    .Select(x => new ExtraPracticeExerciseResultModel
                    {
                        Id = x.Id,
                        CorrectCount = x.CorrectCount,
                        CorrectTotal = x.CorrectTotal,
                        CourseSkill = x.CourseSkill,
                        ExecuteCount = x.ExecuteCount,
                        Percent = x.Percent,
                        Status = x.Status,
                        StudentId = x.StudentId,
                        ExtraPracticeExerciseId = x.ExtraPracticeExerciseId,
                    }).FirstOrDefault()
                }).ToList();
            }
            return extraPracticeExerciseModels;
        }

        public async Task<IList<ExtraPracticeExerciseModel>> SwitchExtraPractice(ExtraPractice? extraPractice, Guid studentId, Guid? extraPracticeChapterId)
        {
            ArgumentNullException.ThrowIfNull(extraPractice);
            IList<ExtraPracticeExerciseModel> extraPracticeExerciseModels = new List<ExtraPracticeExerciseModel>();
            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ExtraPracticeId == extraPractice.Id);
            if (extraPracticeResult != null)
            {
                switch (extraPractice.Type)
                {
                    case EnumExtraPracticeType.Book:
                        if (extraPracticeChapterId != null)
                        {
                            extraPracticeExerciseModels = await GetExtraPracticeExerciseTypeBook(extraPractice.Id, extraPracticeChapterId, extraPracticeResult.Id);
                        }
                        break;

                    case EnumExtraPracticeType.VideoEmbed:
                        extraPracticeExerciseModels = await GetExtraPracticeExerciseTypeVideoEmbed(extraPractice.Id, extraPracticeResult.Id);
                        break;

                    default:
                        break;
                }
            }

            return extraPracticeExerciseModels;
        }
    }
}
