// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Api.Controllers.AdminSchool
{
    using System.Net;
    using Asp.Versioning;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion(ApiSettings.APIVersion1)]
    [ApiVersion(ApiSettings.APIVersion1i1)]
    [Route(Settings.APIDefaultRoute + "/admin-school/student")]
    [Common.Attributes.Permission(role: nameof(EnumRole.AdminSchool))]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSchoolRepository _userSchoolRepository;

        public StudentController(IMediator mediator, IUserSchoolRepository userSchoolRepository)
        {
            _mediator = mediator;
            _userSchoolRepository = userSchoolRepository;
        }

        /// <summary>
        /// Get SchoolId
        /// </summary>
        [HttpGet("schoolId")]
        [ProducesResponseType(typeof(MethodResult<Guid>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSchoolId()
        {
            MethodResult<Guid> methodResult = new MethodResult<Guid>();
            methodResult.Result = await _userSchoolRepository.GetSchoolIdAsync();
            return methodResult.GetActionResult();
        }

        /// <summary>
        /// Get Student
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MethodResult<IList<StudentModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(VoidMethodResult), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get()
        {
            MethodResult<IList<StudentModel>> methodResult = new MethodResult<IList<StudentModel>>();
            methodResult.Result = await _userSchoolRepository.GetStudentsByRoleAdminSchoolAsync();
            return methodResult.GetActionResult();
        }
    }
}
