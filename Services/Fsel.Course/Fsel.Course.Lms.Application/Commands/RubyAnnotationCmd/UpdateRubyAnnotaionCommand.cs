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
            var methodResult = new MethodResult<RubyAnnotationModel>();

            var rubyText = await _rubyAnnotaionRepository.GetByIdAsync(request.Id);

            if (rubyText == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(rubyText));
                return methodResult;
            }

            _mapper.Map(request, rubyText);

            if (!rubyText.IsValid())
            {
                methodResult.AddErrorBadRequest(rubyText.ErrorMessages);
                return methodResult;
            }

            await _rubyAnnotaionRepository.ExecuteTransactionAsync(async () =>
            {
                rubyText = _rubyAnnotaionRepository.Update(rubyText);
                await _rubyAnnotaionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<RubyAnnotationModel>(rubyText);
                return methodResult;
            });

            return methodResult;
        }
    }
}
