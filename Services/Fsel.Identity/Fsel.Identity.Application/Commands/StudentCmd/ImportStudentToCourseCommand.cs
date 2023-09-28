// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.InteractionService.Models;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Infrastructure.Repositories;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using EnumAuthErrorCode = Domain.Enums.ErrorCodes.EnumAuthErrorCode;

    public class ImportStudentToCourseCommand : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ImportStudentToCourseCommandHandler : IRequestHandler<ImportStudentToCourseCommand, MethodResult<Stream>>
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;
        private readonly IOrderService _orderService;
        private readonly IHumanRepository _humanRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IInteractionService _interactionService;
        private readonly IPlatformRepository _platformRepository;
        private const string DefaultPassword = "Hello.123";

        public ImportStudentToCourseCommandHandler(IMediator mediator, UserManager<User> userManager, IOrderService orderService, IHumanRepository humanRepository, ILmsCourseService lmsCourseService, IInteractionService interactionService, IPlatformRepository platformRepository)
        {
            _mediator = mediator;
            _userManager = userManager;
            _orderService = orderService;
            _humanRepository = humanRepository;
            _lmsCourseService = lmsCourseService;
            _interactionService = interactionService;
            _platformRepository = platformRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ImportStudentToCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            if (request.FormFile == null)
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.ImportFileRequired));
                return methodResult;
            }

            var packagesResult = await _orderService.GetPackages();

            var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentToCourseModel x, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                if (string.IsNullOrEmpty(x.Email))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null" });
                }
                else if (_userManager.Users.Any(p => p.Email == x.Email || p.UserName == x.Email))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email Already exist" });
                }
                if (string.IsNullOrEmpty(x.CodeCourse))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CodeCourse), Message = "Code Course is null" });
                }
                else
                {
                    var checkExistCode = await _lmsCourseService.GetCourseByCode(x.CodeCourse);
                    if (!checkExistCode.IsSuccessStatusCode || checkExistCode.Content?.Result == null)
                    {
                        errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CodeCourse), Message = "Code Course is not exist" });
                    }
                }
                if (string.IsNullOrEmpty(x.CodePackage))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CodePackage), Message = "Code Package is null" });
                }
                else if (!packagesResult.Content!.Result!.Any(p => p.Code == x.CodePackage))
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.CodePackage), Message = "Code Package is not exist" });
                }
                return await Task.FromResult(errors.Count == 0);
            });

            var duplicateEmails = result.Datas.GroupBy(user => user.Email).Where(group => group.Count() > 1).Select(group => group.Key);

            if (duplicateEmails.Any())
            {
                methodResult.AddErrorBadRequest("Dupilcate Emails");
                return methodResult;
            }

            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var users = new List<User>();

            try
            {
                foreach (var student in result.Datas.ToList())
                {
                    IdentityResult identityResult;
                    var user = new User();
                    user.UserName = student.Email;
                    user.Email = student.Email;
                    user.FullName = student.Email;
                    user.EmailConfirmed = true;

                    #region Add Platform to User
                    var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
                    if (platform != null)
                    {
                        user.UserPlatforms.Add(new UserPlatform
                        {
                            PlatformId = platform.Id
                        });
                    }
                    #endregion

                    identityResult = await _userManager.CreateAsync(user, DefaultPassword);
                    if (!identityResult.Succeeded)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.UserFailToCreate));
                        return methodResult;
                    }
                    await _userManager.AddToRoleAsync(user, EnumRole.Student.ToString());
                    user = await _userManager.Users.FirstOrDefaultAsync(p => p.UserName == user.UserName, cancellationToken);

                    var getCourseByCodeResult = await _lmsCourseService.GetCourseByCode(student.CodeCourse!);
                    if (!getCourseByCodeResult.IsSuccessStatusCode)
                    {
                        methodResult.AddError(getCourseByCodeResult.Error);
                        return methodResult;
                    }
                    var course = getCourseByCodeResult.Content?.Result;

                    Human human = new Human();
                    human.UserId = user?.Id;
                    human.Code = "Admin@123";
                    human.FullName = user?.FullName;
                    human.Student = new Student
                    {
                        HumanId = human.Id,
                        CreatedByParent = false,
                        Occupation = "Student",
                        CourseLevel = course!.CourseLevel
                    };
                    _humanRepository.Add(human);
                    await _humanRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    var updateCode = await _mediator.Send(new UpdateCodeStudentCommand { UserId = user?.Id, Gender = EnumGender.Male, Birthday = new DateTime(2011, 1, 1) }, cancellationToken).ConfigureAwait(false);

                    if (!updateCode.IsOK)
                    {
                        methodResult.AddError(updateCode.ErrorMessages);
                        return methodResult;
                    }

                    human.Student.CourseLevel = course!.CourseLevel;
                    _humanRepository.Update(human);
                    await _humanRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    var package = packagesResult.Content?.Result?.FirstOrDefault(p => p.Code == student.CodePackage);
                    if (package == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                        return methodResult;
                    }

                    var createOrderResult = await _orderService.CreateOrder(new CreateOrderCommandModel
                    {
                        CourseLevel = course!.CourseLevel,
                        FullName = student.Email,
                        Country = "VietNam",
                        Address = "35 Lac Trung",
                        PaymentMethod = EnumPaymentMethodStatus.BankTransfer,
                        CourseId = course.Id,
                        UserId = Guid.Parse(user!.Id),
                        PackageId = package.Id,
                        CodeCourse = student.CodeCourse
                    });
                    if (!createOrderResult.IsSuccessStatusCode)
                    {
                        methodResult.AddError(createOrderResult.Error);
                        return methodResult;
                    }

                    var updateStatusOrderResult = await _orderService.ChangeStatusOrder(new ChangeStatusOrderCommandModel
                    {
                        OrderId = createOrderResult.Content!.Result!.Id,
                        OrderStatus = EnumOrderStatus.Payment,
                        PackageId = package.Id,
                    });
                    if (!updateStatusOrderResult.IsSuccessStatusCode)
                    {
                        methodResult.AddError(updateStatusOrderResult.Error);
                        return methodResult;
                    }

                    var createSurveyResult = await _interactionService.CreateSurvey(new CreateCustomerSurveyCommandModel
                    {
                        Email = student.Email,
                        UserId = Guid.Parse(user.Id),
                        Answers = new List<CreateSurveyCommandModel>
                        {
                            new CreateSurveyCommandModel
                            {
                                Id = Guid.Parse("ee0e74f5-83ae-44dd-a7d0-0f7b650884f8"),
                                Answer = "FSEL",
                            }
                        }
                    });
                }

                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            catch (Exception ex)
            {
                methodResult.AddErrorBadRequest(ex.Message);
            }

            return methodResult;
        }
    }
}
