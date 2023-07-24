// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
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

    public class GetExerciseByExtraPracticeQuery : IRequest<MethodResult<IList<ExtraPracticeExerciseModel>>>
    {
        public Guid ExtraPracticeId { get; set; }
        public Guid? ExtraPracticeChapterId { get; set; }
    }

    public class GetExerciseByExtraPracticeQueryHandler : IRequestHandler<GetExerciseByExtraPracticeQuery, MethodResult<IList<ExtraPracticeExerciseModel>>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IExtraPracticeExerciseRepository _extraPracticeExerciseRepository;
        private readonly AuthContext _authContext;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;

        public GetExerciseByExtraPracticeQueryHandler(IExtraPracticeRepository extraPracticeRepository
            , IUserService userService
            , IMapper mapper
            , IExtraPracticeExerciseRepository extraPracticeExerciseRepository
            , AuthContext authContext
            , IExtraPracticeResultRepository extraPracticeResultRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _userService = userService;
            _mapper = mapper;
            _extraPracticeExerciseRepository = extraPracticeExerciseRepository;
            _authContext = authContext;
            _extraPracticeResultRepository = extraPracticeResultRepository;
        }

        public async Task<MethodResult<IList<ExtraPracticeExerciseModel>>> Handle(GetExerciseByExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ExtraPracticeExerciseModel>>();
            IList<ExtraPracticeExerciseModel> extraPracticeExerciseModels = new List<ExtraPracticeExerciseModel>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var extraPractice = await _extraPracticeRepository.GetByIdAsync(request.ExtraPracticeId);
            extraPracticeExerciseModels = await SwitchExtraPractice(extraPractice, studentId, request.ExtraPracticeChapterId);
            methodResult.Result = extraPracticeExerciseModels;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<IList<ExtraPracticeExerciseModel>> GetExtraPraticeExerciseTypeBook(Guid extraPracticeId, Guid? extraPracticeChapterId, Guid extraPracticeResultId)
        {
            var extraPracticeExerciseModels = new List<ExtraPracticeExerciseModel>();
            var extraPracticeExercises = await _extraPracticeExerciseRepository.Queryable.Include(x => x.Exercise)
                                     .Include(i => i.ExtraPracticeExerciseResults)
                                     .Where(x => x.ExtraPracticeChapterId == extraPracticeChapterId && x.ExtraPracticeId == extraPracticeId)
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
                        Percent = x.Percent,
                        Status = x.Status,
                        StudentId = x.StudentId,
                        ExtraPracticeExerciseId = x.ExtraPracticeExerciseId,
                    }).FirstOrDefault()
                }).ToList();
            }
            return extraPracticeExerciseModels;
        }

        public async Task<IList<ExtraPracticeExerciseModel>> GetExtraPraticeExerciseTypeVideoEmbed(Guid extraPracticeId, Guid extraPracticeResultId)
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
                        Percent = x.Percent,
                        Status = x.Status,
                        StudentId = x.StudentId,
                        ExtraPracticeExerciseId = x.ExtraPracticeExerciseId,
                    }).FirstOrDefault()
                }).ToList();
            }
            return extraPracticeExerciseModels;
        }

        public async Task<IList<ExtraPracticeExerciseModel>> GetExtraPraticeExerciseTypeExercise(Guid extraPracticeId, Guid extraPracticeResultId)
        {
            var extraPracticeExerciseModels = new List<ExtraPracticeExerciseModel>();
            var extraPracticeExercises = await _extraPracticeExerciseRepository.Queryable.Include(x => x.Exercise)
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
                        MediaPost = x.Exercise.MediaPost,
                        CourseSkill = x.Exercise.CourseSkill,
                    },
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
                            extraPracticeExerciseModels = await GetExtraPraticeExerciseTypeBook(extraPractice.Id, extraPracticeChapterId, extraPracticeResult.Id);
                        }
                        break;

                    case EnumExtraPracticeType.Exercise:
                        extraPracticeExerciseModels = await GetExtraPraticeExerciseTypeExercise(extraPractice.Id, extraPracticeResult.Id);
                        break;

                    case EnumExtraPracticeType.InteractiveVideo:
                        extraPracticeExerciseModels = await GetExtraPraticeExerciseTypeExercise(extraPractice.Id, extraPracticeResult.Id);
                        break;

                    case EnumExtraPracticeType.VideoEmbed:
                        extraPracticeExerciseModels = await GetExtraPraticeExerciseTypeExercise(extraPractice.Id, extraPracticeResult.Id);
                        break;

                    default:
                        break;
                }
            }

            return extraPracticeExerciseModels;
        }
    }
}
