// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.TestRequestHandler
{
    using System.Threading.Tasks;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;

    public interface ITestRequestGetCommonInfoHandler : IBaseTestRequestHandler
    {
    }

    public class TestRequestGetCommonInfoHandler : BaseTestRequestHandler, ITestRequestGetCommonInfoHandler
    {
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public TestRequestGetCommonInfoHandler(IUserService userService, AuthContext authContext)
        {
            _userService = userService;
            _authContext = authContext;
        }

        public override async Task Handle(TestRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.TestRequestCommand.StudentId.HasValue)
            {
                var studentResult = await _userService.GetUserByStudentId(context.TestRequestCommand.StudentId.Value);
                if (!studentResult.IsSuccessStatusCode)
                {
                    context.MethodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                    return;
                }
                context.Student = studentResult.Content?.Result;
            }
            else
            {
                var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
                if (!studentResult.IsSuccessStatusCode)
                {
                    context.MethodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                    return;
                }
                context.Student = studentResult.Content?.Result;
            }
            if (context.Student == null)
            {
                context.MethodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "Student");
                return;
            }

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
