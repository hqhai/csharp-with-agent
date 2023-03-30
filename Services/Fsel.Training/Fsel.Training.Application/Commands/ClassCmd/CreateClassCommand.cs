// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Training.Application.Queries.ClassQuery;
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

            var classnew = await _classRepository.Queryable.FirstOrDefaultAsync(x => x.Code == request.Code, cancellationToken);
            var studentsResult = await _userService.GetStudentByClassIdAsync(classnew?.Id.ToString() ?? string.Empty);
            var students = studentsResult.Content?.Result;

            if (classnew == null)
            {
                classnew = await CreateClassAsync(request.Code, classnew);
            }
            else if(studentsResult != null && studentsResult.IsSuccessStatusCode && students != null)
            {
                if (students.Count == 11)
                {
                    classnew = await UpdateClassAsync(classnew);
                }
                else if (students.Count > 11)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassHasTooManyStudents));
                    return methodResult;
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

        private async Task<Class> CreateClassAsync(string? code, Class? entityClass)
        {
            entityClass = new Class();
            entityClass.Code = code;
            entityClass.Name = code;
            _classRepository.Add(entityClass);
            await _classRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            return entityClass;
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
