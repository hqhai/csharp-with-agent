using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Application.Queries.UnitQuery
{
    public class GetUnitQuery : IRequest<MethodResult<UnitModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetUnitQueryHandler : IRequestHandler<GetUnitQuery, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;

        public GetUnitQueryHandler(IMapper mapper, IUnitRepository unitRepository)
        {
            _unitRepository= unitRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UnitModel>> Handle(GetUnitQuery request, CancellationToken cancellationToken)
        {
            MethodResult<UnitModel> methodResult = new MethodResult<UnitModel>();

            var unit = await _unitRepository.GetByIdAsync(request.Id);

            if (unit == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumUnitErrorCode.U01V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                return methodResult;
            }

            methodResult.Result = _mapper.Map<UnitModel>(unit);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
