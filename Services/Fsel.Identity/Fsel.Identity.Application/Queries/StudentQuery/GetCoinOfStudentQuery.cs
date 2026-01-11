// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Caching;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCoinOfStudentQuery : IRequest<MethodResult<long>>
    {
    }

    public class GetCoinOfStudentQueryHandler : IRequestHandler<GetCoinOfStudentQuery, MethodResult<long>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly ICacheService<StudentModel> _cacheService;

        public GetCoinOfStudentQueryHandler(UserManager<User> userManager, IStudentRepository studentRepository, AuthContext authContext, ICacheService<StudentModel> cacheService)
        {
            _userManager = userManager;
            _studentRepository = studentRepository;
            _authContext = authContext;
            _cacheService = cacheService;
        }

        public async Task<MethodResult<long>> Handle(GetCoinOfStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<long>();

            var key = $"{CacheSettings.GetCoinOfStudent}{_authContext.CurrentUserId}".ToLower(CultureInfo.InvariantCulture);
            var studentCache = await _cacheService.GetAsync(key);
            if (studentCache != null)
            {
                methodResult.Result = studentCache.NumberOfToken;
                return methodResult;
            }

            var student = await (from u in _userManager.Users
                                 join s in _studentRepository.Queryable on u.Id equals s.UserId
                                 where u.Id == _authContext.CurrentUserId
                                 select s).FirstOrDefaultAsync(cancellationToken);

            if (student == null)
            {
                methodResult.Result = 0;
                return methodResult;
            }

            var numberOfToken = student.NumberOfToken;

            await _cacheService.SetAsync(key, new StudentModel() { NumberOfToken = numberOfToken }, TimeSpan.FromMinutes(10));

            methodResult.Result = numberOfToken;
            return methodResult;
        }
    }
}
