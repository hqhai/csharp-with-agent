// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CategoryCmd
{
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Categories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateCategoryCommand : CreateCategoryCommandModel, IRequest<MethodResult<CategoryModel>>
    {
    }

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, MethodResult<CategoryModel>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private static readonly Regex s_regexCode = new Regex("^[a-zA-Z0-9]+$", RegexOptions.Compiled);
        private static readonly Regex s_regexName = new Regex("^[a-zA-Z0-9_]{1,199}$", RegexOptions.Compiled);

        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository,
                                            IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CategoryModel>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CategoryModel> methodResult = new MethodResult<CategoryModel>();

            #region Validate
            if (request.ParentId.HasValue && !await _categoryRepository.Queryable.AnyAsync(x => x.Id == request.ParentId, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ParentId), request.ParentId);
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.Name) || (!string.IsNullOrEmpty(request.Name) && !s_regexName.IsMatch(request.Name)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.NameNotValid), nameof(request.Name), request.Name);
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.Code) || (!string.IsNullOrEmpty(request.Code) && !s_regexCode.IsMatch(request.Code)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.CodeNotValid), nameof(request.Code), request.Code);
                return methodResult;
            }

            var checkCode = await _categoryRepository.Queryable.AnyAsync(x => x.Code == request.Code.Trim(), cancellationToken);
            if (checkCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            var category = _mapper.Map<Category>(request);
            if (!category.IsValid())
            {
                methodResult.AddErrorBadRequest(category.ErrorMessages);
                return methodResult;
            }
            #endregion

            await _categoryRepository.ExecuteTransactionAsync(async () =>
            {
                category.Type = EnumTypeCategory.Subject;

                _categoryRepository.Add(category);
                await _categoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<CategoryModel>(category);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }
    }
}
