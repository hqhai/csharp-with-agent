// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumScoreQuery
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
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListClassForumScoresQuery : IRequest<MethodResult<IList<ClassForumScoreModel>>>
    {
        public Guid ClassForumResultId { get; set; }
    }

    public class GetListClassForumScoresQueryHandler : IRequestHandler<GetListClassForumScoresQuery, MethodResult<IList<ClassForumScoreModel>>>
    {
        private readonly IClassForumScoreRepository _classForumScoreRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly IUserService _userService;

        public GetListClassForumScoresQueryHandler(IClassForumScoreRepository classForumScoreRepository, IClassForumResultRepository classForumResultRepository, IMapper mapper, AuthContext authContext, QuestBoardPublisher questBoardPublisher, IUserService userService)
        {
            _classForumScoreRepository = classForumScoreRepository;
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
            _authContext = authContext;
            _questBoardPublisher = questBoardPublisher;
            _userService = userService;
        }

        public async Task<MethodResult<IList<ClassForumScoreModel>>> Handle(GetListClassForumScoresQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassForumScoreModel>> methodResult = new MethodResult<IList<ClassForumScoreModel>>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = student?.Content?.Result?.Id;

            var isClassForumResult = await _classForumResultRepository.AnyAsync(request.ClassForumResultId);
            if (!isClassForumResult)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isClassForumResult));
                return methodResult;
            }
            var classForumScores = await _classForumScoreRepository.Queryable
                                            .Where(x => x.ClassForumResultId == request.ClassForumResultId)
                                            .ToListAsync(cancellationToken);

            var classForumResult = await _classForumResultRepository.Queryable.Where(x => x.Id == request.ClassForumResultId).FirstOrDefaultAsync(cancellationToken);
            if (classForumResult?.IsViewed == false)
            {
                classForumResult!.IsViewed = true;

                _classForumResultRepository.Update(classForumResult);
                await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }

            var archievePoint = await _classForumResultRepository.Queryable
                            .Where(x => x.Id == request.ClassForumResultId && x.IsViewed && x.StudentId == studentId)
                            .Select(x => x.Id).ToListAsync(cancellationToken);
            var archievePointCount = archievePoint.Count;

            methodResult.Result = _mapper.Map<IList<ClassForumScoreModel>>(classForumScores);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task DoQuestBoard(CancellationToken cancellationToken)
        {
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.SeeFiveTeacherReview, EnumQuestBoardCategory.SeeTenTeacherReview, EnumQuestBoardCategory.SeeAllTeacherReview };
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = student?.Content?.Result?.Id;

            await _questBoardPublisher.Publish(new QuestBoardQueueModel
            {
                StudentId = (Guid)studentId!,
                Categories = categories,
                AchievedPoint = archievePointCount,
            }, cancellationToken);
        }
    }
}
