// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUserByIdQuery : IRequest<MethodResult<HumanModel>>
    {
        public Guid? Id { get; set; }
    }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, MethodResult<HumanModel>>
    {
        private readonly IHumanRepository _humanRepository;
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;

        public GetUserByIdQueryHandler(IHumanRepository humanRepository, IMapper mapper, IStudentRepository studentRepository)
        {
            _humanRepository = humanRepository;
            _mapper = mapper;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<HumanModel>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HumanModel> methodResult = new MethodResult<HumanModel>();

            var human = await _humanRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == request.Id, cancellationToken);
            if (human == null)
            {
                return methodResult;
            }

            var result = _mapper.Map<HumanModel>(human);

            var student = await _studentRepository.Queryable.FirstOrDefaultAsync(p => p.HumanId == human.Id, cancellationToken);
            if (student != null)
            {
                result.CourseId = student.CourseId;
            }
            methodResult.Result = result;
            return methodResult;
        }
    }
}
