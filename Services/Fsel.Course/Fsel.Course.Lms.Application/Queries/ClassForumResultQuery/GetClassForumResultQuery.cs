// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassForumResultQuery : IRequest<MethodResult<ClassForumResultModel>>
    {
        public Guid ClassForumResultId { get; set; }
    }

    public class GetClassForumResultQueryHandler : IRequestHandler<GetClassForumResultQuery, MethodResult<ClassForumResultModel>>
    {
        private readonly IClassForumRepository _classForumRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public GetClassForumResultQueryHandler(IClassForumRepository classForumRepository, IUserService userService, AuthContext authContext, IClassForumResultRepository classForumResultRepository)
        {
            _classForumRepository = classForumRepository;
            _userService = userService;
            _authContext = authContext;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(GetClassForumResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ClassForumResultModel>();

            var classForumResult = await _classForumResultRepository.Queryable
                .Include(x => x.ClassForumResultFiles)
                .Include(x => x.ClassForumScores)
                .Where(x => x.Id == request.ClassForumResultId).FirstOrDefaultAsync(cancellationToken);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumErrorCode.ClassForumNull));
                return methodResult;
            }
            var classForumResultModel = new ClassForumResultModel
            {
                Id = classForumResult.Id,
                Content = classForumResult.Content,
                Status = classForumResult.Status,
                ClassForumResultFiles = classForumResult.ClassForumResultFiles == null ? null : classForumResult.ClassForumResultFiles.Select(x => new ClassForumResultFileModel
                {
                    Id = x.Id,
                    FilePath = x.FilePath,
                }).ToList(),
                ClassForumScores = classForumResult.ClassForumScores == null ? null : classForumResult.ClassForumScores.Select(x => new ClassForumScoreModel
                {
                    Id = x.Id,
                    Feedback = x.Feedback,
                    Criteria = x.Criteria,
                    Score = x.Score
                }).ToList(),
            };

            methodResult.Result = classForumResultModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
