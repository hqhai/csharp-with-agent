// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.QuestionHelper.QuestionTypes.Interface
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;

    public interface IQuestionType
    {
        Task<MethodResult<bool>> ExecuteAsync(CancellationToken cancellationToken);
    }
}
