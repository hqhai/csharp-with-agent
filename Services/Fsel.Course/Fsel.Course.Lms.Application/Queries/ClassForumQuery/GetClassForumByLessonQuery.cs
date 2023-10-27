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
                                   .Include(x => x.ClassForumFiles)
                                   .Include(x => x.ClassForumResults!.OrderBy(x => x.CreatedDate))
                                   .ThenInclude(x => x.ClassForumResultFiles)
                                   .Where(x => x.ClassForumResults.Any(x => x.Status == EnumClassForumResultStatus.PendingForGrading || x.Status == EnumClassForumResultStatus.Graded))
                                   .FirstOrDefaultAsync(x => x.LessonId == request.LessonId, cancellationToken);

            var classForm = _mapper.Map<ClassForumModel>(classForumQuery);

            if (classForumQuery != null)
            {
                var actionsResult = await _interactionService.GetsActionAsync(new InteractionActionCommandModel { ObjectIds = classForm?.ClassForumResults?.Select(x => x.Id).ToList(), UserId = _authContext.CurrentUserId });
                var actions = actionsResult.Content?.Result;

                var studentResult = await _userService.GetStudentsByStudentIdsAsync(classForumQuery!.ClassForumResults.Select(x => x.StudentId).ToList());
                var students = studentResult.Content?.Result;
                if (actions != null)
                {
                    foreach (var item in classForm?.ClassForumResults!)
                    {
                        var action = actions.FirstOrDefault(x => x.ObjectId == item.Id);
                        item.CommentNumber = action?.CommentNumber;
                        item.LikeNumber = action?.LikeNumber;
                        item.IsLiked = action?.IsLiked;
                        var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                        item.AvatarPath = student?.Human?.AvatarPath;
                        item.CourseLevel = student?.CourseLevel ?? default;
                    }
                }
            }

            methodResult.Result = classForm;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
