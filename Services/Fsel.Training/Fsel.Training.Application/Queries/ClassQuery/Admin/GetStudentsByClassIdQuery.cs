// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery.Admin
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsByClassIdQuery : IRequest<MethodResult<IList<ClassStudentModel>?>>
    {
        public Guid ClassId { get; set; }
        public string? KeyWord { get; set; }
    }

    public class GetStudentsByClassIdQueryHandler : IRequestHandler<GetStudentsByClassIdQuery, MethodResult<IList<ClassStudentModel>?>>
    {
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly IUserService _userService;

        public GetStudentsByClassIdQueryHandler(IClassStudentRepository classStudentRepository, IUserService userService)
        {
            _classStudentRepository = classStudentRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<ClassStudentModel>?>> Handle(GetStudentsByClassIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassStudentModel>?> methodResult = new MethodResult<IList<ClassStudentModel>?>();

            var studentIds = await _classStudentRepository.Queryable.Where(p => p.ClassId == request.ClassId).Select(p => p.StudentId).ToListAsync(cancellationToken);
            if (studentIds.Count == 0)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var studentsResult = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            if (!studentsResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentsResult.Error);
                return methodResult;
            }
            var students = studentsResult.Content?.Result;

            var classStudents = students!.Select(x => new ClassStudentModel
            {
                StudentId = x.Id,
                StudentName = x.User?.FullName,
                Code = x.User?.Code,
                PackageId = x.PackageId
            }).ToList();
            if (!string.IsNullOrEmpty(request.KeyWord))
            {
                classStudents = classStudents.Where(p => !string.IsNullOrEmpty(p.StudentName) && p.StudentName.Contains(request.KeyWord, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            methodResult.Result = classStudents;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
