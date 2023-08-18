// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
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
        private readonly IMapper _mapper;

        public GetClassForumResultQueryHandler(IClassForumRepository classForumRepository, IUserService userService, AuthContext authContext, IClassForumResultRepository classForumResultRepository, IMapper mapper)
        {
            _classForumRepository = classForumRepository;
            _userService = userService;
            _authContext = authContext;
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassForumResultModel>> Handle(GetClassForumResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ClassForumResultModel>();

            var classForumResult = await _classForumResultRepository.Queryable
                .Include(x => x.ClassForum)
                .Include(x => x.ClassForumResultFiles)
                .Include(x => x.ClassForumScores)
                .Where(x => x.Id == request.ClassForumResultId)
                .Select(x => new ClassForumResultModel
                {
                    Id = x.Id,
                    Content = x.Content,
                    Status = x.Status,
                    ClassForumId = x.ClassForumId,
                    ClassForum = _mapper.Map<ClassForumModel>(x.ClassForum),
                    CourseCode = x.LessonResult!.Course!.Code,
                    LessonDisplayOrder = x.LessonResult!.Lesson!.UnitLessons.Where(y => y.UnitId == x.LessonResult.UnitId).Select(x => x.DisplayOrder).FirstOrDefault(),
                    UnitDisplayOrder = x.LessonResult.Unit!.CourseUnitMockTests.Where(y => y.CourseId == x.LessonResult.CourseId).Select(x => x.DisplayOrder).FirstOrDefault(),
                    CreatedDate = x.CreatedDate,
                    ClassForumResultFiles = x.ClassForumResultFiles == null ? null : x.ClassForumResultFiles.Select(x => new ClassForumResultFileModel
                    {
                        FilePath = x.FilePath,
                    }).ToList(),
                    ClassForumScores = x.ClassForumScores == null ? null : x.ClassForumScores.Select(x => new ClassForumScoreModel
                    {
                        Id = x.Id,
                        Feedback = x.Feedback,
                        Criteria = x.Criteria,
                        Score = x.Score
                    }).ToList(),
                })
                .FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = classForumResult;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
