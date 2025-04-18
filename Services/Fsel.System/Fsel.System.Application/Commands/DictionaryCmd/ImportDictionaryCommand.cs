// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.DictionaryCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.Dictionaries;
    using global::System.Text.Json;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ImportDictionaryCommand : BaseImportCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ImportDictionaryCommandHandler : IRequestHandler<ImportDictionaryCommand, MethodResult<bool>>
    {
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IMapper _mapper;

        public ImportDictionaryCommandHandler(IDictionaryRepository dictionaryRepository, IMapper mapper)
        {
            _dictionaryRepository = dictionaryRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(ImportDictionaryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.FormFile == null || request.FormFile.Length == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.FormFile));
                return methodResult;
            }

            using var reader = new StreamReader(request.FormFile.OpenReadStream());

            var jsonString = await reader.ReadToEndAsync(cancellationToken);
            var dictionaryList = JsonSerializer.Deserialize<IList<ImportDictionaryCommandModel>>(jsonString);

            var dictionary = _mapper.Map<IList<Dictionary>>(dictionaryList);

            await _dictionaryRepository.ExecuteTransactionAsync(async () =>
            {
                if (dictionary.Count >= 20000)
                {
                    var dictionaryListChunk = dictionary.Chunk(10000);
                    foreach (var chunk in dictionaryListChunk)
                    {
                        await _dictionaryRepository.AddList(chunk);
                    }
                }
                else
                {
                    await _dictionaryRepository.AddList(dictionary);
                }

                await _dictionaryRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;

                return methodResult;
            });

            return methodResult;
        }
    }
}
