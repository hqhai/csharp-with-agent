// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.AiCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Services.GoogleSheetServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using MediatR;

    public class AutoSubmitClassForumCommand : IRequest<MethodResult<bool>>
    {
        public int StartRow { get; set; }

        public int EndRow { get; set; }

        public string? SheetName { get; set; }

    }

    public class AutoSubmitClassForumCommandHandler : IRequestHandler<AutoSubmitClassForumCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly IGoogleSheetService _googleSheetService;
        private readonly IUserService _userService;

        public AutoSubmitClassForumCommandHandler(IMediator mediator, AppSetting appSetting, IUserService userService)
        {
            _mediator = mediator;
            _appSetting = appSetting;
            _googleSheetService = new GoogleSheetService(ResourceSettings.StudentCredentialsFilePath);
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(AutoSubmitClassForumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            List<StudentUpdateClassForum> listStudentNeedUpdate = new List<StudentUpdateClassForum>();

            var googleSheetId = _appSetting.GoogleSheetConfig?.StudentGetErrorClassForumSheetId;

            if (request.EndRow < request.StartRow)
            {
                methodResult.AddErrorBadRequest("EndRow can't greater than StartRow");
                return methodResult;
            }


            if (string.IsNullOrEmpty(googleSheetId) || string.IsNullOrEmpty(request.SheetName))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            try
            {
                IList<IList<object>> dataStudent = _googleSheetService.ReadDataFromSheet(googleSheetId, request.SheetName);


                int take = request.EndRow - request.StartRow + 1;
                var result = dataStudent.Skip(request.StartRow).Take(take).ToList();

                IList<string> emails = result.Select(x => x[2].ToString()).ToList();

                var usersResult = await _userService.GetStudentByEmailsAsync(emails);
                var users = usersResult?.Content?.Result?.ToList();


                var joinedData = result.Join(
                                   users,
                                   r => r[2].ToString(),
                                   u => u.User?.Email,
                                   (r, u) => new StudentUpdateClassForum
                                   {
                                       LessonName = r[3].ToString(),
                                       UserId = u.UserId
                                   }
                               ).ToList();




                if (dataStudent == null || dataStudent.Count < 2)
                {
                    return methodResult;
                }

                // Lấy headers từ dòng đầu tiên
                var headers = dataStudent[0].Select(h => h.ToString()).ToList();


                foreach (var item in joinedData)
                {

                    await _mediator.Send(new ToolSubmitClassForumAICommand
                    {
                        UserId = item.UserId,
                        LessonName = item.LessonName,
                    }, cancellationToken);
                }


            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
                return methodResult;
            }


            return methodResult;
        }

        public class StudentUpdateClassForum
        {
            public Guid UserId { get; set; }

            public string? LessonName { get; set; }
        }
    }
}
