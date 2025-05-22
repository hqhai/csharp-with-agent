// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUsersByPhoneNumberQuery : IRequest<MethodResult<IList<StudentModel>>>
    {
        public string? PhoneNumber { get; set; }
    }

    public class GetUsersByPhoneNumberQueryHandler : IRequestHandler<GetUsersByPhoneNumberQuery, MethodResult<IList<StudentModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public GetUsersByPhoneNumberQueryHandler(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<StudentModel>>> Handle(GetUsersByPhoneNumberQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentModel>>();

            var users = await _userManager.Users.Include(p => p.Student).Where(p => p.PhoneNumber == request.PhoneNumber).OrderByDescending(p => p.CreatedDate).ToListAsync(cancellationToken);

            var students = users.Where(p => p.Student != null).Select(p => p.Student).ToList();

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            return methodResult;
        }
    }
}
