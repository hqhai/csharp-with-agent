// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByClassForumDetailQuery : IRequest<MethodResult<IList<ClassForumScoreModel>>>
    {
        public Guid ClassForumId { get; set; }
    }

    public class GetUnitByClassForumDetailQueryHandler : IRequestHandler<GetUnitByClassForumDetailQuery, MethodResult<IList<ClassForumScoreModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetUnitByClassForumDetailQueryHandler(AuthContext authContext
            , IClassForumRepository classForumRepository
            , IMapper mapper
            , IUserService userService)
        {
            _authContext = authContext;
            _classForumRepository = classForumRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<IList<ClassForumScoreModel>>> Handle(GetUnitByClassForumDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassForumScoreModel>> methodResult = new MethodResult<IList<ClassForumScoreModel>>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;

            var classForum = await _classForumRepository.ReadQueryable.Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                                     .ThenInclude(x => x.ClassForumScores)
                                                     .FirstOrDefaultAsync(x => x.Id == request.ClassForumId, cancellationToken);
            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }
            var classForumScores = classForum.ClassForumResults.FirstOrDefault()?.ClassForumScores.OrderBy(x => x.Criteria).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<IList<ClassForumScoreModel>>(classForumScores ?? default);
            return methodResult;
        }
    }
}
