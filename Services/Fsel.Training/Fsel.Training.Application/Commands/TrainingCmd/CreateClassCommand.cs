// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.TrainingCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Doman.Entities;
    using Fsel.Training.Doman.IRepositories;
    using Fsel.Training.Doman.Models.CommandModels.Trainings;
    using Fsel.Training.Doman.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateClassCommand : CreateTrainingCommandModel, IRequest<MethodResult<TrainingModel>>
    {
    }

    public class CreateTrainingCommandHandler : IRequestHandler<CreateClassCommand, MethodResult<TrainingModel>>
    {
        private readonly ITrainingRepository _trainingRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public CreateTrainingCommandHandler(ITrainingRepository trainingRepository,
            IUserService userService,
            IMapper mapper)
        {
            _trainingRepository = trainingRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<TrainingModel>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
        {
            MethodResult<TrainingModel> methodResult = new MethodResult<TrainingModel>();

            #region Validation

            Class? classnew = await _trainingRepository.Queryable.FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken: cancellationToken);
            if (classnew == null)
            {
                await CreateClassAsync(request, classnew);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<TrainingModel>(classnew);
            }
            else
            {
                var students = await _userService.GetStudentByClassIdAsync(classnew.Id);
                if (students != null && students.IsSuccessStatusCode)
                {
                    var liststudent = students.Content?.Result;
                    if (liststudent != null && liststudent.Count > 12)
                    {
                        await UpdateClassAsync(classnew);
                        classnew = new();
                        await CreateClassAsync(request, classnew);
                    }
                }
                var student = await _userService.UpdateStudentByClassIdAsync(new UpdateStudentByClassIdModel
                {
                    UserId = request.UserId,
                    ClassId = classnew.Id
                });
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<TrainingModel>(classnew);
            }

            #endregion Validation

            return methodResult;
        }

        private async Task CreateClassAsync(CreateClassCommand request, Class? classnew)
        {
            classnew = new Class();
            classnew.Code = request?.Code;
            classnew.Name = request?.Code;
            _trainingRepository.Add(classnew);
            await _trainingRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
        }

        private async Task UpdateClassAsync(Class classnew)
        {
            classnew.Status = EnumTrainingType.Active;
            _trainingRepository.Update(classnew);
            await _trainingRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
        }
    }
}
