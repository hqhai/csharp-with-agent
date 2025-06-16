// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CutOffCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Infrastructure;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class RestoreDataCutOffCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class RestoreDataCutOffCommandHandler : IRequestHandler<RestoreDataCutOffCommand, MethodResult<bool>>
    {
        private readonly CourseDbContext _courseDbContext;
        private readonly IUserService _userService;

        public RestoreDataCutOffCommandHandler(CourseDbContext courseDbContext,
                                               IUserService userService)
        {
            _courseDbContext = courseDbContext;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(RestoreDataCutOffCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.UserIds);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var userResults = await _userService.GetUserByIds(request.UserIds);
            if (!userResults.IsSuccessStatusCode)
            {
                methodResult.AddError(userResults.Error);
                return methodResult;
            }

            var users = userResults.Content?.Result;
            if (users == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(users), users);
                return methodResult;
            }

            if (users.All(x => x.Human?.User?.Status != Shared.Enums.EnumUserStatus.Disable))
            {
                methodResult.AddErrorBadRequest(nameof(EnumOtherErrorCode.NotStatusCutOff), nameof(users), users);
                return methodResult;
            }

            if (users.Any(x => x.Human?.User?.Status != Shared.Enums.EnumUserStatus.Disable))
            {
                methodResult.AddErrorBadRequest(nameof(EnumOtherErrorCode.OnlyUsersCutOff), nameof(users), users);
                return methodResult;
            }

            var userNames = users.Where(x => x.Human?.User?.UserName != null).Select(x => x.Human?.User?.UserName!).ToList();

            const int BatchSize = 50;
            var totalUsers = userNames.Count;

            if (totalUsers == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            try
            {
                for (int i = 0; i < totalUsers; i += BatchSize)
                {
                    var batchUsernames = userNames.Skip(i).Take(BatchSize).ToList();

                    var userNameJson = Common.Helpers.ConvertHelper.Serialize(batchUsernames);
                    if (string.IsNullOrEmpty(userNameJson))
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                        return methodResult;
                    }

                    var param = new SqlParameter("@Usernames", userNameJson);

                    await _courseDbContext.Database.ExecuteSqlRawAsync(
                        "EXEC RestoreDataCutOff @Usernames", new[] { param }, cancellationToken);
                }

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
            }
            catch (SqlException sqlEx)
            {
                methodResult.AddErrorBadRequest(sqlEx.Message);
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
            }

            return methodResult;
        }

    }
}
