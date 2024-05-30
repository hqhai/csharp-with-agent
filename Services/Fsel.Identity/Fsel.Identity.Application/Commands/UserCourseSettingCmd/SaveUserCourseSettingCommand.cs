// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCourseSettingCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SaveUserCourseSettingCommand : IRequest<MethodResult<UserCourseSettingModel>>
    {
        public EnumUserCourseType Type { get; set; }
        public bool IsDeduction { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid UserId { get; set; }
    }

    public class SaveUserCourseSettingCommandHandler : IRequestHandler<SaveUserCourseSettingCommand, MethodResult<UserCourseSettingModel>>
    {
        private readonly IUserCourseSettingRepository _userCourseSettingRepository;
        private readonly IMapper _mapper;
        private const int MaxValue = 3;

        public SaveUserCourseSettingCommandHandler(IUserCourseSettingRepository userCourseSettingRepository, IMapper mapper)
        {
            _userCourseSettingRepository = userCourseSettingRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserCourseSettingModel>> Handle(SaveUserCourseSettingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserCourseSettingModel>();
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
                _userCourseSettingRepository.Update(userCourseSetting);
            }
            await _userCourseSettingRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.Result = _mapper.Map<UserCourseSettingModel>(userCourseSetting);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
