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
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
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

        public ApproveClassForumPenddingCommandHandler(IClassForumResultRepository classForumResultRepository, IMapper mapper, AuthContext authContext, IUserService userService)
        {
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
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
    }
}
