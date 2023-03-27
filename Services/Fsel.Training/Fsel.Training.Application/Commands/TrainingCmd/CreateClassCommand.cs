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
    using Fsel.Training.Doman.Enums.ErrorCodes;
    using Fsel.Training.Doman.IRepositories;
    using Fsel.Training.Doman.Models.CommandModels.Classes;
    using Fsel.Training.Doman.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateClassCommand : CreateClassCommandModel, IRequest<MethodResult<ClassModel>>
    {
    }

    public class CreateTrainingCommandHandler : IRequestHandler<CreateClassCommand, MethodResult<ClassModel>>
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

        public async Task<MethodResult<ClassModel>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
        {
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            #region Validation

            Class? classnew = await _trainingRepository.Queryable.FirstOrDefaultAsync(x => x.Code == request.Code || x.Id == request.ClassId, cancellationToken: cancellationToken);
            if (classnew == null)
            {
                var classs = await _trainingRepository.Queryable
                                                .OrderByDescending(c => c.CreatedDate)
                                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (classs == null)
                {
                    methodResult.StatusCode = StatusCodes.Status400BadRequest;
                    methodResult.AddError(nameof(EnumClassErrorCode.ClassNull));
                    return methodResult;
                }
                var ischeckclass = await _userService.GetStudentByClassIdCheckAsync(classs.Id.ToString());
                var isclass = ischeckclass?.Content?.Result;
                if (isclass == false)
                {
                    classnew = await CreateClassAsync(request, classnew);
                    methodResult.StatusCode = StatusCodes.Status201Created;
                    methodResult.Result = _mapper.Map<ClassModel>(classnew);
                }
            }
            var students = await _userService.GetStudentByClassIdAsync(classnew.Id.ToString());
            if (students != null && students.IsSuccessStatusCode)
            {
                var liststudent = students.Content?.Result;
                if (liststudent != null && liststudent.Count > 12)
                {
                    classnew = await UpdateClassAsync(classnew);
                    classnew = new();
                    await CreateClassAsync(request, classnew);
                }
            }
            var student = await _userService.UpdateStudentByClassIdAsync(new UpdateStudentByClassIdModel
            {
                ClassId = classnew.Id,
                UserId = request.UserId
            });
            methodResult.StatusCode = StatusCodes.Status201Created;
            methodResult.Result = _mapper.Map<ClassModel>(classnew);

            #endregion Validation

            return methodResult;
        }

        private async Task<Class> CreateClassAsync(CreateClassCommand request, Class? classnew)
        {
            classnew = new Class();
            classnew.Code = request?.Code;
            classnew.Name = request?.Code;
            _trainingRepository.Add(classnew);
            await _trainingRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            return classnew;
        }

        private async Task<Class> UpdateClassAsync(Class classnew)
        {
            classnew.Status = EnumTrainingType.Active;
            _trainingRepository.Update(classnew);
            await _trainingRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            return classnew;
        }
    }
}
