// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.TeacherFreeDateCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.TeacherFreeDates;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateTeacherFreeDateCommand : UpdateTeacherFreeDateCommandModel, IRequest<MethodResult<TeacherFreeDateModel>>
    {
    }

    public class UpdateTeacherFreeDateCommandHandler : IRequestHandler<UpdateTeacherFreeDateCommand, MethodResult<TeacherFreeDateModel>>
    {
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly IMapper _mapper;

        public UpdateTeacherFreeDateCommandHandler(IUserService userService,
            AuthContext authContext,
            ITeacherFreeDateRepository teacherFreeDateRepository,
            IMapper mapper)
        {
            _userService = userService;
            _authContext = authContext;
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<TeacherFreeDateModel>> Handle(UpdateTeacherFreeDateCommand request, CancellationToken cancellationToken)
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

            var teacherFreeDate = await _teacherFreeDateRepository.Queryable.Include(x => x.TeacherFreeTimes).FirstOrDefaultAsync(x => x.Id == request.Id && x.TeacherId == teacherId, cancellationToken);
            if (teacherFreeDate == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teacher));
                return methodResult;
            }
            if (request.StartDate > request.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTeacherFreeDateErrorCode.StartDateNotBiggerThanEndDate));
                return methodResult;
            }

            var isCheckStart = await _teacherFreeDateRepository.Queryable.AnyAsync(p => (p.StartDate <= request.StartDate && p.EndDate >= request.StartDate) && p.TeacherId == teacherId && p.Id != request.Id, cancellationToken);
            if (isCheckStart)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(isCheckStart));
                return methodResult;
            }

            var isCheckEnd = await _teacherFreeDateRepository.Queryable.AnyAsync(p => (p.StartDate <= request.EndDate && p.EndDate >= request.EndDate) && p.TeacherId == teacherId && p.Id != request.Id, cancellationToken);
            if (isCheckEnd)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(isCheckEnd));
                return methodResult;
            }
            _mapper.Map(request, teacherFreeDate);
            teacherFreeDate.TeacherId = teacherId ?? default;
            if (!teacherFreeDate.IsValid())
            {
                methodResult.AddErrorBadRequest(teacherFreeDate.ErrorMessages);
                return methodResult;
            }

            #endregion Validate

            await _teacherFreeDateRepository.ExecuteTransactionAsync(async () =>
            {
                teacherFreeDate = _teacherFreeDateRepository.Update(teacherFreeDate);
                await _teacherFreeDateRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<TeacherFreeDateModel>(teacherFreeDate);
                return methodResult;
            });
            return methodResult;
        }
    }
}
