// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCompetitionSnapShotCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentCompetitionSnapShot;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateSnapShotResultCommand : CreateStudentCompetitionSnapShotModel, IRequest<MethodResult<StudentCompetitionSnapShotModel>>
    {
    }

    public class CreateSnapShotResultCommandHandler : IRequestHandler<CreateSnapShotResultCommand, MethodResult<StudentCompetitionSnapShotModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentCompetitionSnapShotRepository _studentCompetitionSnapShotRepository;

        public CreateSnapShotResultCommandHandler(IMapper mapper, IStudentCompetitionSnapShotRepository studentCompetitionSnapShotRepository)
        {
            _mapper = mapper;
            _studentCompetitionSnapShotRepository = studentCompetitionSnapShotRepository;
        }

        public async Task<MethodResult<StudentCompetitionSnapShotModel>> Handle(CreateSnapShotResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentCompetitionSnapShotModel> methodResult = new MethodResult<StudentCompetitionSnapShotModel>();

            var studentCompetitionSnapshot = _mapper.Map<StudentCompetitionSnapShot>(request);

            await _studentCompetitionSnapShotRepository.ExecuteTransactionAsync(async () =>
            {
                _studentCompetitionSnapShotRepository.Add(studentCompetitionSnapshot);
                await _studentCompetitionSnapShotRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;

            });
            return methodResult;
        }
    }
}
