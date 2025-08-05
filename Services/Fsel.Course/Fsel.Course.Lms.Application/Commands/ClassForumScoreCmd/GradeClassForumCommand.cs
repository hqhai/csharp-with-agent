// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumScoreCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.ClassForumScores;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GradeClassForumCommand : GradeClassForumCommandModel, IRequest<MethodResult<List<ClassForumScoreModel>>>
    {
    }

    public class GradeClassForumCommandHandler : IRequestHandler<GradeClassForumCommand, MethodResult<List<ClassForumScoreModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IClassForumScoreRepository _classForumScoreRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public GradeClassForumCommandHandler(IMapper mapper,
            IClassForumScoreRepository classForumScoreRepository,
            IUserService userService,
            AuthContext authContext,
            IClassForumResultRepository classForumResultRepository)
        {
            _mapper = mapper;
            _classForumScoreRepository = classForumScoreRepository;
            _userService = userService;
            _authContext = authContext;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<List<ClassForumScoreModel>>> Handle(GradeClassForumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<ClassForumScoreModel>> methodResult = new MethodResult<List<ClassForumScoreModel>>();

            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            if (!teacherResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var teacherId = teacherResult.Content?.Result?.Id;
            List<ClassForumScore> classForumScores = new List<ClassForumScore>();
            if (request.ClassForumScores == null || request.ClassForumScores.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ClassForumScores));
                return methodResult;
            }
            var isClassForumResult = await _classForumResultRepository.AnyAsync(request.ClassForumResultId);
            if (!isClassForumResult)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isClassForumResult));
                return methodResult;
            }

            var listClassForumScore = request.ClassForumScores.Select(x => new
            {
                x.Score,
                x.Criteria,
                x.Feedback,
                Guid = x.ClassForumResultId = request.ClassForumResultId
            }).Distinct().ToList();
            foreach (var item in request.ClassForumScores)
            {
                if (item.Score > 9 || item.Score == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumClassForumScoreErrorCode.ScoreMustLessThan9), nameof(item.Score), new
                    {
                        Score = item.Score,
                    });
                    return methodResult;
                }
                ClassForumScore classForumScoreNew = _mapper.Map<ClassForumScore>(item);

                if (!classForumScoreNew.IsValid())
                {
                    methodResult.AddErrorBadRequest(classForumScoreNew.ErrorMessages);
                    return methodResult;
                }
                classForumScores.Add(classForumScoreNew);
            }
            var classForumResult = await _classForumResultRepository.Queryable.Where(e => e.Id == request.ClassForumResultId).FirstOrDefaultAsync(cancellationToken);
            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }

            if (classForumResult.GradingTeacherId != teacherId)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.TeacherInvalid), nameof(classForumResult.GradingTeacherId));
                return methodResult;
            }
            if (classForumResult.Status != EnumClassForumResultStatus.PendingForGrading)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultStatusNotPendingForGrading));
                return methodResult;
            }
            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                classForumResult.Status = EnumClassForumResultStatus.Graded;
                classForumResult.ClassForumScores = classForumScores;
                await _classForumResultRepository.BulkUpdateList(new List<ClassForumResult> { classForumResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.LessonResultId, c.ClassForumId };
                });
                await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<List<ClassForumScoreModel>>(classForumScores);
                return methodResult;
            });

            return methodResult;
        }
    }
}
