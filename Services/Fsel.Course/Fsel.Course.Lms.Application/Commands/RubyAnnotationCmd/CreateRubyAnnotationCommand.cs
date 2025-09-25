// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.RubyAnnotationCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.RubyAnnotation;
    using Fsel.Course.Domain.Models.CommandModels.RubyScope;
    using Fsel.Course.Domain.Models.QueryModels.Ruby;
    using Fsel.Course.Infrastructure.Common.RubyHelpers;
    using Fsel.Course.Lms.Application.Services.RubyService;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using EntityRubyAnnotation = Domain.Entities.RubyAnnotation;
    using EntityRubyScope = Domain.Entities.RubyScope;

    public class CreateRubyAnnotationCommand : CreateRubyAnnotationCommandModel, IRequest<MethodResult<CreateRubyByScopeCommandModel>>
    {
    }

    public class RubyTextCommandHandler : IRequestHandler<CreateRubyAnnotationCommand, MethodResult<CreateRubyByScopeCommandModel>>
    {
        private readonly IRubyScopeRepository _rubyScopeRepository;
        private readonly IRubyAnnotationRepository _rubyAnnotationRepository;
        private readonly IMapper _mapper;
        private readonly IRubyService _rubyService;

        public RubyTextCommandHandler(IRubyAnnotationRepository rubyAnnotationRepository,
            IRubyScopeRepository rubyScopeRepository,
            IMapper mapper,
            IRubyService rubyService)
        {
            _rubyScopeRepository = rubyScopeRepository;
            _rubyAnnotationRepository = rubyAnnotationRepository;
            _mapper = mapper;
            _rubyService = rubyService;
        }

        public async Task<MethodResult<CreateRubyByScopeCommandModel>> Handle(CreateRubyAnnotationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CreateRubyByScopeCommandModel> methodResult = new MethodResult<CreateRubyByScopeCommandModel>();

            var baseNfc = string.Empty;

            if (!string.IsNullOrWhiteSpace(request.Text))
            {
                baseNfc = RubyTextNormalization.ToNfc(request.Text);
                _ = await _rubyScopeRepository.TryUpdateBaseTextAsync(request.HostType, request.HostId, request.FieldKey, baseNfc, cancellationToken);
            }
            else
            {
                var (found, nfc) = await _rubyScopeRepository.TryGetBaseTextAsync(request.HostType, request.HostId, request.FieldKey, cancellationToken);
                if (!found || string.IsNullOrEmpty(nfc))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    baseNfc = RubyTextNormalization.ToNfc(nfc!);
                }
            }

            var scope = await _rubyScopeRepository.Queryable
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.HostType == request.HostType && x.HostId == request.HostId && x.FieldKey == request.FieldKey, cancellationToken);

            if (scope == null)
            {
                await _rubyScopeRepository.ExecuteTransactionAsync(async () =>
                {
                    scope = new EntityRubyScope { Id = Guid.NewGuid(), FieldKey = request.FieldKey, HostId = request.HostId, HostType = request.HostType, Text = request.Text };
                    _rubyScopeRepository.Add(scope);
                    (scope.Text, scope.BaseLengthGraphemes) = _rubyService.Snapshot(baseNfc);
                    await _rubyScopeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    return methodResult;
                });
            }

            #region Validation
            if (request.LengthGraphemes is < 1 or > 5)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (request.StartGraphemeIndex < 0 || request.StartGraphemeIndex + request.LengthGraphemes > scope.BaseLengthGraphemes)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.MaxLength));
                return methodResult;
            }

            var rubyAnnotaiton = await _rubyAnnotationRepository.Queryable
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RubyScopeId == scope.Id && x.SelectedText == request.SelectedText && x.Phonetic == request.Phonetic, cancellationToken);
            if (rubyAnnotaiton != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }
            #endregion

            var rubyText = _mapper.Map<EntityRubyAnnotation>(request);
            rubyText.SelectedText = RubyTextNormalization.ToNfc(request.SelectedText);
            rubyText.RubyScopeId = scope.Id;
            rubyText.LanguageType = request.LangueType;

            if (rubyText == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _rubyAnnotationRepository.ExecuteTransactionAsync(async () =>
            {
                rubyText = _rubyAnnotationRepository.Add(rubyText);
                await _rubyAnnotationRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var ruby = await _rubyAnnotationRepository.Queryable
                    .AsNoTracking()
                    .Where(x => x.RubyScopeId == scope.Id && !x.IsDeleted)
                    .OrderBy(x => x.StartGraphemeIndex)
                    .ToListAsync(cancellationToken);
                var html = _rubyService.RenderHtml(baseNfc, ruby);

                var result = new CreateRubyByScopeCommandModel
                {
                    RubyId = rubyText.Id,
                    ScopeId = scope.Id,
                    Html = request.RubyReturn?.Html != false ? html : null,
                };

                if (request.RubyReturn?.Annotations == true)
                {
                    result.Annotations = await _rubyAnnotationRepository.Queryable
                    .AsNoTracking()
                    .Where(x => x.RubyScopeId == scope.Id && !x.IsDeleted)
                    .OrderBy(x => x.StartGraphemeIndex)
                    .Select(x => new RubyAnnotaionModel
                    {
                        Id = x.Id,
                        StartGraphemeIndex = x.StartGraphemeIndex,
                        LengthGraphemes = x.LengthGraphemes,
                        SelectedText = x.SelectedText,
                        Phonetic = x.Phonetic,
                        LanguageType = x.LanguageType
                    }).ToListAsync(cancellationToken);
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = result;

                return methodResult;
            });

            return methodResult;
        }
    }
}
