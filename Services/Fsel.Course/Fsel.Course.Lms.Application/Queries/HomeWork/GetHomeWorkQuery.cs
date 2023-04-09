// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWork
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;

    public class GetHomeWorkQuery : IRequest<MethodResult<HomeWorkModel>>
    {
        public Guid HomeWorkId { get; set; }
        public Guid? LessonResultId { get; set; }
    }

    /* public class GetHomeWorkQueryHandler : IRequestHandler<GetHomeWorkQuery, MethodResult<HomeWorkModel>>
     {
         private readonly IMapper _mapper;
         private readonly IHomeWorkRepository _homeWorkRepository;
         private readonly QuestionTypeConverter _questionTypeConverter;
         private readonly AuthContext _authContext;
         private readonly IUserService _userService;

         public GetHomeWorkQueryHandler(IMapper mapper
             , IHomeWorkRepository homeWorkRepository
             , QuestionTypeConverter questionTypeConverter
             , AuthContext authContext

             , IUserService userService)
         {
             _mapper = mapper;
             _homeWorkRepository = homeWorkRepository;
             _questionTypeConverter = questionTypeConverter;
             _authContext = authContext;
             _userService = userService;
         }

         public async Task<MethodResult<HomeWorkModel>> Handle(GetHomeWorkQuery request, CancellationToken cancellationToken)
         {
             MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();

             var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId.ToString());
             if (studentsResult == null)
             {
                 methodResult.AddErrorBadRequest(nameof(EnumCourseClassStudentErrorCode.UserIdNotExist));
                 return methodResult;
             }
             var studentId = studentsResult.Content!.Result!.Id;
         }
     }*/
}
