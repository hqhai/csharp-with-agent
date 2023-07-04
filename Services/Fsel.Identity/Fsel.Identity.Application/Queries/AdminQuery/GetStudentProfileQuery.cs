// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProfileQuery : IRequest<MethodResult<UserProfileModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetStudentProfileQueryHandler : IRequestHandler<GetStudentProfileQuery, MethodResult<UserProfileModel>>
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly ITrainingService _trainingService;
        private readonly IOrderService _orderService;

        public GetStudentProfileQueryHandler(IMapper mapper, UserManager<User> userManager, ITrainingService trainingService, IOrderService orderService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _trainingService = trainingService;
            _orderService = orderService;
        }

        public async Task<MethodResult<UserProfileModel>> Handle(GetStudentProfileQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserProfileModel> methodResult = new MethodResult<UserProfileModel>();

            var userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Student)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.StudentId, cancellationToken);
            if (userView == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.StudentNotExist));
                return methodResult;
            }
            var student = userView.Human?.Student;
            var parentStudent = student?.ParentStudents.FirstOrDefault();
            if (student != null && student.CreatedByParent == false && parentStudent != null)
            {
                userView = await _userManager.Users.Include(x => x.Human)
                                              .ThenInclude(x => x!.Student)
                                              .ThenInclude(x => x!.ParentStudents)
                                              .ThenInclude(x => x.Parent)
                                              .ThenInclude(x => x!.Human)
                                              .FirstOrDefaultAsync(x => x.Id == userView.Id, cancellationToken);
                student = userView?.Human?.Student;
                parentStudent = student?.ParentStudents.FirstOrDefault();
            }
            var userModel = _mapper.Map<UserProfileModel>(userView);
            _mapper.Map(userView?.Human, userModel);
            _mapper.Map(student, userModel);
            if (student?.CreatedByParent == false && parentStudent != null)
            {
                userModel.Parent = _mapper.Map<ParentProfileModel>(parentStudent.Parent);
            }
            var classStudent = await _trainingService.GetClassByStudentId(student?.Id ?? default);
            if (classStudent.Content?.Result != null)
            {
                userModel.CodeClass = classStudent?.Content?.Result?.Code;
            }
            var package = await _orderService.GetPackages();
            if (package.IsSuccessStatusCode)
            {
                userModel.Membership = package.Content?.Result?.FirstOrDefault(p => p.Id == userModel.PackageId)?.Code;
            }
            methodResult.Result = userModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
