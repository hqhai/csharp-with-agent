// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentByUserIdsQuery : IRequest<MethodResult<IList<StudentModel>>>
    {
        public IList<string>? UserIds { get; set; }
    }

    public class GetStudentByUserIdsQueryHandler : IRequestHandler<GetStudentByUserIdsQuery, MethodResult<IList<StudentModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;

        public GetStudentByUserIdsQueryHandler(IMapper mapper, IStudentRepository studentRepository)
        {
            _mapper = mapper;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<IList<StudentModel>>> Handle(GetStudentByUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentModel>> methodResult = new MethodResult<IList<StudentModel>>();

            var students = await _studentRepository.Queryable
                                        .Include(i => i.Human)
                                        .Where(i => i.Human != null && request.UserIds!.Contains(i.Human.UserId!))
                                        .Select(x => new StudentModel
                                        {
                                            Membership = x.Membership,
                                            ClassId = x.ClassId,
                                            CourseLevel = x.CourseLevel,
                                            CreatedDate = x.CreatedDate,
                                            HumanId = x.HumanId,
                                            School = x.School,
                                            Human = _mapper.Map<HumanProfileModel>(x),
                                            UserId = x.Human!.UserId,
                                        }).ToListAsync(cancellationToken);

            if (students == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.StudentsNotExist));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
