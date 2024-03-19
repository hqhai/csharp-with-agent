// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.StudentTrialRegistration;

    public class StudentTrialRegistrationProfile : Profile
    {
        public StudentTrialRegistrationProfile()
        {
            CreateMap<StudentTrialRegistrationCommandModel, StudentTrialRegistration>().IgnoreAllNonExisting();
        }
    }
}
