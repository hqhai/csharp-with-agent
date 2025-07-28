// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCourseSettingCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveUserCourseSettingCommand : SaveUserCourseSettingQueueModel, IRequest<MethodResult<UserCourseSettingModel>>
    {
    }

    public class SaveUserCourseSettingCommandHandler : IRequestHandler<SaveUserCourseSettingCommand, MethodResult<UserCourseSettingModel>>
    {
        private readonly IUserCourseSettingRepository _userCourseSettingRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private const int MaxValue = 3;

        public SaveUserCourseSettingCommandHandler(IUserCourseSettingRepository userCourseSettingRepository, IMapper mapper, UserManager<User> userManager)
        {
            _userCourseSettingRepository = userCourseSettingRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<MethodResult<UserCourseSettingModel>> Handle(SaveUserCourseSettingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserCourseSettingModel>();
            var user = await _userManager.Users.Where(x => x.Id == request.UserId).FirstOrDefaultAsync(cancellationToken);
            if (user == null)
            {
                return methodResult;
            }
            var userCourseSetting = await _userCourseSettingRepository.Queryable.Where(x => !request.CourseLevel.HasValue || x.CourseLevel == request.CourseLevel)
                                                                                .FirstOrDefaultAsync(x => x.Type == request.Type && x.UserId == request.UserId, cancellationToken);
            if (userCourseSetting == null)
            {
                userCourseSetting = new UserCourseSetting
                {
                    CourseLevel = request.CourseLevel,
                    Value = request.IsDeduction ? MaxValue - 1 : MaxValue,
                    Type = request.Type,
                    UserId = request.UserId
                };
                _userCourseSettingRepository.Add(userCourseSetting);
            }
            else
            {
                userCourseSetting.Value = request.IsDeduction ? --userCourseSetting.Value : MaxValue;
                if (userCourseSetting.Value < 0)
                {
                    userCourseSetting.Value = 0;
                }
                _userCourseSettingRepository.Update(userCourseSetting);
            }
            await _userCourseSettingRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.Result = _mapper.Map<UserCourseSettingModel>(userCourseSetting);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
