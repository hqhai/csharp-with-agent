// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.QuestionHelper.TracingQuestionType.Interface
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;

    public interface ITracingQuestionType
    {
        Task<MethodResult<bool>> ExecuteAsync(CancellationToken cancellationToken);
    }
}
