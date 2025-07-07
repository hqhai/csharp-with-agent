// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Commands.TestCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateTestCommand : UpdateTestCommandModel, IRequest<MethodResult<TestModel>>
    {
    }

    public class UpdateTestConfigCommandHandler : IRequestHandler<UpdateTestCommand, MethodResult<TestModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITestRepository _testRepository;
        private readonly ITestSectionRepository _testSectionRepository;
        private readonly ITestSectionQuestionRepository _testSectionQuestionRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionConverter _questionConverter;

        public UpdateTestConfigCommandHandler(IMapper mapper
            , ITestRepository testRepository
            , ITestSectionRepository testSectionRepository
            , ITestSectionQuestionRepository testSectionQuestionRepository
            , IQuestionRepository questionRepository
            , QuestionConverter questionConverter)
        {
            _mapper = mapper;
            _testRepository = testRepository;
            _testSectionRepository = testSectionRepository;
            _testSectionQuestionRepository = testSectionQuestionRepository;
            _questionRepository = questionRepository;
            _questionConverter = questionConverter;
        }

        public async Task<MethodResult<TestModel>> Handle(UpdateTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TestModel>();

            var test = await _testRepository.GetIncludeByIdAsync(request.Id);
            if (test == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            var codeExists = await _testRepository.Queryable.AnyAsync(x => x.Code == request.Code && x.Id != request.Id, cancellationToken);
            if (codeExists)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            _mapper.Map(request, test);
            if (!test.IsValid())
            {
                methodResult.AddErrorBadRequest(test.ErrorMessages);
                return methodResult;
            }

            // Bắt đầu transaction
            await _testRepository.ExecuteTransactionAsync(async () =>
            {
                // Cập nhật Test
                _testRepository.Update(test);
                await _testRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<TestModel>(test);
                return methodResult;
            });
            return methodResult;
        }

        //private async Task<IList<TestSection>> GetTestSections(IList<TestSection> testSections, Test? test = null)
        //{
        //    if (test != null)
        //    {
        //        var listTestSection = await _testSectionRepository.Queryable.Include(x => x.TestSections).Where(x => x.TestId == test.Id).ToListAsync();
        //        foreach (var testSection in listTestSection)
        //        {
        //            if(testSection.t)

        //        }
        //    }
        //}
    }
}
