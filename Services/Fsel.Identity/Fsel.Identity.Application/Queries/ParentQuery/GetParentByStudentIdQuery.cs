using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
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
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var parent = from u in _userManager.Users
                         join h in _humanRepository.Queryable on u.Id equals h.UserId
                         join p in _parentRepository.Queryable on h.Id equals p.HumanId
                         where p.Id == parentStudent.ParentId
                         select new ParentProfileModel
                         {
                             FullName = u.FullName,
                             Email = u.Email,
                             PhoneNumber = u.PhoneNumber
                         };
            methodResult.Result = await parent.FirstOrDefaultAsync(cancellationToken);
            return methodResult;
        }
    }
}
