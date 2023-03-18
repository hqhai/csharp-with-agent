using System.ComponentModel.DataAnnotations.Schema;

namespace Fsel.Common.ActionResults
{
    [NotMapped]
    public class ErrorResult
    {
        public string? ErrorCode { get; set; }

        public IList<Error> Errors { get; set; }

        public ErrorResult()
        {
            Errors = new List<Error>();
        }
    }

    [NotMapped]
    public class Error
    {
        public Error()
        {
            ErrorValues = new List<object>();
            ExactValues = new List<object>();
        }

        public string? ErrorField { get; set; }
        public IList<object> ErrorValues { get; set; }
        public IList<object> ExactValues { get; set; }
    }
}
