// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
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
    using Fsel.Course.Domain.Models.CommandModels.ClassForumResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ApproveClassForumPenddingCommand : ApproveClassForumPenddingCommandModel, IRequest<MethodResult<ClassForumResultModel>>
    {
    }

    public class ApproveClassForumPenddingCommandHandler : IRequestHandler<ApproveClassForumPenddingCommand, MethodResult<ClassForumResultModel>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private const float Achieved_Point = 1; // Nhiệm vụ làm 1 lần nên achievepoint = 1

        public ApproveClassForumPenddingCommandHandler(IClassForumResultRepository classForumResultRepository, IMapper mapper, AuthContext authContext, IUserService userService, QuestBoardPublisher questBoardPublisher)
        {
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _questBoardPublisher = questBoardPublisher;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(ApproveClassForumPenddingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ClassForumResultModel>();

            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumResultFiles)
                                                                    .Include(x => x.ClassForum)
                                                                    .Include(x => x.ClassForumScores)
                                                                    .Where(e => e.Id == request.ClassForumResultId)
                                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }
            var csoResults = await _userService.GetCSOByUserId(_authContext.CurrentUserId);
            var csoId = csoResults.Content?.Result?.Id;

            if (classForumResult.Status != EnumClassForumResultStatus.Pending)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultStatusNotPendding));
                return methodResult;
            }
            //if (classForumResult.CheckCsoId != csoId)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.CsoInvalid), nameof(classForumResult.CheckCsoId));
            //    return methodResult;
            //}
            await _classForumResultRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.IsApprove)
                {
                    if (classForumResult.ClassForum?.GradingStyle == EnumGradingStyle.Autodot)
                    {
                        long score = 0;
                        if ((classForumResult.ClassForum?.CourseSkill == EnumCourseSkill.Writing && classForumResult.ClassForum?.TaggetWordLimit <= request.WordLimit) || (classForumResult.ClassForum?.CourseSkill == EnumCourseSkill.Speaking && classForumResult.ClassForum?.TaggetTimeLimit <= request.TimeLimit))
                        {
                            score = 9;
                        }

                        var enumClassForumScores = Enum.GetValues(typeof(EnumClassForumScoreCriteria)).Cast<EnumClassForumScoreCriteria>().ToList();
                        classForumResult.ClassForumScores = enumClassForumScores.Select(x => new ClassForumScore
                        {
                            ClassForumResultId = classForumResult.Id,
                            Score = score,
                            Criteria = x,
                        }).ToList();
                        classForumResult.Status = EnumClassForumResultStatus.Graded;
                    }
                    else
                    {
                        classForumResult.Status = EnumClassForumResultStatus.PendingForGrading;
                    }

                    var csoResults = await _userService.GetCSOByUserId(_authContext.CurrentUserId);
                    var csoId = csoResults.Content?.Result?.Id;
                    classForumResult.CheckCsoId = csoId;

                    var courseId = classForumResult.LessonResult?.CourseId;
                    if (classForumResult != null && courseId != null)
                    {
                        await DoQuestBoard(classForumResult.Id, (Guid)courseId, classForumResult.CreatedUserId, cancellationToken);
                    }
                }
                else
                {
                    classForumResult.Status = EnumClassForumResultStatus.Denied;
                    classForumResult.CheckStartDate = null;
                    classForumResult.CheckCsoId = null;
                }

                _classForumResultRepository.Update(classForumResult);
                await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassForumResultModel>(classForumResult);
                return methodResult;
            });
            return methodResult;
        }


        public async Task DoQuestBoard(Guid classForumResultId, Guid courseId, Guid userId, CancellationToken cancellationToken)
        {
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.CommentOnOtherPost };
            var student = await _userService.GetStudentByUserIdAsync(userId);
            var studentId = student?.Content?.Result?.Id;

            var hasFirstClassForumPost = _classForumResultRepository.Queryable.Any(c => c.CreatedUserId == userId && c.Status != EnumClassForumResultStatus.Pending && c.Status != EnumClassForumResultStatus.Draft && c.Status != EnumClassForumResultStatus.Denied);

            if (!hasFirstClassForumPost)
            {
                await _questBoardPublisher.Publish(new QuestBoardQueueModel
                {
                    StudentId = (Guid)studentId!,
                    Categories = categories,
                    AchievedPoint = Achieved_Point,
                    ObjectId = classForumResultId,
                    CourseId = courseId
                }, cancellationToken);
            }
        }
    }
}
