// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Models;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassForumResultFlagQuery : IRequest<MethodResult<ClassForumResultModel>>
    {
        public Guid ClassForumResultId { get; set; }
    }

    public class GetClassForumResultFlagQueryHandler : IRequestHandler<GetClassForumResultFlagQuery, MethodResult<ClassForumResultModel>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMapper _mapper;
        private readonly IInteractionService _interactionService;

        public GetClassForumResultFlagQueryHandler(IClassForumResultRepository classForumResultRepository, IMapper mapper, IInteractionService interactionService)
        {
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
            _interactionService = interactionService;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(GetClassForumResultFlagQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ClassForumResultModel>();

            var actionResult = await _interactionService.ExecuteListActionQueryAsync(new BaseQueryModel
            {
                Filters = new List<GenericFilterModel>
                {
                    new GenericFilterModel
                    {
                        Property = nameof(InteractionActionModel.ObjectId),
                        Value = request.ClassForumResultId,
                        Operator = Common.Enums.EnumFilterOperator.Equal
                    }
                }
            });
            var action = actionResult.Content?.Result;

            var classForumResult = await _classForumResultRepository.Queryable
                .Include(x => x.ClassForumResultFiles)
                .Where(x => x.Id == request.ClassForumResultId)
                .Select(x => new ClassForumResultModel
                {
                    Id = x.Id,
                    Status = x.Status,
                    ClassForumId = x.ClassForumId,
                    WordContent = x.WordContent,
                    ClassForumResultFiles = x.ClassForumResultFiles.Select(x => new ClassForumResultFileModel
                    {
                        FilePath = x.FilePath
                    }).ToList(),
                    Content = x.Content,
                    LessonResultId = x.LessonResultId,
                    LikeNumber = action!.LikeNumber,
                    CommentNumber = action.CommentNumber,
                }).FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = classForumResult;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
