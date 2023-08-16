// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.TeacherFreeDateCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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

            var teacher = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            if (!teacher.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teacher));
                return methodResult;
            }
            var teacherId = teacher.Content?.Result?.Id;
            if (request.StartDate < DateTime.Now.Date)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTeacherFreeDateErrorCode.StartDateBiggerThanDateNow));
                return methodResult;
            }
            if (request.StartDate > request.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTeacherFreeDateErrorCode.StartDateNotBiggerThanEndDate));
                return methodResult;
            }

            var teacherFreeDates = await _teacherFreeDateRepository.Queryable.Where(p => p.TeacherId == teacherId).ToListAsync(cancellationToken);
            var isCheckStart = teacherFreeDates.All(p => p.EndDate < request.StartDate);
            if (!isCheckStart)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(isCheckStart));
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
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<TeacherFreeDateModel>(teacherFreeDate);
                return methodResult;
            });
            return methodResult;
        }
    }
}
