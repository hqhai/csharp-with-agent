// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.TestRequestHandler
{
    using System.Threading.Tasks;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    public interface ITestRequestValidateHandler : IBaseTestRequestHandler
    {
    }
    public class TestRequestValidateHandler : BaseTestRequestHandler, ITestRequestValidateHandler
    {
        private readonly IRepository<TestSectionResult> _sectionResultRepository;

        public TestRequestValidateHandler(IRepository<TestSectionResult> sectionGroupResultRepository)
        {
            _sectionResultRepository = sectionGroupResultRepository;
        }

        public override async Task Handle(TestRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var request = context.TestRequestCommand;
            var sectionResult = await _sectionResultRepository.GetByIdAsync(request.SectionResultId);
            if (sectionResult == null)
            {
                context.MethodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionResult));
                return;
            }
            else if (sectionResult.Status == EnumResultStatus.Done)
            {
                context.MethodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.SectionGroupResultDone), nameof(sectionResult.Status));
                return;
            }

            context.TestSectionResult = sectionResult;

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
