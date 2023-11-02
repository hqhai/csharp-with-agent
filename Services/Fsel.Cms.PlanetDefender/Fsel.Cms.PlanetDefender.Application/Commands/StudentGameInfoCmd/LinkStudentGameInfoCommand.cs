// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Commands.StudentGameInfoCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Application.Queues;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices;
    using Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.StudentGameInfos;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class LinkStudentGameInfoCommand : LinkStudentGameInfoCommandModel, IRequest<MethodResult<StudentGameInfoModel>>
    {
    }

    public class LinkStudentGameInfoCommandHandler : IRequestHandler<LinkStudentGameInfoCommand, MethodResult<StudentGameInfoModel>>
    {
        private readonly IStudentGameInfoRepository _studentGameInfoRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly DeleteGuestStudentPublisher _deleteGuestStudentPublisher;
        private readonly AuthContext _authContext;

        public LinkStudentGameInfoCommandHandler(IStudentGameInfoRepository studentGameInfoRepository
            , IMapper mapper
            , IUserService userService
            , DeleteGuestStudentPublisher deleteGuestStudentPublisher
            , AuthContext authContext)
        {
            _studentGameInfoRepository = studentGameInfoRepository;
            _mapper = mapper;
            _userService = userService;
            _deleteGuestStudentPublisher = deleteGuestStudentPublisher;
            _authContext = authContext;
        }

        public async Task<MethodResult<StudentGameInfoModel>> Handle(LinkStudentGameInfoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentGameInfoModel> methodResult = new MethodResult<StudentGameInfoModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);
            var student = studentResult.Content?.Result;

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var guestUserId = _authContext.CurrentUserId;

            var studentGuestResult = await _userService.GetStudentByUserIdAsync(guestUserId);
            var guestStudent = studentGuestResult.Content?.Result;
            if (guestStudent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(guestStudent));
                return methodResult;
            }

            var studentGameInfo = await _studentGameInfoRepository.Queryable.Where(x => x.StudentId == student.Id).FirstOrDefaultAsync(cancellationToken);

            if (request.IsChooseUser)
            {
                if (studentGameInfo != null)
                {
                    studentGameInfo.Level = (Shared.Enums.EnumGameCourseLevel)student.CourseLevel;
                    _studentGameInfoRepository.Update(studentGameInfo);
                }
                else
                {
                    studentGameInfo = new StudentGameInfo
                    {
                        Level = (Shared.Enums.EnumGameCourseLevel)student.CourseLevel!,
                        StudentId = student.Id
                    };
                    _studentGameInfoRepository.Add(studentGameInfo);
                }
            }
            else
            {
                if (studentGameInfo != null)
                {
                    studentGameInfo.Level = (Shared.Enums.EnumGameCourseLevel)student.CourseLevel;
                    _studentGameInfoRepository.Update(studentGameInfo);
                }
                else
                {
                    studentGameInfo = new StudentGameInfo
                    {
                        StudentId = guestStudent.Id,
                        Level = (Shared.Enums.EnumGameCourseLevel)guestStudent.CourseLevel
                    };

                    _studentGameInfoRepository.Add(studentGameInfo);
                }
            }
            var updateTokenStudent = await _userService.UpdateStudentByTokenAsync(new UpdateStudentByTokenModel { NumberOfToken = guestStudent.NumberOfToken, StudentId = student.Id });
            if (!updateTokenStudent.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(updateTokenStudent));
                return methodResult;
            }

            await _studentGameInfoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            await _deleteGuestStudentPublisher.Publish(guestUserId, cancellationToken).ConfigureAwait(false);

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
