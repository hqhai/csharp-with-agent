namespace Fsel.Interaction.Application.Commands.CommentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ToolUpdateCourseIdCommand : IRequest<MethodResult<bool>>
    {
        public bool IsComment { get; set; }
    }

    public class ToolUpdateCourseIdCommandHandler : IRequestHandler<ToolUpdateCourseIdCommand, MethodResult<bool>>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ICourseService _courseService;
        private readonly IInteractionActionRepository _interactionActionRepository;

        public ToolUpdateCourseIdCommandHandler(ICommentRepository commentRepository, ICourseService courseService, IInteractionActionRepository interactionActionRepository)
        {
            _commentRepository = commentRepository;
            _courseService = courseService;
            _interactionActionRepository = interactionActionRepository;
        }

        public async Task<MethodResult<bool>> Handle(ToolUpdateCourseIdCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var results = new List<CourseClassForumResultModel>();

            if (request.IsComment)
            {
                var query = _commentRepository.Queryable.Where(p => p.CourseId == null && (p.Type == EnumInteractionType.ClassForum || p.Type == EnumInteractionType.ReplyComment));

                var comments = query.Where(p => p.Type == EnumInteractionType.ClassForum);
                var replyComments = query.Where(p => p.Type == EnumInteractionType.ReplyComment);

                var objectIds = comments.Select(p => p.ObjectId).ToList();

                if (objectIds.Any())
                {
                    var chunks = ChunkBy(objectIds, 10000);

                    foreach (var chunk in chunks)
                    {
                        var courseResults = await _courseService.GetCourseIds(new GetCourseIdsByClassForumResultIdsModel()
                        {
                            ClassForumResultIds = chunk
                        });
                        if (!courseResults.IsSuccessStatusCode || courseResults.Content?.Result == null)
                        {
                            methodResult.AddError(courseResults.Error);
                            return methodResult;
                        }

                        results.AddRange(courseResults.Content.Result);
                    }

                    var list = new List<Comment>();

                    comments.ForEach(p =>
                    {
                        var result = results?.FirstOrDefault(x => x.ClassForumResultId == p.ObjectId);
                        if (result != null)
                        {
                            p.CourseId = result.CourseId;
                            list.Add(p);
                        }
                    });

                    replyComments.ForEach(p =>
                    {
                        var result = comments?.FirstOrDefault(x => x.Id == p.ObjectId);
                        if (result != null)
                        {
                            p.CourseId = result.CourseId;
                            list.Add(p);
                        }
                    });

                    await _commentRepository.ExecuteTransactionAsync(async () =>
                    {
                        await _commentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        methodResult.StatusCode = StatusCodes.Status200OK;
                        methodResult.Result = true;
                        return methodResult;
                    });
                }
            }
            else
            {
                var query = _commentRepository.Queryable.Where(p => (p.Type == EnumInteractionType.ClassForum || p.Type == EnumInteractionType.ReplyComment));

                var queryAction = _interactionActionRepository.Queryable.Where(p => p.CourseId == null && (p.BusinessType == EnumInteractionType.ClassForum || p.BusinessType == EnumInteractionType.ReplyComment));

                var likeComments = queryAction.Where(p => p.BusinessType == EnumInteractionType.ClassForum);
                var likeReplyComments = queryAction.Where(p => p.BusinessType == EnumInteractionType.ReplyComment);

                var objectIds = likeComments.Select(p => p.ObjectId).ToList();

                if (objectIds.Any())
                {
                    var chunks = ChunkBy(objectIds, 10000);

                    foreach (var chunk in chunks)
                    {
                        var courseResults = await _courseService.GetCourseIds(new GetCourseIdsByClassForumResultIdsModel()
                        {
                            ClassForumResultIds = chunk
                        });
                        if (!courseResults.IsSuccessStatusCode || courseResults.Content?.Result == null)
                        {
                            methodResult.AddError(courseResults.Error);
                            return methodResult;
                        }

                        results.AddRange(courseResults.Content.Result);
                    }

                    var list = new List<InteractionAction>();

                    likeComments.ForEach(p =>
                    {
                        var result = results?.FirstOrDefault(x => x.ClassForumResultId == p.ObjectId);
                        if (result != null)
                        {
                            p.CourseId = result.CourseId;
                            list.Add(p);
                        }
                        else
                        {
                            var comment = query.FirstOrDefault(x => x.Id == p.ObjectId);
                            if (comment != null)
                            {
                                p.CourseId = comment.CourseId;
                                list.Add(p);
                            }
                        }
                    });

                    likeReplyComments.ForEach(p =>
                    {
                        var result = likeComments?.FirstOrDefault(x => x.Id == p.ObjectId);
                        if (result != null)
                        {
                            p.CourseId = result.CourseId;
                            list.Add(p);
                        }
                        else
                        {
                            var comment = query.FirstOrDefault(x => x.Id == p.ObjectId);
                            if (comment != null)
                            {
                                p.CourseId = comment.CourseId;
                                list.Add(p);
                            }
                        }
                    });

                    await _interactionActionRepository.ExecuteTransactionAsync(async () =>
                    {
                        await _interactionActionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        methodResult.StatusCode = StatusCodes.Status200OK;
                        methodResult.Result = true;
                        return methodResult;
                    });
                }
            }

            return methodResult;
        }

        private static List<List<T>> ChunkBy<T>(List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(g => g.Select(x => x.Value).ToList())
                .ToList();
        }
    }
}
