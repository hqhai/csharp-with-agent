// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CampusCmd.Classes
{
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Entities.Campus;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Campus;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SaveSchoolClassCommand : SaveSchoolClassCommandModel, IRequest<MethodResult<SchoolClassModel>>
    {
    }

    public class SaveSchoolClassCommandHandler : IRequestHandler<SaveSchoolClassCommand, MethodResult<SchoolClassModel>>
    {
        private readonly ISchoolClassRepository _schoolClassRepository;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IStudentRepository _studentRepository;

        public SaveSchoolClassCommandHandler(ISchoolClassRepository schoolClassRepository, UserManager<User> userManager, RoleManager<Role> roleManager, IUserRoleRepository userRoleRepository, IMapper mapper, AuthContext authContext, IStudentRepository studentRepository)
        {
            _schoolClassRepository = schoolClassRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _userRoleRepository = userRoleRepository;
            _mapper = mapper;
            _authContext = authContext;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<SchoolClassModel>> Handle(SaveSchoolClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SchoolClassModel>();

            var schoolIdStr = _authContext.ClaimsPrincipal?.FindFirstValue("SchoolId");

            if (!string.IsNullOrEmpty(schoolIdStr) || !Guid.TryParse(schoolIdStr, out Guid schoolId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolId), _authContext.CurrentUserId);
                return methodResult;
            }

            if (await _schoolClassRepository.Queryable.AnyAsync(p => p.Name == request.Name && p.Id != request.Id, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Name), request.Name);
                return methodResult;
            }

            var schoolClass = _mapper.Map<SchoolClass>(request);
            schoolClass.SchoolId = schoolId;

            if (!schoolClass.IsValid())
            {
                methodResult.AddError(schoolClass.ErrorMessages);
                return methodResult;
            }

            var students = await _studentRepository.Queryable.Where(p => p.SchoolClassId == schoolClass.Id).ToListAsync(cancellationToken);

            var schoolClasses = new List<SchoolClass>() { schoolClass };

            await _schoolClassRepository.ExecuteTransactionAsync(async () =>
            {
                if (students.Any())
                {
                    students.ForEach(x =>
                    {
                        x.SchoolClass = schoolClass.Name;
                    });
                    await _studentRepository.BulkMergeAsync(students);
                }
                await _schoolClassRepository.BulkMergeAsync(schoolClasses);
                await _schoolClassRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = _mapper.Map<SchoolClassModel>(schoolClass);
                return methodResult;
            });

            return methodResult;
        }
    }
}
