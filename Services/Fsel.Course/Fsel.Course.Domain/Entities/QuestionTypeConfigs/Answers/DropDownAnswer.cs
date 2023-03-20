// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DropDownAnswer
    {
        public string? Name { get; set; }

        public IList<DropDownAnswers>? Answers { get; set; }

        public class DropDownAnswers
        {
            public int Id { get; set; }

            public bool IsChecked { get; set; }

            public bool IsExact { get; set; }
        }
    }
}
