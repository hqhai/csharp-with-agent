using Fsel.Common.Helpers;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoSubFilePathModel
    {
        public string? Language { get; set; }

        private string? _subFilePath;

        public string? SubFilePath
        {
            set { _subFilePath = value; }
            get { return _subFilePath.AddS3BaseUrl(); }
        }
    }
}
