using Fsel.Class.Doman.IRepositories;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using MediatR;

namespace Fsel.Class.Application.Queries.ClassQuery
{
    public class GetNewClassCodeQuery : IRequest<MethodResult<string>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class GetNewClassCodeQueryHandler : IRequestHandler<GetNewClassCodeQuery, MethodResult<string>>
    {
        private readonly IClassRepository _classRepository;

        public GetNewClassCodeQueryHandler(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public async Task<MethodResult<string>> Handle(GetNewClassCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<string> methodResult = new MethodResult<string>();

            var weekString = $"{DateTimeHelper.ConvertInt():000}";
            int currentYear = DateTime.Now.Year;
            int lastTwoDigitsOfYear = currentYear % 100;
            int lastDigitOfYear = lastTwoDigitsOfYear % 10;
            var stt = $"{_classRepository.Queryable.Count():000}";
            var level = EnumHelper.GetClassCodeByEnumCourseLevel(request.CourseLevel);

            string codeClass = $"{level}_{weekString}{stt}{lastDigitOfYear}S";
            methodResult.Result = codeClass;
            return methodResult;
        }
    }
}
