// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2.Unit
{
    using System;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByClassForumDtoQuery : IRequest<MethodResult<IList<ClassForumAIModel>>>
    {
        public Guid LessonResultId { get; set; }
        public Guid ClassForumId { get; set; }
    }

    public class GetUnitByClassForumDtoQueryHandler : IRequestHandler<GetUnitByClassForumDtoQuery, MethodResult<IList<ClassForumAIModel>>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;

        public GetUnitByClassForumDtoQueryHandler(IClassForumResultRepository classForumResultRepository)
        {
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<IList<ClassForumAIModel>>> Handle(GetUnitByClassForumDtoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ClassForumAIModel>>();
            var classForumResult = await _classForumResultRepository.ReadQueryable.Where(x => x.LessonResultId == request.LessonResultId)
                                                                    .Where(x => x.ClassForumId == request.ClassForumId)
                                                                    .FirstOrDefaultAsync(cancellationToken);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }

            if (string.IsNullOrEmpty(classForumResult.GradingAlFeedback))
            {
                methodResult.Result = new List<ClassForumAIModel>();
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            methodResult.Result = classForumResult.GradingAlFeedback.Deserialize<List<ClassForumAIModel>>();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
