// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.TeacherFreeDateCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.TeacherFreeDates;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateTeacherFreeDateCommand : CreateTeacherFreeDateCommandModel, IRequest<MethodResult<TeacherFreeDateModel>>
    {
    }

    public class CreateTeacherFreeDateCommandHandler : IRequestHandler<CreateTeacherFreeDateCommand, MethodResult<TeacherFreeDateModel>>
    {
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly IMapper _mapper;

        public CreateTeacherFreeDateCommandHandler(IUserService userService,
            AuthContext authContext,
            ITeacherFreeDateRepository teacherFreeDateRepository,
            IMapper mapper)
        {
            _userService = userService;
            _authContext = authContext;
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<TeacherFreeDateModel>> Handle(CreateTeacherFreeDateCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TeacherFreeDateModel> methodResult = new MethodResult<TeacherFreeDateModel>();

            #region Validate

            var teacher = await _userService.GetTeacherByIdAsync(_authContext.CurrentUserId);
            if (!teacher.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTeacherFreeDateErrorCode.TeacherNotExits));
                return methodResult;
            }
            var teacherId = teacher.Content?.Result?.Id;

            if (request.StartDate > request.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTeacherFreeDateErrorCode.StartDateNotBiggerThanEndDate));
                return methodResult;
            }

            var isCheck = await _teacherFreeDateRepository.Queryable.AnyAsync(p => (p.StartTime >= request.StartDate || p.EndTime >= request.StartDate) && p.TeacherId == teacherId, cancellationToken);
            if (isCheck)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTeacherFreeDateErrorCode.StartDateAlreadyExists));
                return methodResult;
            }

            TeacherFreeDate teacherFreeDate = _mapper.Map<TeacherFreeDate>(request);
            teacherFreeDate.TeacherId = teacherId ?? default;
            if (!teacherFreeDate.IsValid())
            {
                methodResult.AddErrorBadRequest(teacherFreeDate.ErrorMessages);
                return methodResult;
            }

            #endregion Validate

            await _teacherFreeDateRepository.ExecuteTransactionAsync(async () =>
            {
                teacherFreeDate = _teacherFreeDateRepository.Add(teacherFreeDate);
                await _teacherFreeDateRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<TeacherFreeDateModel>(teacherFreeDate);
                return methodResult;
            });
            return methodResult;
        }
    }
}
