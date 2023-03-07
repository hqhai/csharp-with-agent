using AutoMapper;
using Azure.Core;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;

using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Infrastructure;
using Fsel.Course.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unit = Fsel.Course.Domain.Entities.Unit;
namespace Fsel.Course.Application.Commands.UnitCmd
{
    public class DeleteUnitCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }

        public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand, MethodResult<bool>> 
        {

            private readonly IUnitRepository _unitRepository;
            private readonly IMapper _mapper;
            public DeleteUnitCommandHandler(IUnitRepository unitRepository,
            IMapper mapper) 
            {
                _unitRepository= unitRepository;
                _mapper= mapper;
            }



            public async Task<MethodResult<bool>> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
            {
                MethodResult<bool> methodResult = new MethodResult<bool >();

                var unit= await _unitRepository.GetByIdAsync(request.Id);

               /* if (!unit.IsValid())
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddResultFromErrorList(unit.ErrorMessages);
                    return methodResult;
                }*/
                if (unit == null)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddErrorMessage(
                        nameof(EnumUnitErrorCode.U01V),
                        new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                    return methodResult;
                }

                /*await _unitRepository.ExecuteTransactionAsync(async () => {
                    *//*unit = _unitRepository.Add(unit);*//*
                    
                     var delete = await _unitRepository.DeleteAsync(unit);
                    await _unitRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    if(delete == true)
                    {
                        methodResult.StatusCode = StatusCodes.Status201Created;
                        methodResult.Result = unit.Id;
                    }
                    else
                    {
                        methodResult.AddErrorMessage("Can't delete");
                    }

                    
                    return methodResult;

                });*/
                await _unitRepository.ExecuteTransactionAsync(async () =>
                {
                    var result = await _unitRepository.DeleteAsync(unit);
                    await _unitRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = result;
                    return methodResult;
                });
                return methodResult;
            }

            
            
        }
    }
}
