using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Entities;

namespace Fsel.Course.Common.Models.Entities
{
    public class VideoModel : BaseEntityModel
    {
        /// <summary>
        /// Tên Video
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Link Video
        /// </summary>
        public string? VideoFilePath { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt
        /// </summary>
        public bool IsActive { get; set; }

        public EnumVideoType Type { get; set; }

        /// <summary>
        /// Giáo Viên ID
        /// </summary>
        public Guid? TeacherId { get; set; }

        /// <summary>
        /// Trình độ khóa
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        public List<VideoTimeCodeModel>? VideoTimeCodes { get; set; }
    }
}