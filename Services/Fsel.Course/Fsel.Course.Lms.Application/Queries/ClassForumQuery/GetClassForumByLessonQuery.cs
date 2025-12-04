// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassForumByLessonQuery : IRequest<MethodResult<ClassForumModel>>
    {
        public Guid LessonId { get; set; }
    }

    public class GetClassForumByLessonQueryHandler : IRequestHandler<GetClassForumByLessonQuery, MethodResult<ClassForumModel>>
    {
        private readonly IClassForumRepository _classForumRepository;
        private readonly IMapper _mapper;
        private readonly IInteractionService _interactionService;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetClassForumByLessonQueryHandler(IClassForumRepository classForumRepository, IMapper mapper, IInteractionService interactionService, IUserService userService, AuthContext authContext)
        {
            _classForumRepository = classForumRepository;
            _mapper = mapper;
            _interactionService = interactionService;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<ClassForumModel>> Handle(GetClassForumByLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumModel> methodResult = new MethodResult<ClassForumModel>();
            var classForumQuery = await _classForumRepository.Queryable
                                   .Include(x => x.Lesson)
                                   .Include(x => x.ClassForumFiles)
                                   .Include(x => x.ClassForumResults.Where(x => x.Status.HasValue).OrderBy(x => x.CreatedDate))
                                   .ThenInclude(x => x.ClassForumResultFiles)
                                   .FirstOrDefaultAsync(x => x.LessonId == request.LessonId, cancellationToken);
            var classForm = _mapper.Map<ClassForumModel>(classForumQuery);

            if (classForumQuery != null)
            {
                var actionsResult = await _interactionService.GetsActionAsync(new InteractionActionCommandModel { ObjectIds = classForumQuery.ClassForumResults.Select(x => x.Id).ToList(), UserId = _authContext.CurrentUserId });
                var actions = actionsResult.Content?.Result;

                var userResult = await _userService.GetUsersByUserIdsAsync(classForumQuery.ClassForumResults.Select(x => x.CreatedUserId).ToList());
                var users = userResult.Content?.Result;
                if (actions != null)
                {
                    foreach (var item in classForm?.ClassForumResults!)
                    {
                        item.CourseLevel = classForumQuery.Lesson?.CourseLevel ?? default;
                        var action = actions.FirstOrDefault(x => x.ObjectId == item.Id);
                        if (action != null)
                        {
                            item.CommentNumber = action.CommentNumber;
                            item.LikeNumber = action.LikeNumber;
                            item.IsLiked = action.IsLiked;
                        }
                        var user = users?.FirstOrDefault(x => x.Id == item.CreatedUserId);
                        item.CreatedFullName = user?.FullName ?? item.CreatedFullName;
                        item.AvatarPath = user?.AvatarPath;
                    }
                }
            }
            methodResult.Result = classForm;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
