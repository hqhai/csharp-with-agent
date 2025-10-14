// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class SubjectModel
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public List<SubjectModel> ChildSubjects { get; set; } = new List<SubjectModel>();
    }
}
