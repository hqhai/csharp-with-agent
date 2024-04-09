// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery.Admin
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Application.Services.OrderServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassByIdQuery : IRequest<MethodResult<ClassModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetClassByIdQueryHandler : IRequestHandler<GetClassByIdQuery, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IClassStudentRepository _classStudentRepository;

        public GetClassByIdQueryHandler(IClassRepository classRepository, IMapper mapper, IOrderService orderService, IUserService userService, IClassStudentRepository classStudentRepository)
        {
            _classRepository = classRepository;
            _mapper = mapper;
            _orderService = orderService;
            _userService = userService;
            _classStudentRepository = classStudentRepository;
        }

        public async Task<MethodResult<ClassModel>> Handle(GetClassByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            var classes = await _classRepository.GetByIdAsync(request.Id);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classes));
                return methodResult;
            }
            var classModel = _mapper.Map<ClassModel>(classes);
            classModel.NumberOfStudent = await _classStudentRepository.Queryable.Where(p => p.ClassId == classes.Id && p.IsActive).CountAsync(cancellationToken);
            var packageResult = await _orderService.GetPackages();
            classModel.PackageCode = packageResult.Content?.Result?.FirstOrDefault(p => p.Id == classModel.PackageId)?.Code;

            if (classModel.TeacherId.HasValue)
            {
                var tearcherResult = await _userService.GetTeacherByIdAsync(classModel.TeacherId ?? default);
                var teacher = tearcherResult.Content?.Result;
                var countClass = await _classRepository.Queryable.Where(p => p.TeacherId == classModel.TeacherId).CountAsync(cancellationToken);
                classModel.Teacher = new CSOTeacherModel
                {
                    Name = teacher?.User?.FullName,
                    Phonenumber = teacher?.User?.PhoneNumber,
                    Email = teacher?.User?.Email,
                    CountClass = countClass,
                    AvatarPath = teacher?.User?.AvatarPath,
                };
            }
            if (classModel.CsoId.HasValue)
            {
                var csoResult = await _userService.GetCSOByIds(new List<Guid> { classModel.CsoId ?? default });
                var cso = csoResult.Content?.Result?.FirstOrDefault();
                var countClass = await _classRepository.Queryable.Where(p => p.CsoId == classModel.CsoId).CountAsync(cancellationToken);
                classModel.Cso = new CSOTeacherModel
                {
                    Name = cso?.User?.FullName,
                    Phonenumber = cso?.User?.PhoneNumber,
                    Email = cso?.User?.Email,
                    CountClass = countClass,
                    AvatarPath = cso?.User?.AvatarPath
                };
            }

            methodResult.Result = classModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
