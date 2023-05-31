// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumScoreQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListClassForumScoresQuery : IRequest<MethodResult<IList<ClassForumScoreModel>>>
    {
        public Guid ClassForumResultId { get; set; }
    }

    public class GetListClassForumScoresQueryHandler : IRequestHandler<GetListClassForumScoresQuery, MethodResult<IList<ClassForumScoreModel>>>
    {
        private readonly IClassForumScoreRepository _classForumScoreRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMapper _mapper;

        public GetListClassForumScoresQueryHandler(IClassForumScoreRepository classForumScoreRepository
            , IMapper mapper
            , IClassForumResultRepository classForumResultRepository)
        {
            _classForumScoreRepository = classForumScoreRepository;
            _mapper = mapper;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<IList<ClassForumScoreModel>>> Handle(GetListClassForumScoresQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassForumScoreModel>> methodResult = new MethodResult<IList<ClassForumScoreModel>>();
            var isClassForumResult = await _classForumResultRepository.AnyAsync(request.ClassForumResultId);
            if (!isClassForumResult)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultNotExist));
                return methodResult;
            }
            var classForumScores = await _classForumScoreRepository.Queryable
                                            .Where(x => x.ClassForumResultId == request.ClassForumResultId)
                                            .ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<ClassForumScoreModel>>(classForumScores);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
