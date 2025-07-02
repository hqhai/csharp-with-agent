// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteStudentByUserIdCommand : IRequest<MethodResult<bool>>
    {
        public Guid UserId { get; set; }
    }

    public class DeleteStudentByUserIdCommandHandler : IRequestHandler<DeleteStudentByUserIdCommand, MethodResult<bool>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IUserSettingRepository _userSettingRepository;
        private readonly IUserPlatformRepository _userPlatformRepository;
        private readonly IStudentRankingRepository _studentRankingRepository;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;
        private readonly IStudentFocusTimeRepository _studentFocusTimeRepository;
        private readonly ISystemService _systemService;
        private readonly IInteractionService _interactionService;
        private readonly IOrderService _orderService;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly ITrainingService _trainingService;
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<User> _userManager;

        public DeleteStudentByUserIdCommandHandler(IUserOtpCodeRepository userOtpCodeRepository
                                                 , IUserSettingRepository userSettingRepository
                                                 , IUserPlatformRepository userPlatformRepository
                                                 , IStudentRankingRepository studentRankingRepository
                                                 , IStudentDailyStreakRepository studentDailyStreakRepository
                                                 , IStudentFocusTimeRepository studentFocusTimeRepository
                                                 , ISystemService systemService
                                                 , IInteractionService interactionService
                                                 , IOrderService orderService
                                                 , ILmsCourseService lmsCourseService
                                                 , ITrainingService trainingService
                                                 , IStudentRepository studentRepository
                                                 , UserManager<User> userManager)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _userSettingRepository = userSettingRepository;
            _userPlatformRepository = userPlatformRepository;
            _studentRankingRepository = studentRankingRepository;
            _studentDailyStreakRepository = studentDailyStreakRepository;
            _studentFocusTimeRepository = studentFocusTimeRepository;
            _systemService = systemService;
            _interactionService = interactionService;
            _orderService = orderService;
            _lmsCourseService = lmsCourseService;
            _trainingService = trainingService;
            _userManager = userManager;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteStudentByUserIdCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            ////delete Training
            //await _trainingService.DeleteListDataUser(request.UserId);

            ////delete System
            //await _systemService.DeleteListDataUser(request.UserId);

            ////delete Interaction
            //await _interactionService.DeleteListDataUser(request.UserId);

            ////delete Ordering
            //await _orderService.DeleteListDataUser(request.UserId);

            ////delete Course
            //await _lmsCourseService.DeleteListDataUser(request.UserId);

            //// delete User Otp Code
            //var userOtpCode = await _userOtpCodeRepository.Queryable
            //                                              .Where(x => x.UserId == request.UserId && x.IsDeleted != true)
            //                                              .ToListAsync(cancellationToken);
            //if (userOtpCode.Count != 0)
            //{
            //    await _userOtpCodeRepository.DeleteListAsync(userOtpCode);
            //    await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            //}

            //// delete User Setting
            //var userSetting = await _userSettingRepository.Queryable
            //                                              .Where(x => x.UserId == request.UserId && x.IsDeleted != true)
            //                                              .ToListAsync(cancellationToken);
            //if (userSetting.Count != 0)
            //{
            //    await _userSettingRepository.DeleteListAsync(userSetting);
            //    await _userSettingRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            //}

            //// delete user platform
            //var userPlatform = await _userPlatformRepository.Queryable
            //                                                .Where(x => x.UserId == request.UserId && x.IsDeleted != true)
            //                                                .ToListAsync(cancellationToken);
            //if (userPlatform.Count != 0)
            //{
            //    await _userPlatformRepository.DeleteListAsync(userPlatform);
            //    await _userPlatformRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            //}

            var student = await _studentRepository.Queryable
                                              .Include(x => x.ParentStudents)
                                              .Where(x => x.UserId == request.UserId)
                                              .FirstOrDefaultAsync(cancellationToken);

            if (student != null)
            {
                ////delete student ranking
                //var studentRankings = await _studentRankingRepository.Queryable
                //                                                  .Where(x => x.StudentId == human.Student!.Id)
                //                                                  .ToListAsync(cancellationToken);
                //if (studentRankings.Count != 0)
                //{
                //    await _studentRankingRepository.DeleteListAsync(studentRankings);
                //    await _studentRankingRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                //}

                //// delete student daily streak
                //var studentDailyStreak = await _studentDailyStreakRepository.Queryable
                //                                                            .Where(x => x.StudentId == human.Student!.Id)
                //                                                            .ToListAsync(cancellationToken);
                //if (studentDailyStreak.Count != 0)
                //{
                //    await _studentDailyStreakRepository.DeleteListAsync(studentDailyStreak);
                //    await _studentDailyStreakRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                //}

                //// delete student focus time

                //var studentFocusTime = await _studentFocusTimeRepository.Queryable
                //                                                        .Where(x => x.StudentId == human.Student!.Id)
                //                                                        .ToListAsync(cancellationToken);
                //if (studentFocusTime.Count != 0)
                //{
                //    await _studentFocusTimeRepository.DeleteListAsync(studentFocusTime);
                //    await _studentFocusTimeRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                //}

                // delete human va student va parent student
                await _studentRepository.DeleteAsync(student);
                await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }

            // delete user
            var user = await _userManager.Users.Where(x => x.Id == request.UserId).FirstOrDefaultAsync(cancellationToken);
            await _userManager.DeleteAsync(user!);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }

}
