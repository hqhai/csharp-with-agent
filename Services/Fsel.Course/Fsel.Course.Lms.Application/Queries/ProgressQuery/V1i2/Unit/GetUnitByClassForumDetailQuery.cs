// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2.Unit
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByClassForumDetailQuery : IRequest<MethodResult<IList<ClassForumScoreModel>>>
    {
        public Guid ClassForumResultId { get; set; }
    }

    public class GetUnitByClassForumDetailQueryHandler : IRequestHandler<GetUnitByClassForumDetailQuery, MethodResult<IList<ClassForumScoreModel>>>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMapper _mapper;

        public GetUnitByClassForumDetailQueryHandler(IClassForumResultRepository classForumResultRepository,
            IMapper mapper)
        {
            _classForumResultRepository = classForumResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<ClassForumScoreModel>>> Handle(GetUnitByClassForumDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassForumScoreModel>> methodResult = new MethodResult<IList<ClassForumScoreModel>>();

            var classForumResult = await _classForumResultRepository.ReadQueryable
                                                     .Include(x => x.ClassForumScores)
                                                     .FirstOrDefaultAsync(x => x.Id == request.ClassForumResultId, cancellationToken);
            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumResult));
                return methodResult;
            }
            var classForumScores = classForumResult.ClassForumScores.OrderBy(x => x.Criteria).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<IList<ClassForumScoreModel>>(classForumScores ?? default);
            return methodResult;
        }
    }
}
