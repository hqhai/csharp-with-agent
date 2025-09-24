// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.RubyAnnotationCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Fsel.Course.Domain.Models.CommandModels.RubyAnnotation;

    public class UpdateRubyAnnotationCommand : UpdateRubyAnnotationCommandModel, IRequest<MethodResult<RubyAnnotationModel>>
    {
    }

    public class UpdateRubyTextCommandHanlder : IRequestHandler<UpdateRubyAnnotationCommand, MethodResult<RubyAnnotationModel>>
    {
        private readonly IRubyAnnotationRepository _rubyAnnotaionRepository;
        private readonly IMapper _mapper;

        public UpdateRubyTextCommandHanlder(IRubyAnnotationRepository rubyAnnotaionRepository, IMapper mapper)
        {
            _rubyAnnotaionRepository = rubyAnnotaionRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<RubyAnnotationModel>> Handle(UpdateRubyAnnotationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<RubyAnnotationModel> methodResult = new MethodResult<RubyAnnotationModel>();

            var rubyAnnotation = await _rubyAnnotaionRepository.GetByIdAsync(request.Id);

            if (rubyAnnotation == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(rubyAnnotation));
                return methodResult;
            }

            _mapper.Map(request, rubyAnnotation);

            if (!rubyAnnotation.IsValid())
            {
                methodResult.AddErrorBadRequest(rubyAnnotation.ErrorMessages);
                return methodResult;
            }

            await _rubyAnnotaionRepository.ExecuteTransactionAsync(async () =>
            {
                rubyAnnotation = _rubyAnnotaionRepository.Update(rubyAnnotation);
                await _rubyAnnotaionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<RubyAnnotationModel>(rubyAnnotation);

                return methodResult;
            });

            return methodResult;
        }
    }
}
