using System.ComponentModel.DataAnnotations.Schema;

namespace Fsel.Common.ActionResults
{
    [NotMapped]
    public class ErrorResult
    {
        public string? ErrorCode { get; set; }

        public string? ErrorMessage { get; set; }

        public List<string> ErrorValues { get; set; }

        public ErrorResult()
        {
            ErrorValues = new List<string>();
        }

        public override string ToString()
        {
            if (ErrorValues != null && ErrorValues.Count > 0)
            {
                return "[" + ErrorCode + ": " + ErrorMessage + " (" + string.Join(',', ErrorValues) + ")]";
            }

            return "[" + ErrorCode + ": " + ErrorMessage + "]";
        }
    }
}