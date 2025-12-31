// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.CategoryCmd
{
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Categories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateCategoryCommand : UpdateCategoryCommandModel, IRequest<MethodResult<CategoryModel>>
    {
    }

    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, MethodResult<CategoryModel>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private static readonly Regex s_regexCode = new Regex("^[a-zA-Z0-9]+$", RegexOptions.Compiled);

        public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository,
                                            IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CategoryModel>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CategoryModel> methodResult = new MethodResult<CategoryModel>();

            #region Validate

            var category = await _categoryRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (category == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(category), category);
                return methodResult;
            }

            if (request.ParentId.HasValue && !await _categoryRepository.Queryable.AnyAsync(x => x.Id == request.ParentId, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ParentId), request.ParentId);
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.Name))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.NameNotValid), nameof(request.Name), request.Name);
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.Code) || (!string.IsNullOrEmpty(request.Code) && !s_regexCode.IsMatch(request.Code)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCategoryErrorCode.CodeNotValid), nameof(request.Code), request.Code);
                return methodResult;
            }

            var checkCode = await _categoryRepository.Queryable.AnyAsync(x => x.Id != category.Id && x.Code == request.Code.Trim(), cancellationToken);
            if (checkCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            _mapper.Map(request, category);
            if (!category.IsValid())
            {
                methodResult.AddErrorBadRequest(category.ErrorMessages);
                return methodResult;
            }

            #endregion Validate

            await _categoryRepository.ExecuteTransactionAsync(async () =>
            {
                _categoryRepository.Update(category);
                await _categoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = _mapper.Map<CategoryModel>(category);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
