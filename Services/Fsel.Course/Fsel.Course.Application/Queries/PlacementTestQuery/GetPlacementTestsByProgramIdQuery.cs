// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.PlacementTestQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPlacementTestsByProgramIdQuery : BaseQueryModel, IRequest<MethodResult<IList<PlacementTestSearchModel>>>
    {
        public Guid ProgramId { get; set; }
        public string? LevelIdStr { get; set; }

        public IList<Guid>? LevelIds
        {
            get
            {
                return LevelIdStr.ToList<Guid>();
            }
        }
    }

    public class GetPlacementTestsByProgramIdQueryHandler : IRequestHandler<GetPlacementTestsByProgramIdQuery, MethodResult<IList<PlacementTestSearchModel>>>
    {
        private readonly ILevelRepository _levelRepository;
        private readonly ITestRepository _testRepository;

        public GetPlacementTestsByProgramIdQueryHandler(ILevelRepository levelRepository,
            ITestRepository testRepository)
        {
            _levelRepository = levelRepository;
            _testRepository = testRepository;
        }

        public async Task<MethodResult<IList<PlacementTestSearchModel>>> Handle(GetPlacementTestsByProgramIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<PlacementTestSearchModel>> methodResult = new MethodResult<IList<PlacementTestSearchModel>>();
            var queryLevel = _levelRepository.Queryable.Where(x => x.ProgramId == request.ProgramId);
            if (request.LevelIds != null && request.LevelIds.Any())
            {
                queryLevel = queryLevel.WhereBulkContains(request.LevelIds, x => x.Id);
            }

            var query = from baseQ in _testRepository.Queryable
                        join l in queryLevel on baseQ.LevelId equals l.Id
                        where baseQ.ProgramId == request.ProgramId
                        select new PlacementTestSearchModel
                        {
                            Id = baseQ.OriginalId,
                            Name = baseQ.Name,
                            CreatedDate = baseQ.CreatedDate,
                            UpdatedDate = baseQ.UpdatedDate,
                            LevelCode = baseQ.Level != null ? baseQ.Level.Code : string.Empty,
                            LevelId = baseQ.LevelId,
                            LevelName = baseQ.Level != null ? baseQ.Level.Name : null,
                        };
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    query = query.Where(m => m.Id == guid);
                }
                else
                {
                    query = query.Where(x => x.Name != null && x.Name.Contains(request.Keyword));
                }
            }

            methodResult.Result = await query.ApplySort(request).ToListAsync(cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
