// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Class.Application.Commands.ClassCmd
{
    using AutoMapper;
    using Fsel.Class.Doman.IRepositories;
    using Fsel.Class.Doman.Models.CommandModels.Classes;
    using Fsel.Class.Doman.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateClassCommand : CreateClassCommandModel, IRequest<MethodResult<ClassModel>>
    {
    }

    public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IMapper _mapper;

        public CreateClassCommandHandler(IClassRepository classRepository,
            IMapper mapper)
        {
            _classRepository = classRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassModel>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
        {
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            #region Validation

            var classs = await _classRepository.Queryable.FirstOrDefaultAsync(x => x.Code == request.Code);
            if (classs != null)
            {
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassModel>(classs);
                return methodResult;
            }

            #endregion Validation

            return methodResult;
        }
    }
}
