// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CutOffCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class RestoreDataCutOffCommand : IRequest<MethodResult<bool>>
    {
        public IList<string>? UserNames { get; set; }
    }

    public class RestoreDataCutOffCommandHandler : IRequestHandler<RestoreDataCutOffCommand, MethodResult<bool>>
    {
        private readonly CourseDbContext _courseDbContext;

        public RestoreDataCutOffCommandHandler(CourseDbContext courseDbContext)
        {
            _courseDbContext = courseDbContext;
        }

        public async Task<MethodResult<bool>> Handle(RestoreDataCutOffCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.UserNames);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            const int BatchSize = 50;
            var totalUsers = request.UserNames.Count;

            if (totalUsers == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            try
            {
                for (int i = 0; i < totalUsers; i += BatchSize)
                {
                    var batchUsernames = request.UserNames.Skip(i).Take(BatchSize).ToList();

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
