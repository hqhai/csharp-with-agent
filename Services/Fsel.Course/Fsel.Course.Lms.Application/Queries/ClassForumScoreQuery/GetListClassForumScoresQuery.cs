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
        private readonly IMapper _mapper;

        public GetListClassForumScoresQueryHandler(IClassForumScoreRepository classForumScoreRepository, IMapper mapper)
        {
            _classForumScoreRepository = classForumScoreRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<ClassForumScoreModel>>> Handle(GetListClassForumScoresQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassForumScoreModel>> methodResult = new MethodResult<IList<ClassForumScoreModel>>();
            var classForumResults = await _classForumScoreRepository.Queryable
                                            .Include(x => x.ClassForumResult)
                                            .Where(x => x.ClassForumResult!.Id == request.ClassForumResultId)
                                            .Select(x => new ClassForumScoreModel
                                            {
                                                Id = x.Id,
                                                CreatedDate = x.CreatedDate,
                                                Criteria = x.Criteria,
                                                Feedback = x.Feedback,
                                                Score = x.Score,
                                                ClassForumResultId = x.ClassForumResultId,
                                            }).ToListAsync(cancellationToken);
            methodResult.Result = classForumResults;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
