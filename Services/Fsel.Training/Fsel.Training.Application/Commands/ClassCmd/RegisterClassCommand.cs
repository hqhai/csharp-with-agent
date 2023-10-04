// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
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
        private readonly IMediator _mediator;

        public RegisterClassCommandHandler(IClassRepository classRepository,
            IClassStudentRepository classStudentRepository,
            IUserService userService,
            IMapper mapper,
            IMediator mediator)
        {
            _classRepository = classRepository;
            _classStudentRepository = classStudentRepository;
            _userService = userService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MethodResult<ClassModel>> Handle(RegisterClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();
            var codeResult = await _mediator.Send(new GetNewClassCodeQuery { Code = request.Code, CourseLevel = request.CourseLevel }, cancellationToken).ConfigureAwait(false);
            var code = codeResult.Result;

            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                var classnew = await _classRepository.Queryable.Include(x => x.ClassStudents).OrderBy(x => x.CreatedDate)
                           .FirstOrDefaultAsync(x => x.CourseId == request.CourseId, cancellationToken);

                if (classnew == null)
                {
                    classnew = await CreateClassAsync(code, request.CourseId, request.PackageId, request.LiveTimeFrameId, request.LiveDays);
                }
                else if (classnew.ClassStudents.Count > 99 || classnew.Status == EnumStatusClass.Active)
                {
                    var code = await _mediator.Send(new GetNewClassCodeQuery { Code = request.Code, CourseLevel = request.CourseLevel }, cancellationToken).ConfigureAwait(false);
                    classnew = await CreateClassAsync(code.Result, request.CourseId, request.PackageId, request.LiveTimeFrameId, request.LiveDays).ConfigureAwait(false);
                }
                var student = await _userService.GetStudentByUserIdAsync(request.UserId ?? default);
                if (student == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                    return methodResult;
                }
                var studentId = student?.Content?.Result?.Id;

                var classStudent = await _classStudentRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.ClassId == classnew.Id, cancellationToken);
                if (classStudent == null)
                {
                    await UpdateClassAsync(classnew, studentId ?? default);
                }

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
