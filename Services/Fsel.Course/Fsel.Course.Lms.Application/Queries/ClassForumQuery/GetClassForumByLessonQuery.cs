// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
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

        public GetClassForumByLessonQueryHandler(IClassForumRepository classForumRepository)
        {
            _classForumRepository = classForumRepository;
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
            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumErrorCode.ClassForumNull));
                return methodResult;
            }
            var classForumModel = new ClassForumModel
            {
                Id = classForum.Id,
                CourseSkill = classForum.CourseSkill,
                IsActive = classForum.IsActive,
                MediaPost = classForum.MediaPost,
                TaggetTimeLimit = classForum.TaggetTimeLimit,
                ClassForumFiles = classForum.ClassForumFiles == null ? null : classForum.ClassForumFiles.Select(x => new ClassForumFileModel
                {
                    Id = x.Id,
                    FilePath = x.FilePath,
                }).ToList(),
                ClassForumResults = classForum.ClassForumResults == null ? null : classForum.ClassForumResults.Select(x => new ClassForumResultModel
                {
                    Id = x.Id,
                    Content = x.Content,
                    Status = x.Status,
                    ClassForumResultFiles = x.ClassForumResultFiles == null ? null : x.ClassForumResultFiles.Select(x => new ClassForumResultFileModel
                    {
                        Id = x.Id,
                        FilePath = x.FilePath,
                    }).ToList(),
                }).ToList(),
            };
            methodResult.Result = classForumModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
