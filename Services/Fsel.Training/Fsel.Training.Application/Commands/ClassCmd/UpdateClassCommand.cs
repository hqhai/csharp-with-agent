// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateClassCommand : UpdateClassCommandModel, IRequest<MethodResult<ClassModel>>
    {
    }

    public class UpdateClassCommandHandler : IRequestHandler<UpdateClassCommand, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IMapper _mapper;

        public UpdateClassCommandHandler(IClassRepository classRepository, IMapper mapper)
        {
            _classRepository = classRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassModel>> Handle(UpdateClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();
            var classes = await _classRepository.GetByIdAsync(request.Id);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classes));
                return methodResult;
            }
            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                classes.LiveTimeFrameId = request.LiveTimeFrameId;
                classes.LiveDays = request.LiveDays;
                _classRepository.Update(classes);
                await _classRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ClassModel>(classes);
                return methodResult;
            });
            return methodResult;
        }
    }
}
