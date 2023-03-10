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
        public string? Name { get; set; }

        public string? VideoFilePath { get; set; }

        public bool IsActive { get; set; }

        public Guid? TeacherId { get; set; }

        /// <summary>
        /// Time code information
        /// </summary>
        ///
       /* public Guid? Time*/
        public long DisplayTimeTicks { get; set; }

        public long ExecutionTimeTicks { get; set; }

        public EnumTimeCodeType TimeCodeType { get; set; }

        /// <summary>
        /// Excercise and Question information
        /// </summary>

        public EnumCourseLevel CourseLevel { get; set; }

        public string? ExcerciseName { get; set; }

        public EnumQuestionType QuestionType { get; set; }

        public string? MediaPost { get; set; }

        public EnumQuestionType EnumQuestionType { get; set; }

        public string? Config { get; set; }
    }
}