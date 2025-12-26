// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.AuthCmd;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.CommandModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Admins;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Net.Http.Headers;

    public class ImportPersonalTrainingRecordsFromFileCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ImportPersonalTrainingRecordsFromFileCommandHandler : IRequestHandler<ImportPersonalTrainingRecordsFromFileCommand, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IStudentRepository _studentRepository;

        public ImportPersonalTrainingRecordsFromFileCommandHandler(IMediator mediator,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            ILmsCourseService lmsCourseService,
            IStudentRepository studentRepository)
        {
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _lmsCourseService = lmsCourseService;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ImportPersonalTrainingRecordsFromFileCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }
            var result = request.FormFile.ImportAndValidateExcel(async (PtImportRowCommandModel x, IList<PtImportRowCommandModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                if (!await _userManager.Users.AnyAsync(y => y.Email == x.Email))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = $"Email is {EnumSystemErrorCode.DataNotExist}" });
                }

                if (!string.IsNullOrEmpty(x.CourseLevel) && !Enum.TryParse(x.CourseLevel, out EnumCourseLevel _))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CourseLevel), Message = $"CourseLevel is {EnumSystemErrorCode.InValidFormat}" });
                }
                if (!string.IsNullOrEmpty(x.PhoneNumber) && !x.PhoneNumber.IsValidPhoneNumber())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.PhoneNumber), Message = $"PhoneNumber is {EnumSystemErrorCode.InValidFormat}" });
                }

                return await Task.FromResult(errors.Count == 0);
            });
            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var tokenAdmin = _httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString();
            var listUser = new List<CreateUserStudentToAdminCommandModel>();

            var emails = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(p => p.Email!).Distinct().ToList() ?? new List<string>();
            var users = await (from baseQ in _studentRepository.Queryable
                               join userQ in _userManager.Users.WhereBulkContains(emails, x => x.Email) on baseQ.UserId equals userQ.Id
                               select new
                               {
                                   Student = baseQ,
                                   User = userQ
                               }).ToListAsync(cancellationToken);

            foreach (var item in result.Datas)
            {
                var user = users.FirstOrDefault(x => x.User.Email?.ToLower(System.Globalization.CultureInfo.CurrentCulture) == item.Email?.ToLower(System.Globalization.CultureInfo.CurrentCulture));
                if (user == null)
                {
                    continue;
                }
                var student = user.Student;
                var userDto = user.User;

                if (!string.IsNullOrEmpty(item.PhoneNumber) && item.PhoneNumber.IsValidPhoneNumber() && userDto.PhoneNumber != item.PhoneNumber)
                {
                    userDto.PhoneNumber = item.PhoneNumber;
                    await _userManager.UpdateAsync(userDto);
                }

                var tokenResult = await _mediator.Send(new GenerateTokenCommand { Id = user.User.Id }, cancellationToken);
                if (tokenResult.Result?.AccessToken != null && _httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization] = "Bearer " + tokenResult.Result?.AccessToken;
                }
                if (!string.IsNullOrEmpty(item.CourseLevel) && Enum.TryParse(item.CourseLevel, out EnumCourseLevel courseLevel))
                {
                    await _lmsCourseService.SavePlacementTestDoneAsync(new SavePlacementTestDoneCommandModel { CourseLevel = courseLevel, StudentId = student?.Id ?? default, IsSendLevel = false });
                }
                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization] = tokenAdmin;
                }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
