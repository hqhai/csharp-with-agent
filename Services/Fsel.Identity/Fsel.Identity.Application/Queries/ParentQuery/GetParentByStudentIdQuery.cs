using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Queries.ParentQuery
{
    public class GetParentByStudentIdQuery : IRequest<MethodResult<ParentProfileModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetParentByStudentIdQueryHandler : IRequestHandler<GetParentByStudentIdQuery, MethodResult<ParentProfileModel>>
    {
        private readonly IParentRepository _parentRepository;
        private readonly IParentStudentRepository _parentStudentRepository;
        private readonly UserManager<User> _userManager;

        public GetParentByStudentIdQueryHandler(IParentStudentRepository parentStudentRepository, UserManager<User> userManager, IParentRepository parentRepository)
        {
            _parentStudentRepository = parentStudentRepository;
            _userManager = userManager;
            _parentRepository = parentRepository;
        }

        public async Task<MethodResult<ParentProfileModel>> Handle(GetParentByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ParentProfileModel>();

            var parentStudent = await _parentStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentId, cancellationToken);
            if (parentStudent == null)
            {
                methodResult.StatusCode = StatusCodes.Status204NoContent;
                return methodResult;
            }
            var parent = from u in _userManager.Users
                         join p in _parentRepository.Queryable on u.Id equals p.UserId
                         where p.Id == parentStudent.ParentId
                         select new ParentProfileModel
                         {
                             Id = p.Id,
                             FullName = u.FullName,
                             Email = u.Email,
                             PhoneNumber = u.PhoneNumber,
                             Birthday = u.Birthday,
                             Occupation = p.Occupation
                         };
            methodResult.Result = await parent.FirstOrDefaultAsync(cancellationToken);
            return methodResult;
        }
    }
}
