// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassForumResultByLessonResultIdQuery : IRequest<MethodResult<ClassForumSearchModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetClassForumResultByLessonResultIdQueryHandler : IRequestHandler<GetClassForumResultByLessonResultIdQuery, MethodResult<ClassForumSearchModel>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMapper _mapper;

        public GetClassForumResultByLessonResultIdQueryHandler(IClassForumResultRepository classForumResultRepository, IMapper mapper)
        {
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassForumSearchModel>> Handle(GetClassForumResultByLessonResultIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumSearchModel> methodResult = new MethodResult<ClassForumSearchModel>();

            var classForumResult = await _classForumResultRepository.Queryable
                                                                    .Include(x => x.ClassForum)
                                                                    .ThenInclude(x => x.ClassForumFiles)
                                                                    .Include(x => x.ClassForumResultFiles)
                                                                    .Include(x => x.ClassForumDetailResults)
                                                                    .FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId, cancellationToken);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<ClassForumSearchModel>(classForumResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
