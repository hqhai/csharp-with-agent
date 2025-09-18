// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CampusQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentsQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<StudentCampusModel>>>
    {
        public EnumStudentCampusLearningStatus? LearningStatus { get; set; }
    }

    public class SearchStudentsQueryHandler : IRequestHandler<SearchStudentsQuery, MethodResult<PagingItemsModel<StudentCampusModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ILmsCourseService _lmsCourseService;

        public SearchStudentsQueryHandler(UserManager<User> userManager, IHumanRepository humanRepository, IStudentRepository studentRepository, ILmsCourseService lmsCourseService)
        {
            _userManager = userManager;
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<PagingItemsModel<StudentCampusModel>>> Handle(SearchStudentsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentCampusModel>>();

            var query = from u in _userManager.Users
                        join h in _humanRepository.Queryable on u.Id equals h.UserId
                        join s in _studentRepository.Queryable on h.Id equals s.HumanId
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
                    query = query.Where(p => !string.IsNullOrEmpty(p.FullName) && p.FullName.Contains(request.Keyword, StringComparison.CurrentCultureIgnoreCase));
                }
            }

            if (request.LearningStatus.HasValue)
            {
                var students = await query.ToListAsync(cancellationToken);

                var studentIds = students.Select(p => p.StudentId).ToList();
                var learningProgressResults = await _lmsCourseService.GetStudentsLearningProgress(new GetStudentsLearningProgressQueryModel() { StudentIds = studentIds });
                var learningProgress = learningProgressResults.Content?.Result;

                students.ForEach(p =>
                {
                    p.LearningProgresses = learningProgress?.Where(x => x.StudentId == p.StudentId).ToList();
                });

                students = students.Where(p => p.LearningProgresses != null && p.LearningProgresses.Any(x => x.ProgressStatus == request.LearningStatus)).ToList();

                int totalItem = students.Count;
                var lists = students.ApplySortAndPaging(request).ToList();

                methodResult.Result = new PagingItemsModel<StudentCampusModel>(lists, request, totalItem);
            }
            else
            {
                int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                var lists = await query.ApplySortAndPaging(request)
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

                methodResult.Result = new PagingItemsModel<StudentCampusModel>(lists, request, totalItem);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
