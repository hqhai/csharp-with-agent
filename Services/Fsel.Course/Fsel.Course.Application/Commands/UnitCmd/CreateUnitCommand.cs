
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Common.Models.Commands.Unit;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unit = Fsel.Course.Domain.Entities.Unit;

namespace Fsel.Course.Application.Commands.UnitCmd
{
    public class CreateUnitCommand : CreateUnitCommandModel, IRequest<MethodResult<UnitModel>>
    {
    }
    public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IMapper _mapper;
        public CreateUnitCommandHandler(IUnitRepository unitRepository,
            IMapper mapper)
        {
            _unitRepository= unitRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UnitModel>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
        {
            MethodResult<UnitModel> methodResult = new MethodResult<UnitModel>();

            #region Validation
            Unit unit = _mapper.Map<Unit>(request);

            if (!unit.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(unit.ErrorMessages);
                return methodResult;
            }
            #endregion

            await _unitRepository.ExecuteTransactionAsync(async () => {
                unit = _unitRepository.Add(unit);
                await _unitRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<UnitModel>(unit);
                return methodResult;
            });

            return methodResult;
        }
    }


}
