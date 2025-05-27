// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Admins;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ImportAccountDashboardCommand : BaseImportCommandModel, IRequest<MethodResult<ImportAccountDashboardModel>>
    {
        public EnumRole Role { get; set; }
    }

    public class ImportAccountDashboardCommandHandler : IRequestHandler<ImportAccountDashboardCommand, MethodResult<ImportAccountDashboardModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IEventManagerRepository _eventManagerRepository;
        private static readonly char[] s_lowercaseLetters = "abcdefghijkmnpqrstuvwxyz".ToCharArray();
        private static readonly char[] s_uppercaseLetters = "ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] s_digits = "123456789".ToCharArray();
        private static readonly char[] s_allowedChars = s_lowercaseLetters.Concat(s_uppercaseLetters).ToArray();

        public ImportAccountDashboardCommandHandler(UserManager<User> userManager,
                                                    RoleManager<Role> roleManager,
                                                    ICompetitionEventsRepository competitionEventsRepository,
                                                    IEventManagerRepository eventManagerRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _competitionEventsRepository = competitionEventsRepository;
            _eventManagerRepository = eventManagerRepository;
        }

        public async Task<MethodResult<ImportAccountDashboardModel>> Handle(ImportAccountDashboardCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ImportAccountDashboardModel> methodResult = new MethodResult<ImportAccountDashboardModel>();

            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            if (request.Role != EnumRole.DepartmentAdmin && request.Role != EnumRole.EducationDepartment && request.Role != EnumRole.EducationDivision)
            {
                methodResult.AddError(nameof(EnumAuthUserErrorCode.RoleNotInDashboard));
                return methodResult;
            }

            var result = request.FormFile.ImportAndValidateExcel(async (ImportAccountDashboardCommandModel x, IList<ImportAccountDashboardCommandModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.FullName))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.FullName), Message = "Họ và tên không được để trống" });
                }

                if (string.IsNullOrEmpty(x.UserName))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.UserName), Message = $"Username chưa điền hoặc sai định dạng" });
                }

                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email không đúng định dạng" });
                }

                if (string.IsNullOrEmpty(x.EventCode))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.EventCode), Message = "EventCode không được để trống" });
                }

                List<string> eventCodes = x.EventCode?.Split(',').ToList() ?? new List<string>();
                eventCodes = eventCodes.Select(x => x.Trim()).ToList();
                foreach (var eventCode in eventCodes)
                {
                    if (!string.IsNullOrEmpty(eventCode) && !await _competitionEventsRepository.Queryable.AnyAsync(c => !string.IsNullOrEmpty(c.EventCode) && eventCode == c.EventCode.Trim()))
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.EventCode), Message = "EventCode sai hoặc không tồn tại" });
                        break;
                    }
                }

                return await Task.FromResult(errors.Count == 0);
            });

            if (result.Stream != null)
            {
                methodResult.Result = new ImportAccountDashboardModel { Stream = result.Stream };
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var emails = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!.Trim()).ToList();

            var role = await _roleManager.FindByNameAsync(nameof(request.Role));
            if (role == null)
            {
                role = new Role
                {
                    Name = nameof(request.Role),
                    NormalizedName = nameof(request.Role),
                };
                await _roleManager.CreateAsync(role);
            }

            var users = await _userManager.Users.Where(x => !string.IsNullOrEmpty(x.Email) && emails.Contains(x.Email)).ToListAsync(cancellationToken);

            int countAccount = 0;

            foreach (var item in result.Datas)
            {
                if (string.IsNullOrEmpty(item.Email))
                {
                    continue;
                }

                if (users.Any(x => !string.IsNullOrEmpty(x.Email) && x.Email.Contains(item.Email, StringComparison.CurrentCulture)))
                {
                    continue;
                }

                try
                {
                    var passWord = GeneratePassword();
                    var user = new User();
                    user.UserName = item.UserName;
                    user.Email = item.Email;
                    user.FullName = item.FullName ?? item.Email;
                    user.EmailConfirmed = true;
                    user.DefaultPassword = passWord;
                    if (!user.IsValid())
                    {
                        methodResult.AddErrorBadRequest(user.ErrorMessages);
                        return methodResult;
                    }

                    var resultUser = await _userManager.CreateAsync(user, passWord);
                    if (!resultUser.Succeeded)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                        return methodResult;
                    }

                    await _userManager.AddToRoleAsync(user, request.Role.ToString());

                    // thêm vào eventManager
                    List<string> eventCodes = item.EventCode?.Split(',').ToList() ?? new List<string>();
                    eventCodes = eventCodes.Select(x => x.Trim()).ToList();
                    foreach (var eventCode in eventCodes)
                    {
                        await AddEventManager(eventCode, user.Id, request.Role, cancellationToken);
                    }

                    countAccount += 1;
                }
                catch (Exception e)
                {
                    methodResult.AddError(nameof(EnumAuthUserErrorCode.SendAuthErorr), e.Message);
                    return methodResult;
                }

            }

            methodResult.Result = new ImportAccountDashboardModel { CountAccount = countAccount };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<VoidMethodResult> AddEventManager(string? eventCode, Guid userId, EnumRole role, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var competitionEvent = await _competitionEventsRepository.Queryable
                                                                     .FirstOrDefaultAsync(x => !string.IsNullOrEmpty(x.EventCode) && !string.IsNullOrEmpty(eventCode) && x.EventCode.Trim() == eventCode.Trim(), cancellationToken);
            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (role == EnumRole.DepartmentAdmin || role == EnumRole.EducationDepartment)
            {
                var parentEventId = await ParentEventAsync(competitionEvent.Id, cancellationToken);
                var eventChildren = await _competitionEventsRepository.Queryable.Where(x => x.ParentEventId == parentEventId).ToListAsync(cancellationToken);
                var eventIds = eventChildren.Select(x => x.Id).ToList();
                eventIds.Add(competitionEvent.Id);

                IList<EventManager> events = new List<EventManager>();
                foreach (var eventId in eventIds)
                {
                    events.Add(new EventManager { UserId = userId, CompetitionEventId = eventId });
                }

                await _eventManagerRepository.AddList(events);
            }
            else
            {
                _eventManagerRepository.Add(new EventManager { UserId = userId, CompetitionEventId = competitionEvent.Id });
            }

            await _eventManagerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return methodResult;
        }

        private async Task<Guid?> ParentEventAsync(Guid parentEventId, CancellationToken cancellationToken)
        {
            var competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(x => x.Id == parentEventId, cancellationToken);
            if (competitionEvent == null)
            {
                return parentEventId;
            }

            if (!competitionEvent.ParentEventId.HasValue)
            {
                return competitionEvent.Id;
            }
            else
            {
                return await ParentEventAsync(competitionEvent.ParentEventId.Value, cancellationToken);
            }
        }

        private static string GeneratePassword()
        {
            Random random = new Random();
            var randomChars = new char[4];
            for (int i = 0; i < randomChars.Length; i++)
            {
                randomChars[i] = s_allowedChars[random.Next(s_allowedChars.Length)];

                if (i == 3)
                {
                    randomChars[i] = s_digits[random.Next(s_digits.Length)];
                }
            }

            return "Fsel@" + new string(randomChars);
        }
    }
}
