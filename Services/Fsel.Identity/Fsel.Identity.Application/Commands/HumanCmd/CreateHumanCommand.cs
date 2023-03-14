// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.HumanCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Humans;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateHumanCommand : CreateHumanCommandModel, IRequest<MethodResult<HumanModel>>
    {
    }

    public class CreateCourseCommandHandler : IRequestHandler<CreateHumanCommand, MethodResult<HumanModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHumanRepository _humanRepository;
        private readonly IParentRepository _parentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IParentStudentRepository _parentStudentRepository;

        public CreateCourseCommandHandler(IMapper mapper
            , IHumanRepository humanRepository
            , IParentRepository parentRepository
            , IStudentRepository studentRepository
            , IParentStudentRepository parentStudentRepository)
        {
            _mapper = mapper;
            _humanRepository = humanRepository;
            _parentRepository = parentRepository;
            _studentRepository = studentRepository;
            _parentStudentRepository = parentStudentRepository;
        }

        public async Task<MethodResult<HumanModel>> Handle(CreateHumanCommand request, CancellationToken cancellationToken)
        {
            MethodResult<HumanModel> methodResult = new MethodResult<HumanModel>();

            if (request == null)
            {
            }

            Human human = _mapper.Map<Human>(request);

            if (!human.IsValid())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddResultFromErrorList(human.ErrorMessages);
                return methodResult;
            }

            await _humanRepository.ExecuteTransactionAsync(async () =>
            {
                human = _humanRepository.Add(human);

                await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<HumanModel>(human);
                return methodResult;
            });

            return methodResult;
        }
    }
}
