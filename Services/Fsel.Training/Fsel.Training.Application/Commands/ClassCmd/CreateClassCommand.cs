// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Doman.Entities;
    using Fsel.Training.Doman.IRepositories;
    using Fsel.Training.Doman.Models.CommandModels.Classes;
    using Fsel.Training.Doman.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateClassCommand : CreateClassCommandModel, IRequest<MethodResult<ClassModel>>
    {
    }

    public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public CreateClassCommandHandler(IClassRepository classRepository,
            IUserService userService,
            IMapper mapper)
        {
            _classRepository = classRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassModel>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            Class? classnew = await _classRepository.Queryable.FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken: cancellationToken);
            if (classnew == null)
            {
                classnew = await CreateClassAsync(request, classnew);
            }
            var students = await _userService.GetStudentByClassIdAsync(classnew.Id.ToString());
            if (students != null && students.IsSuccessStatusCode)
            {
                var liststudent = students.Content?.Result;
                if (liststudent != null && liststudent.Count > 12)
                {
                    classnew = await UpdateClassAsync(classnew);
                    classnew = new();
                    classnew = await CreateClassAsync(request, classnew);
                }
            }
            var student = await _userService.UpdateStudentByClassIdAsync(new UpdateStudentByClassIdModel
            {
                ClassId = classnew.Id,
                UserId = request.UserId
            });
            methodResult.StatusCode = StatusCodes.Status201Created;
            methodResult.Result = _mapper.Map<ClassModel>(classnew);
            return methodResult;
        }

        private async Task<Class> CreateClassAsync(CreateClassCommand request, Class? classnew)
        {
            classnew = new Class();
            classnew.Code = request?.Code;
            classnew.Name = request?.Code;
            _classRepository.Add(classnew);
            await _classRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            return classnew;
        }

        private async Task<Class> UpdateClassAsync(Class classnew)
        {
            classnew.Status = EnumClassType.Active;
            _classRepository.Update(classnew);
            await _classRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            return classnew;
        }
    }
}
