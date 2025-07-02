// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteUserCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
        public bool IsHashDelete { get; set; }
    }

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, MethodResult<bool>>
    {
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IUserSettingRepository _userSettingRepository;
        private readonly IUserPlatformRepository _userPlatformRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentRankingRepository _studentRankingRepository;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;
        private readonly IStudentFocusTimeRepository _studentFocusTimeRepository;
        private readonly UserManager<User> _userManager;

        public DeleteUserCommandHandler(IUserOtpCodeRepository userOtpCodeRepository
                                                 , IUserSettingRepository userSettingRepository
                                                 , IUserPlatformRepository userPlatformRepository
                                                 , IStudentRepository studentRepository
                                                 , IStudentRankingRepository studentRankingRepository
                                                 , IStudentDailyStreakRepository studentDailyStreakRepository
                                                 , IStudentFocusTimeRepository studentFocusTimeRepository
                                                 , UserManager<User> userManager)
        {
            _userOtpCodeRepository = userOtpCodeRepository;
            _userSettingRepository = userSettingRepository;
            _userPlatformRepository = userPlatformRepository;
            _studentRepository = studentRepository;
            _studentRankingRepository = studentRankingRepository;
            _studentDailyStreakRepository = studentDailyStreakRepository;
            _studentFocusTimeRepository = studentFocusTimeRepository;
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }

            // delete User Otp Code
            var userOtpCode = await _userOtpCodeRepository.Queryable
                                                          .Where(x => x.UserId == user.Id)
                                                          .ToListAsync(cancellationToken);
            if (userOtpCode.Count != 0)
            {
                await _userOtpCodeRepository.DeleteListAsync(userOtpCode);
                await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(true, !request.IsHashDelete, cancellationToken);
            }

            // delete User Setting
            var userSetting = await _userSettingRepository.Queryable
                                                          .Where(x => x.UserId == user.Id)
                                                          .ToListAsync(cancellationToken);
            if (userSetting.Count != 0)
            {
                await _userSettingRepository.DeleteListAsync(userSetting);
                await _userSettingRepository.UnitOfWork.SaveChangesAsync(true, !request.IsHashDelete, cancellationToken);
            }

            // delete user platform
            var userPlatform = await _userPlatformRepository.Queryable
                                                            .Where(x => x.UserId == user.Id)
                                                            .ToListAsync(cancellationToken);
            if (userPlatform.Count != 0)
            {
                await _userPlatformRepository.DeleteListAsync(userPlatform);
                await _userPlatformRepository.UnitOfWork.SaveChangesAsync(true, !request.IsHashDelete, cancellationToken);
            }

            var student = await _studentRepository.Queryable
                                              .Include(x => x.ParentStudents)
                                              .Where(x => x.UserId == user.Id)
                                              .FirstOrDefaultAsync(cancellationToken);

            if (student != null)
            {
                //delete student ranking
                var studentRankings = await _studentRankingRepository.Queryable
                                                                  .Where(x => x.StudentId == student.Id)
                                                                  .ToListAsync(cancellationToken);
                if (studentRankings.Count != 0)
                {
                    await _studentRankingRepository.DeleteListAsync(studentRankings);
                    await _studentRankingRepository.UnitOfWork.SaveChangesAsync(true, !request.IsHashDelete, cancellationToken);
                }

                // delete student daily streak
                var studentDailyStreak = await _studentDailyStreakRepository.Queryable
                                                                            .Where(x => x.StudentId == student.Id)
                                                                            .ToListAsync(cancellationToken);
                if (studentDailyStreak.Count != 0)
                {
                    await _studentDailyStreakRepository.DeleteListAsync(studentDailyStreak);
                    await _studentDailyStreakRepository.UnitOfWork.SaveChangesAsync(true, !request.IsHashDelete, cancellationToken);
                }

                // delete student focus time

                var studentFocusTime = await _studentFocusTimeRepository.Queryable
                                                                        .Where(x => x.StudentId == student.Id)
                                                                        .ToListAsync(cancellationToken);
                if (studentFocusTime.Count != 0)
                {
                    await _studentFocusTimeRepository.DeleteListAsync(studentFocusTime);
                    await _studentFocusTimeRepository.UnitOfWork.SaveChangesAsync(true, !request.IsHashDelete, cancellationToken);
                }

                await _studentRepository.DeleteAsync(student);
                await _studentRepository.UnitOfWork.SaveChangesAsync(true, !request.IsHashDelete, cancellationToken);
            }

            await _userManager.DeleteAsync(user, !request.IsHashDelete);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
