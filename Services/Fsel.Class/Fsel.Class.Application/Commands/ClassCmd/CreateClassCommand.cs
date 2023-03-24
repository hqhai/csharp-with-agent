using AutoMapper;
using Fsel.Class.Doman.Models.CommandModels.Classes;
using Fsel.Class.Doman.Models.EntityModels;
using Fsel.Common.ActionResults;
using MediatR;

namespace Fsel.Class.Application.Commands.ClassCmd
{
    public class CreateClassCommand : CreateClassCommandModel, IRequest<MethodResult<ClassModel>>
    {
    }

    public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, MethodResult<ClassModel>>
    {
        private readonly IMapper _mapper;

        public CreateClassCommandHandler(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassModel>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
        {
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            return methodResult;
        }
    }
}
