// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    public class LiveSessionInformationModel
    {
        public string? ClassName { get; set; }
        public string? TeacherName { get; set; }
        public string? Description { get; set; }
        public IList<InforStudentModel>? Students { get; set; }
    }

    public class InforStudentModel
    {
        public string? StudentName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}
