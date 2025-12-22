// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CampusQuery.Classes
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolClassByIdQuery : IRequest<MethodResult<SchoolClassModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetSchoolClassByIdQueryHandler : IRequestHandler<GetSchoolClassByIdQuery, MethodResult<SchoolClassModel>>
    {
        private readonly ISchoolClassRepository _schoolClassRepository;
        private readonly UserManager<User> _userManager;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public GetSchoolClassByIdQueryHandler(ISchoolClassRepository schoolClassRepository, UserManager<User> userManager, IStudentRepository studentRepository, IMapper mapper)
        {
            _schoolClassRepository = schoolClassRepository;
            _userManager = userManager;
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SchoolClassModel>> Handle(GetSchoolClassByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SchoolClassModel>();

            var schoolClass = await _schoolClassRepository.GetByIdAsync(request.Id);
            if (schoolClass == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolClass), request.Id);
                return methodResult;
            }

            var schoolClassModel = _mapper.Map<SchoolClassModel>(schoolClass);

            schoolClassModel.NumberOfStudent = await _studentRepository.Queryable.Where(p => p.SchoolClassId == schoolClass.Id).CountAsync(cancellationToken);

            var createdUser = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == schoolClass.CreatedUserId, cancellationToken);

            schoolClassModel.CreatedFullName = createdUser?.FullName;

            if (schoolClassModel.TeacherId.HasValue)
            {
                var teacher = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == schoolClass.TeacherId, cancellationToken);
                schoolClassModel.TeacherName = teacher?.FullName;
            }

            methodResult.Result = schoolClassModel;
            return methodResult;
        }
    }
}
