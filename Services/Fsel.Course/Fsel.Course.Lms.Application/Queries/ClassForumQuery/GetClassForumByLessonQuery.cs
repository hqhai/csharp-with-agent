// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
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

        public GetClassForumByLessonQueryHandler(IClassForumRepository classForumRepository, IMapper mapper, IInteractionService interactionService)
        {
            _classForumRepository = classForumRepository;
            _mapper = mapper;
            _interactionService = interactionService;
        }

        public async Task<MethodResult<ClassForumModel>> Handle(GetClassForumByLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumModel> methodResult = new MethodResult<ClassForumModel>();
            var classForum = await _classForumRepository.Queryable
                                   .Include(x => x.ClassForumResults!)
                                   .ThenInclude(x => x.ClassForumResultFiles)
                                   .Where(x => x.ClassForumResults!.Any(x => x.Status == EnumClassForumResultStatus.PendingForGrading || x.Status == EnumClassForumResultStatus.Graded))
                                   .FirstOrDefaultAsync(x => x.LessonId == request.LessonId, cancellationToken);

            var classFormQuery = _mapper.Map<ClassForumModel>(classForum);
            var actionsResult = await _interactionService.GetsActionAsync(new InteractionActionCommandModel { ObjectIds = classForum?.ClassForumResults.Select(x => x.Id).ToList() });
            var actions = actionsResult.Content?.Result;

            if (actions != null)
            {
                classFormQuery!.ClassForumResults = classFormQuery.ClassForumResults?.Where(x => !actions.Any(n => n.IsDisable && n.ObjectId == x.Id)).ToList();
                foreach (var item in classFormQuery.ClassForumResults!)
                {
                    var action = actions.FirstOrDefault(x => x.ObjectId == item.Id);
                    item.CommentNumber = action?.CommentNumber;
                    item.LikeNumber = action?.LikeNumber;
                    item.IsLiked = action?.IsLiked;
                }
            }

            methodResult.Result = classFormQuery;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
