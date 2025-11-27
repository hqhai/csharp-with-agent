// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.FlagQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Models;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Application.Services.UserServices.Models;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.Flags;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchFlagQuery : SearchFlagQueryModel, IRequest<MethodResult<PagingItemsModel<FlagModel>>>
    {
    }

    public class SearchFlagQueryHandler : IRequestHandler<SearchFlagQuery, MethodResult<PagingItemsModel<FlagModel>>>
    {
        private readonly IFlagRepository _flagRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;

        public SearchFlagQueryHandler(IFlagRepository flagRepository, ICommentRepository commentRepository, ICourseService courseService, IUserService userService)
        {
            _flagRepository = flagRepository;
            _commentRepository = commentRepository;
            _courseService = courseService;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<FlagModel>>> Handle(SearchFlagQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<FlagModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var flagQuery = _flagRepository.Queryable
                              .Where(x => x.Status == EnumFlagStatus.New && x.Type == request.Type)
                              .Select(x => new FlagModel
                              {
                                  Id = x.Id,
                                  ObjectId = x.ObjectId,
                                  CreatedDate = x.CreatedDate,
                                  CreatedFullName = x.CreatedFullName,
                                  FeedBack = x.FeedBack,
                                  FlagIssue = x.FlagIssue,
                                  Status = x.Status,
                                  Type = x.Type,
                              });

            if (request.Type != null)
            {
                flagQuery = flagQuery.Where(m => m.Type == request.Type);
            }

            var comments = await _commentRepository.Queryable.Where(x => flagQuery.Select(x => x.ObjectId).Contains(x.Id)).ToListAsync(cancellationToken);

            int totalItem = await flagQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await flagQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var classForumResults = new List<ClassForumResultModel>();

            if (lists.Count > 0)
            {
                var classForumResultResult = await _courseService.ExecuteListClassForumResultQueryAsync(new BaseQueryModel
                {
                    Filters = new List<GenericFilterModel>
                    {
                        new GenericFilterModel
                        {
                            Property = nameof(ClassForumResultModel.Id),
                            Value = lists.Select(x => x.ObjectId).ToList(),
                            Operator = Common.Enums.EnumFilterOperator.In
                        }
                    }
                });
                classForumResults = classForumResultResult.Content?.Result?.ToList() ?? new List<ClassForumResultModel>();
            }

            var studentResult = await _userService.ExecuteListStudentQueryAsync(new BaseQueryModel
            {
                Filters = new List<GenericFilterModel>
                 {
                     new GenericFilterModel
                     {
                         Property = nameof(StudentModel.Id),
                         Value = classForumResults?.Select(x => x.StudentId).ToList(),
                         Operator = Common.Enums.EnumFilterOperator.In
                     }
                 }
            });
            var studentResults = studentResult.Content?.Result;

            foreach (var item in lists)
            {
                if (item.Type == EnumInteractionType.ReplyComment)
                {
                    var commentLevel2Id = _commentRepository.Queryable.Where(x => x.Id == item.ObjectId).Select(x => x.ObjectId).FirstOrDefault();
                    var commentLevel1Id = _commentRepository.Queryable.Where(x => x.Id == commentLevel2Id).FirstOrDefault()?.ObjectId;

                    var classForumResult = classForumResults?.Where(x => x.Id == (commentLevel1Id ?? commentLevel2Id)).FirstOrDefault();
                    item.Content = classForumResult?.Content;
                    item.CreatedUserName = classForumResult?.CreatedFullName;
                    item.UserId = classForumResult?.CreatedUserId;
                    item.StudentId = classForumResult?.StudentId;
                    item.AvatarPath = studentResults?.Where(x => x.Id == item.StudentId).FirstOrDefault()?.AvatarPath;
                    item.ClassForumResultId = classForumResult?.Id;
                }
                else if (item.Type == EnumInteractionType.ClassForum)
                {
                    var classForumResult = classForumResults?.Where(x => x.Id == item.ObjectId).FirstOrDefault();
                    item.Content = classForumResult?.Content;
                    item.CreatedUserName = classForumResult?.CreatedFullName;
                    item.UserId = classForumResult?.CreatedUserId;
                    item.StudentId = classForumResult?.StudentId;
                    item.AvatarPath = studentResults?.Where(x => x.Id == item.StudentId).FirstOrDefault().AvatarPath;
                    item.ClassForumResultId = classForumResult?.Id;
                }
            }
            methodResult.Result = new PagingItemsModel<FlagModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
