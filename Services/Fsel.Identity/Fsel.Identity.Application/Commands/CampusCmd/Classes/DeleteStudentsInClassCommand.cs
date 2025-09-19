// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CampusCmd.Classes
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Campus;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class DeleteStudentsInClassCommand : DeleteStudentsInClassCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class DeleteStudentsInClassCommandHandler : IRequestHandler<DeleteStudentsInClassCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<User> _userManager;
        private readonly IHumanRepository _humanRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IOrderService _orderService;

        public DeleteStudentsInClassCommandHandler(IStudentRepository studentRepository, UserManager<User> userManager, IHumanRepository humanRepository, ILmsCourseService lmsCourseService, IOrderService orderService)
        {
            _studentRepository = studentRepository;
            _userManager = userManager;
            _humanRepository = humanRepository;
            _lmsCourseService = lmsCourseService;
            _orderService = orderService;
        }

        public async Task<MethodResult<bool>> Handle(DeleteStudentsInClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                return methodResult;
            }

            var query = await (from u in _userManager.Users
                               join h in _humanRepository.Queryable on u.Id equals h.UserId
                               join s in _studentRepository.Queryable.WhereBulkContains(request.StudentIds, x => x.Id) on h.Id equals s.HumanId
                               where s.SchoolClassId == request.SchoolClassId
                               select new
                               {
                                   User = u,
                                   Human = h,
                                   Student = s
                               }).ToListAsync(cancellationToken);

            var students = query.Select(x => x.Student).ToList();
            var humans = query.Select(x => x.Human).ToList();
            var users = query.Select(x => x.User).ToList();

            var studentIds = students.Select(x => x.Id).ToList();
            var userIds = users.Select(x => x.Id).ToList();

            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                await _studentRepository.BulkDeleteList(students, true);
                await _humanRepository.BulkDeleteList(humans, true);
                await _studentRepository.DbContext.BulkDeleteAsync(users);
                await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var deleteResult = await _lmsCourseService.DeleteCurriculumsByStudentIds(new DeleteCurriculumsByStudentIdsCommandModel() { StudentIds = studentIds });
                if (!deleteResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(deleteResult.Error);
                    return methodResult;
                }

                var deleteOrderResult = await _orderService.DeleteOrderOfStudentsCampus(new DeleteOrderOfStudentsCampusCommandModel() { UserIds = userIds });

                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
