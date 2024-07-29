// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.GoogleSheetQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Newtonsoft.Json;

    public class GetListSchoolLuckySpinQuery : IRequest<MethodResult<IList<string>?>>
    {
    }

    public class GetListSchoolLuckySpinQueryHandler : IRequestHandler<GetListSchoolLuckySpinQuery, MethodResult<IList<string>?>>
    {
        public async Task<MethodResult<IList<string>?>> Handle(GetListSchoolLuckySpinQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<string>?>();

            string competitionConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.SchoolEventRules);
            string jsonData = File.ReadAllText(competitionConfigPath);
            var listSchoolEventRules = JsonConvert.DeserializeObject<IList<SchoolEventRule>>(jsonData);

            var schools = listSchoolEventRules?.Where(p => !string.IsNullOrEmpty(p.SchoolCode) && p.LuckySpin).Select(p => p.SchoolCode ?? string.Empty);

            methodResult.Result = schools?.ToList();
            return methodResult;
        }
    }
}
