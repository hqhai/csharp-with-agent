// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RegisterClassCommand : RegisterClassCommandModel, IRequest<MethodResult<ClassModel>>
    {
    }

    public class RegisterClassCommandHandler : IRequestHandler<RegisterClassCommand, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;

        public RegisterClassCommandHandler(IClassRepository classRepository,
            IClassStudentRepository classStudentRepository,
            IUserService userService,
            IMapper mapper,
            AuthContext authContext,
            IMediator mediator)
        {
            _classRepository = classRepository;
            _classStudentRepository = classStudentRepository;
            _userService = userService;
            _mapper = mapper;
            _authContext = authContext;
            _mediator = mediator;
        }

        public async Task<MethodResult<ClassModel>> Handle(RegisterClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                var classnew = await _classRepository.Queryable.Include(x => x.ClassStudents)
                           .FirstOrDefaultAsync(x => x.Code == request.Code && x.CourseId == request.CourseId, cancellationToken);

                if (classnew == null)
                {
                    classnew = await CreateClassAsync(request.Code, request.CourseId, request.PackageId, request.LiveTimeFrameId, request.LiveDays);
                }
                else if (classnew.ClassStudents.Count > 11 || classnew.Status == EnumStatusClass.Active)
                {
                    var code = await _mediator.Send(new GetNewClassCodeQuery { CourseLevel = request.CourseLevel }, cancellationToken).ConfigureAwait(false);
                    methodResult.Result = new ClassModel { Code = code.Result };
                    methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassHasTooManyStudents));
                    return methodResult;
                }

                var userId = _authContext.CurrentUserId;
                var student = await _userService.GetStudentByUserIdAsync(userId);
                if (student == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                    return methodResult;
                }
                var studentId = student!.Content!.Result!.Id;

                var classStudent = await _classStudentRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ClassId == classnew.Id, cancellationToken);
                if (classStudent != null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentId));
                    return methodResult;
                }

                await UpdateClassAsync(classnew, studentId);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassModel>(classnew);
                return methodResult;
            });

            return methodResult;
        }

        private async Task<Class> CreateClassAsync(string? code, Guid courseId, Guid packageId, Guid? liveTimeFrameId, IList<DayOfWeek>? liveDays)
        {
            var newClass = new Class();
            newClass.Code = code;
            newClass.Name = code;
            newClass.CourseId = courseId;
            newClass.PackageId = packageId;
            newClass.LiveTimeFrameId = liveTimeFrameId;
            newClass.LiveDays = liveDays;
            _classRepository.Add(newClass);
            await _classRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            return newClass;
        }

        private async Task<Class> UpdateClassAsync(Class classToUpdate, Guid studentId)
        {
            try
            {
                classToUpdate.ClassStudents.Add(new ClassStudent { StudentId = studentId });
                _classRepository.Update(classToUpdate);
                await _classRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                return classToUpdate;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the class object.", ex);
            }
        }
    }
}
