using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Common.Helpers;
using Fsel.Training.Doman.IRepositories;
using MediatR;

namespace Fsel.Training.Application.Queries.TrainingQuery
{
    public class GetNewTrainingCodeQuery : IRequest<MethodResult<string>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class GetNewTrainingCodeQueryHandler : IRequestHandler<GetNewTrainingCodeQuery, MethodResult<string>>
    {
        private readonly ITrainingRepository _trainingRepository;

        public GetNewTrainingCodeQueryHandler(ITrainingRepository trainingRepository)
        {
            _trainingRepository = trainingRepository;
        }

        public async Task<MethodResult<string>> Handle(GetNewTrainingCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<string> methodResult = new MethodResult<string>();

            var weekString = $"{DateTimeHelper.ConvertInt():000}";
            int currentYear = DateTime.Now.Year;
            int lastTwoDigitsOfYear = currentYear % 100;
            int lastDigitOfYear = lastTwoDigitsOfYear % 10;
            var stt = $"{_trainingRepository.Queryable.Count():000}";
            var level = EnumHelper.GetTrainingCodeByEnumCourseLevel(request.CourseLevel);

            string codeTraining = $"{level}_{weekString}{stt}{lastDigitOfYear}S";
            methodResult.Result = codeTraining;
            return methodResult;
        }
    }
}
