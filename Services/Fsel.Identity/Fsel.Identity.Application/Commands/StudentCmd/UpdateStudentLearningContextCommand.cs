// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentLearningContextCommand : IRequest<MethodResult<StudentModel>>
    {
        public Guid? CourseId { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? SubjectId { get; set; }
        public Guid? LevelId { get; set; }
    }

    public class UpdateStudentLearningContextCommandHandler : IRequestHandler<UpdateStudentLearningContextCommand, MethodResult<StudentModel>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public UpdateStudentLearningContextCommandHandler(IStudentRepository studentRepository, AuthContext authContext, IMapper mapper)
        {
            _studentRepository = studentRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<StudentModel>> Handle(UpdateStudentLearningContextCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentModel>();

            var student = await _studentRepository.Queryable
                                                  .FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId, cancellationToken: cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            student.CourseId = request.CourseId;
            student.ProgramId = request.ProgramId;
            student.SubjectId = request.SubjectId;
            student.LevelId = request.LevelId;
            try
            {
                await _studentRepository.BulkUpdateList(new[] { student }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new
                    {
                        entity.CourseId,
                        entity.ProgramId,
                        entity.SubjectId,
                        entity.LevelId
                    };
                });
            }
            catch
            {
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<StudentModel>(student);
            return methodResult;
        }
    }
}
