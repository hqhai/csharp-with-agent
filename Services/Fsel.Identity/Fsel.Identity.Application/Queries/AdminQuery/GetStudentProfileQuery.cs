// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProfileQuery : IRequest<MethodResult<StudentModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetStudentProfileQueryHandler : IRequestHandler<GetStudentProfileQuery, MethodResult<StudentModel>>
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly ITrainingService _trainingService;
        private readonly IOrderService _orderService;
        private readonly ISystemService _systemService;
        private readonly IUserSchoolRepository _userSchoolRepository;

        public GetStudentProfileQueryHandler(IMapper mapper,
            UserManager<User> userManager,
            ITrainingService trainingService,
            IOrderService orderService,
            ISystemService systemService,
            IUserSchoolRepository userSchoolRepository)
        {
            _mapper = mapper;
            _userManager = userManager;
            _trainingService = trainingService;
            _orderService = orderService;
            _systemService = systemService;
            _userSchoolRepository = userSchoolRepository;
        }

        public async Task<MethodResult<StudentModel>> Handle(GetStudentProfileQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentModel> methodResult = new MethodResult<StudentModel>();
            var isStudentToSchool = await _userSchoolRepository.CheckStudentToAdminSchoolAsync(request.StudentId);
            if (!isStudentToSchool)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserSchoolErrorCode.StudentNotInSchool), nameof(isStudentToSchool));
                return methodResult;
            }
            var userView = await _userManager.Users.Include(x => x.Student)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .FirstOrDefaultAsync(x => x.Student != null && x.Student.Id == request.StudentId, cancellationToken);
            if (userView == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(userView));
                return methodResult;
            }
            var student = userView.Student;

            var parentStudent = student?.ParentStudents.FirstOrDefault();
            if (student != null && parentStudent != null)
            {
                userView = await _userManager.Users.Include(x => x!.Student)
                                              .ThenInclude(x => x!.ParentStudents)
                                              .ThenInclude(x => x.Parent)
                                              .ThenInclude(x => x!.User)
                                              .FirstOrDefaultAsync(x => x.Student != null && x.Student.Id == request.StudentId, cancellationToken);
                student = userView?.Student;
                //if (student?.SchoolId != null)
                //{
                //    var schoolResult = await _systemService.ExecuteListSchoolQueryAsync(new BaseQueryModel
                //    {
                //        Filters = new List<GenericFilterModel>() { new GenericFilterModel { Property = "Id", Operator = Common.Enums.EnumFilterOperator.Equal, Value = student.SchoolId } },
                //        IncludePaths = new List<string>() { "School" }
                //    });
                //    if (!schoolResult.IsSuccessStatusCode || schoolResult.Content?.Result == null)
                //    {
                //        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                //        return methodResult;
                //    }
                //    student.School = schoolResult.Content?.Result?.FirstOrDefault()?.Name;
                //}
                parentStudent = student?.ParentStudents.FirstOrDefault();
            }
            var userModel = _mapper.Map<StudentModel>(userView);
            _mapper.Map(userView, userModel);
            _mapper.Map(student, userModel);
            if (parentStudent != null && parentStudent.Parent != null)
            {
                userModel.Parent = _mapper.Map<ParentProfileModel>(parentStudent.Parent.User);
                userModel.Parent.Occupation = parentStudent.Parent.Occupation;
            }
            var classStudent = await _trainingService.GetClassToStudentId(student?.Id ?? default);
            var @class = classStudent?.Content?.Result;
            if (@class != null)
            {
                userModel.CodeClass = classStudent?.Content?.Result?.Code;
            }
            var packageResult = await _orderService.GetPackages();
            var packages = packageResult?.Content?.Result;
            if (packages != null && packages.Any())
            {
                userModel.Membership = packages.FirstOrDefault(p => p.Id == student?.PackageId)?.Code;
            }
            methodResult.Result = userModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
