// Copyright (classForumResults) Atlantic. All rights reserved.

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
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
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
        private const double Standard_Ratio = 1; // tỉ lệ xem đánh giá 100/100

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

            var isClassForumResult = await _classForumResultRepository.AnyAsync(request.ClassForumResultId);
            if (!isClassForumResult)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isClassForumResult));
                return methodResult;
            }

            var classForumResult = await _classForumResultRepository.GetByIdAsync(request.ClassForumResultId);
            if (classForumResult != null && !classForumResult.IsViewed)
            {
                classForumResult.IsViewed = true;
                await _classForumResultRepository.BulkUpdateList(new List<ClassForumResult> { classForumResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.ClassForumId, c.StudentId, c.LessonResultId };
                });
            }

            var classForumScores = await _classForumScoreRepository.Queryable
                                            .Where(x => x.ClassForumResultId == request.ClassForumResultId)
                                            .ToListAsync(cancellationToken);
            var courseId = classForumResult?.LessonResult?.CourseId;

            methodResult.Result = _mapper.Map<IList<ClassForumScoreModel>>(classForumScores);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
