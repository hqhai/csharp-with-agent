// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.StudentServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetLessonByUnitQuery : IRequest<MethodResult<LessonModel>>
    {
    }

    public class GetLessonByUnitQueryHandler : IRequestHandler<GetLessonByUnitQuery, MethodResult<LessonModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentService _studentService;
        private readonly ILessonStudentRepository _lessonStudentRepository;
        private readonly AuthContext _authContext;
        private readonly ILessonRepository _lessonRepository;

        public GetLessonByUnitQueryHandler(IMapper mapper,
            IStudentService studentService,
            ILessonStudentRepository lessonStudentRepository,
            AuthContext authContext,
            ILessonRepository lessonRepository
            )
        {
            _mapper = mapper;
            _studentService = studentService;
            _lessonStudentRepository = lessonStudentRepository;
            _authContext = authContext;
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<LessonModel>> Handle(GetLessonByUnitQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<LessonModel>();

            var user = _authContext.CurrentUserId.ToString();
            var student = await _studentService.GetStudentByUserIdAsync(user);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.NotStudent));
                return methodResult;
            }
            /*   var lessonStudent = await _lessonStudentRepository.Queryable.FirstOrDefault(x => x.StudentId)*/

            /*   var lessonQuery = from i in _lessonRepository.Queryable
                                 .Include(x => x.LessonStudents)*/
            /*.ThenInclude(x => x.StudentId)*/

            /*    .Include(x => x.UnitLessons).Where(x => x.)*/
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
