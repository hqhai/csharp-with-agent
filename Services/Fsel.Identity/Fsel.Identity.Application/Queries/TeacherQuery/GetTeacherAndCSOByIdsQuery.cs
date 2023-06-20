// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.TeacherQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTeacherAndCSOByIdsQuery : IRequest<MethodResult<IList<HumanModel>>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class GetCSOByIdsQueryHandler : IRequestHandler<GetTeacherAndCSOByIdsQuery, MethodResult<IList<HumanModel>>>
    {
        private readonly ICSORepository _csoRepository;
        private readonly ITeacherRepository _teacherRepository;

        public GetCSOByIdsQueryHandler(ICSORepository csoRepository, ITeacherRepository teacherRepository)
        {
            _csoRepository = csoRepository;
            _teacherRepository = teacherRepository;
        }

        public async Task<MethodResult<IList<HumanModel>>> Handle(GetTeacherAndCSOByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<IList<HumanModel>>();
            if (request.Ids == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTeacherErrorCode.IdsNull));
                return methodResult;
            }
            IList<HumanModel> human = new List<HumanModel>();
            var listCso = await _csoRepository.Queryable.Where(p => request.Ids.Contains(p.Id)).Include(i => i.Human).Select(x => new HumanModel
            {
                Id = x.Id,
                FullName = x.Human!.FullName
            }).ToListAsync(cancellationToken);

            human = human.Concat(listCso).ToList();
            var teachers = await _teacherRepository.Queryable.Where(p => request.Ids.Contains(p.Id)).Include(i => i.Human).Select(x => new HumanModel
            {
                Id = x.Id,
                FullName = x.Human!.FullName
            }).ToListAsync(cancellationToken);

            human = human.Concat(teachers).ToList();

            methodResult.Result = human;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
