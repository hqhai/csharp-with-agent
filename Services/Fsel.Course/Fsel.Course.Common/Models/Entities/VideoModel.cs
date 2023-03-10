using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Common.Models.Entities
{
    public class VideoModel : BaseEntityModel
    {
        public string? Name { get; set; }

        public string? VideoFilePath { get; set; }

        public bool IsActive { get; set; }

        public Guid? TeacherId { get; set; }

<<<<<<< Updated upstream
        public EnumCourseLevel CourseLevel { get; set; }
=======
        /// <summary>
        /// Time code information
        /// </summary>
        ///
        public Guid? VideoTimeCodeId { get; set; }

        public long DisplayTimeTicks { get; set; }

        public long ExecutionTimeTicks { get; set; }

        public EnumTimeCodeType TimeCodeType { get; set; }

        /// <summary>
        /// Excercise  information
        /// </summary>

        public Guid ExcerciseId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }

        public string? ExcerciseName { get; set; }

        public EnumQuestionType QuestionType { get; set; }

        public string? MediaPost { get; set; }

        /// <summary>
        ///  Question information
        /// </summary>
        public Guid QuestionId { get; set; }

        public EnumQuestionType EnumQuestionType { get; set; }

        public string? Config { get; set; }
>>>>>>> Stashed changes
    }
}
