// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery.Admin
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListTeacherOrCSOInClassQuery : IRequest<MethodResult<IList<CSOTeacherModel>>>
    {
        public bool ChangeFind { get; set; }
        public string? Keyword { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid? CsoId { get; set; }
        public Guid? TeacherId { get; set; }
    }

    public class GetListTeacherOrCSOInClassQueryHandler : IRequestHandler<GetListTeacherOrCSOInClassQuery, MethodResult<IList<CSOTeacherModel>>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;

        public GetListTeacherOrCSOInClassQueryHandler(IClassRepository classRepository, IUserService userService)
        {
            _classRepository = classRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<CSOTeacherModel>>> Handle(GetListTeacherOrCSOInClassQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CSOTeacherModel>> methodResult = new MethodResult<IList<CSOTeacherModel>>();
            if (request.ChangeFind)
            {
                var allCSOResult = await _userService.GetAllCSO();
                if (!allCSOResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(allCSOResult.Error);
                    return methodResult;
                }
                var allClass = await _classRepository.Queryable.ToListAsync(cancellationToken);

                var allCSO = allCSOResult.Content?.Result?.Select(x => new CSOTeacherModel
                {
                    Id = x.Id,
                    Name = x.User?.FullName,
                    Code = x.User?.Code,
                    CourseLevels = x.CourseLevels,
                    CountClass = allClass.Count(p => p.CsoId == x.Id),
                }).ToList();
                if (allCSO == null)
                {
                    return methodResult;
                }
                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    allCSO = allCSO.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                if (request.CourseLevel.HasValue)
                {
                    allCSO = allCSO.Where(p => p.CourseLevels != null && p.CourseLevels.Contains(request.CourseLevel ?? default)).ToList();
                }
                if (request.CsoId.HasValue)
                {
                    allCSO = allCSO.Where(p => p.Id != request.CsoId).ToList();
                }
                methodResult.Result = allCSO;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            else
            {
                var allTeacherResult = await _userService.GetAllTeacher();
                if (!allTeacherResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(allTeacherResult.Error);
                    return methodResult;
                }
                var allClass = await _classRepository.Queryable.ToListAsync(cancellationToken);

                var allTeacher = allTeacherResult.Content?.Result?.Select(x => new CSOTeacherModel
                {
                    Id = x.Id,
                    Name = x.User?.FullName,
                    Code = x.User?.Code,
                    CourseLevels = x.CourseLevels,
                    CountClass = allClass.Count(p => p.TeacherId == x.Id),
                }).ToList();
                if (allTeacher == null)
                {
                    return methodResult;
                }
                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    allTeacher = allTeacher!.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                if (request.CourseLevel.HasValue)
                {
                    allTeacher = allTeacher!.Where(p => p.CourseLevels != null && p.CourseLevels.Contains(request.CourseLevel ?? default)).ToList();
                }
                if (request.TeacherId.HasValue)
                {
                    allTeacher = allTeacher.Where(p => p.Id != request.TeacherId).ToList();
                }
                methodResult.Result = allTeacher;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
        }
    }
}
