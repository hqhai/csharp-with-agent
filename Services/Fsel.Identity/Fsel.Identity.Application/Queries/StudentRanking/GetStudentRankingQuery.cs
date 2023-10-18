// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentRanking

{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentRankingQuery : BaseQueryModel, IRequest<MethodResult<List<StudentRankingModel>>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class GetStudentRankingQueryHandler : IRequestHandler<GetStudentRankingQuery, MethodResult<List<StudentRankingModel>>>
    {
        private readonly IStudentRankingRepository _studentRankingRepository;
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;
        public GetStudentRankingQueryHandler(IStudentRankingRepository studentRankingRepository, IMapper mapper, IStudentRepository studentRepository)
        {
            _studentRankingRepository = studentRankingRepository;
            _mapper = mapper;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<List<StudentRankingModel>>> Handle(GetStudentRankingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<StudentRankingModel>> methodResult = new MethodResult<List<StudentRankingModel>>();

            var studentRankingsQuery = await _studentRankingRepository.Queryable.Where(x => x.CourseLevel == request.CourseLevel).OrderBy(x => x.CurrentPosition).ToListAsync(cancellationToken);

            var studentIds = studentRankingsQuery.Select(s => s.StudentId);
            var studentInfo = _studentRepository.Queryable.Include(x => x.Human).Where(x => studentIds.Contains(x.Id)).ToList();
            var studentRankingResult = _mapper.Map<List<StudentRankingModel>>(studentRankingsQuery);

            studentRankingResult.ForEach(x =>
            {
                var student = studentInfo.FirstOrDefault(s => s.Id == x.StudentId);
                if (student != null)
                {
                    x.FullName = student.Human?.FullName;
                    x.AvatarPath = student.Human?.AvatarPath;
                }
            });

            methodResult.Result = studentRankingResult;
            methodResult.Result = _mapper.Map<List<StudentRankingModel>>(studentRankingResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
