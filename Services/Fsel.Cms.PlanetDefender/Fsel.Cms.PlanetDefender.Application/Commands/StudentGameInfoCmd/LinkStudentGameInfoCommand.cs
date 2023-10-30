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
    using Fsel.Common.Models;
    using Fsel.Core.Base.BaseModels;
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

        public LinkStudentGameInfoCommandHandler(IStudentGameInfoRepository studentGameInfoRepository, IMapper mapper, IUserService userService, DeleteGuestStudentPublisher deleteGuestStudentPublisher)
        {
            _studentGameInfoRepository = studentGameInfoRepository;
            _mapper = mapper;
            _userService = userService;
            _deleteGuestStudentPublisher = deleteGuestStudentPublisher;
        }

        public async Task<MethodResult<StudentGameInfoModel>> Handle(LinkStudentGameInfoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentGameInfoModel> methodResult = new MethodResult<StudentGameInfoModel>();

            var guestStudentResult = await _userService.ExecuteListStudentQueryAsync(new BaseQueryModel
            {
                Filters = new List<GenericFilterModel>
                {
                    new GenericFilterModel
                    {
                        Property = nameof(StudentModel.Human.UserId),
                        Value = request.GuestUserId,
                        Operator = Common.Enums.EnumFilterOperator.Equal
                    }
                }
            });
            var guestStudent = guestStudentResult.Content?.Result;

            var studentResult = await _userService.ExecuteListStudentQueryAsync(new BaseQueryModel
            {
                Filters = new List<GenericFilterModel>
                {
                    new GenericFilterModel
                    {
                        Property = nameof(StudentModel.Human.UserId),
                        Value = request.UserId,
                        Operator = Common.Enums.EnumFilterOperator.Equal
                    }
                }
            });
            var student = studentResult.Content?.Result;

            var guestStudentId = guestStudent?.Where(x => x.Human?.UserId == request.GuestUserId).Select(x => x.Id).FirstOrDefault();

            var guestStudentGameId = await _studentGameInfoRepository.Queryable.Where(x => x.StudentId == guestStudentId).FirstOrDefaultAsync(cancellationToken);

            if (guestStudentGameId == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(guestStudentGameId));
                return methodResult;
            }

            var studentId = await _studentGameInfoRepository.Queryable.Where(x => x.StudentId == request.UserId).FirstOrDefaultAsync(cancellationToken);

            if (request.IsChooseUser)
            {
                var level = student?.Where(x => x.Id == request.UserId).Select(x => x.CourseLevel).FirstOrDefault();

                studentId = new StudentGameInfo
                {
                    Level = (Shared.Enums.EnumGameCourseLevel)level!,
                    StudentId = request.UserId
                };
                _studentGameInfoRepository.Add(studentId);
            }
            else
            {
                var guestLevel = guestStudent?.Where(x => x.Id == request.GuestUserId).Select(x => x.CourseLevel).FirstOrDefault();

                studentId = new StudentGameInfo
                {
                    StudentId = request.UserId,
                    Level = guestStudentGameId.Level
                };
                var updateTokenStudent = await _userService.UpdateStudentByTokenAsync(new Services.UserServices.Models.UpdateStudentByTokenModel { NumberOfToken = student!.Where(x => x.Id == request.UserId).Select(x => x.NumberOfToken).FirstOrDefault(), StudentId = request.UserId });
                if (!updateTokenStudent.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(updateTokenStudent));
                    return methodResult;
                }
                _studentGameInfoRepository.Add(studentId);
            }
            await _studentGameInfoRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            await _deleteGuestStudentPublisher.Publish(guestStudentGameId, cancellationToken).ConfigureAwait(false);

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
