// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CampusQuery
{
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentsQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<StudentCampusModel>>>
    {
        public IList<EnumCurriculumStatus>? CurriculumStatus { get; set; }
        public Guid? SchoolClassId { get; set; }
        public IList<Guid>? SchoolClassIds { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }

    public class SearchStudentsQueryHandler : IRequestHandler<SearchStudentsQuery, MethodResult<PagingItemsModel<StudentCampusModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IStudentRepository _studentRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly ISchoolClassRepository _schoolClassRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;
        private readonly AuthContext _authContext;

        public SearchStudentsQueryHandler(UserManager<User> userManager, IStudentRepository studentRepository, ILmsCourseService lmsCourseService, ISchoolClassRepository schoolClassRepository, AuthContext authContext, IUserSchoolRepository userSchoolRepository)
        {
            _userManager = userManager;
            _studentRepository = studentRepository;
            _lmsCourseService = lmsCourseService;
            _schoolClassRepository = schoolClassRepository;
            _authContext = authContext;
            _userSchoolRepository = userSchoolRepository;
        }

        public async Task<MethodResult<PagingItemsModel<StudentCampusModel>>> Handle(SearchStudentsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentCampusModel>>();

            var schoolIdStr = _authContext.ClaimsPrincipal?.FindFirstValue("SchoolId");

            if (string.IsNullOrEmpty(schoolIdStr) || !Guid.TryParse(schoolIdStr, out Guid schoolId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolId), _authContext.CurrentUserId);
                return methodResult;
            }

            var query = from u in _userManager.Users
                        join s in _studentRepository.Queryable on u.Id equals s.UserId
                        where s.SchoolId == schoolId
                        select new StudentCampusModel()
                        {
                            Id = u.Id,
                            CreatedDate = u.CreatedDate,
                            FullName = u.FullName,
                            Class = s.SchoolClass,
                            Email = u.Email,
                            UserName = u.UserName,
                            PhoneNumber = u.PhoneNumber,
                            StudentId = s.Id,
                            SchoolClassId = s.SchoolClassId,
                            SchoolId = s.SchoolId,
                            Gender = u.Gender,
                            School = s.School,
                            Birthday = u.Birthday,
                            DefaultPassword = u.DefaultPassword,
                            StudentCode = u.Code
                        };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(p => !string.IsNullOrEmpty(p.Email) && p.Email == request.Keyword);
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    query = query.Where(p => !string.IsNullOrEmpty(p.PhoneNumber) && p.PhoneNumber == request.Keyword);
                }
                else
                {
                    query = query.Where(p => !string.IsNullOrEmpty(p.FullName) && p.FullName.Contains(request.Keyword));
                }
            }

            if (request.SchoolClassId.HasValue)
            {
                query = query.Where(p => p.SchoolClassId == request.SchoolClassId);
            }

            if (request.SchoolClassIds != null && request.SchoolClassIds.Any())
            {
                query = query.Where(p => p.SchoolClassId.HasValue && request.SchoolClassIds.Contains(p.SchoolClassId.Value));
            }

            if (request.StudentIds != null && request.StudentIds.Any())
            {
                query = query.Where(p => request.StudentIds.Contains(p.StudentId));
            }

            query = query.OrderBy(p => p.FullName);

            var lists = new List<StudentCampusModel>();
            int totalItem = 0;

            if (request.CurriculumStatus != null && request.CurriculumStatus.Any())
            {
                var students = await query.ToListAsync(cancellationToken);

                var studentIds = students.Select(p => p.StudentId).ToList();
                var learningProgressResults = await _lmsCourseService.GetStudentsLearningProgress(new GetStudentsLearningProgressQueryModel() { StudentIds = studentIds });
                var learningProgress = learningProgressResults.Content?.Result;

                students.ForEach(p =>
                {
                    p.LearningProgresses = learningProgress?.Where(x => x.StudentId == p.StudentId).ToList();
                });

                students = students.Where(p => p.LearningProgresses != null && p.LearningProgresses.Any(x => request.CurriculumStatus.Contains(x.CurriculumStatus))).ToList();

                totalItem = students.Count;
                lists = students.ApplySortAndPaging(request).ToList();
            }
            else
            {
                totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                lists = await query.ApplyPaging(request)
                                           .AsNoTracking()
                                           .ToListAsync(cancellationToken: cancellationToken)
                                           .ConfigureAwait(false);

                var studentIds = lists.Select(p => p.StudentId).ToList();

                var learningProgressResults = await _lmsCourseService.GetStudentsLearningProgress(new GetStudentsLearningProgressQueryModel() { StudentIds = studentIds });
                var learningProgress = learningProgressResults.Content?.Result;

                lists.ForEach(p =>
                {
                    p.LearningProgresses = learningProgress?.Where(x => x.StudentId == p.StudentId).ToList();
                });
            }

            var schoolClassIds = lists.Where(p => p.SchoolClassId.HasValue).Select(p => p.SchoolClassId).Distinct().ToList();
            var schoolClasses = await _schoolClassRepository.Queryable.WhereBulkContains(schoolClassIds, p => p.Id).ToListAsync(cancellationToken);
            lists.ForEach(p =>
            {
                p.Class = schoolClasses.FirstOrDefault(x => x.Id == p.SchoolClassId)?.Name;
            });

            methodResult.Result = new PagingItemsModel<StudentCampusModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
