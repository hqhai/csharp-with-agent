// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
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
        private readonly IMapper _mapper;

        public GetClassForumByLessonQueryHandler(IClassForumRepository classForumRepository, IMapper mapper)
        {
            _mapper = mapper;
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

            methodResult.Result = _mapper.Map<ClassForumModel>(classForum);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
