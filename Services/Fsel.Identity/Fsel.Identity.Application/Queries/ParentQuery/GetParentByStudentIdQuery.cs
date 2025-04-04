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
        private readonly IHumanRepository _humanRepository;
        private readonly IParentStudentRepository _parentStudentRepository;
        private readonly UserManager<User> _userManager;

        public GetParentByStudentIdQueryHandler(IParentStudentRepository parentStudentRepository, UserManager<User> userManager, IParentRepository parentRepository, IHumanRepository humanRepository)
        {
            _parentStudentRepository = parentStudentRepository;
            _userManager = userManager;
            _parentRepository = parentRepository;
            _humanRepository = humanRepository;
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
            var parent = from h in _humanRepository.Queryable
                         join p in _parentRepository.Queryable on h.Id equals p.HumanId
                         where p.Id == parentStudent.ParentId
                         select new ParentProfileModel
                         {
                             Id = p.Id,
                             FullName = h.FullName,
                             Email = h.Email,
                             PhoneNumber = h.PhoneNumber,
                             Birthday = h.Birthday,
                             Occupation = p.Occupation
                         };
            methodResult.Result = await parent.FirstOrDefaultAsync(cancellationToken);
            return methodResult;
        }
    }
}
