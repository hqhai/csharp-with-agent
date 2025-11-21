// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Domain.Models.EntityModels.V1i2;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.Documents;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;

    public class DocumentProfile : Profile
    {
        public DocumentProfile()
        {
            CreateMap<Document, DocumentModel>().IgnoreAllNonExisting();
            CreateMap<DocumentFile, DocumentFileModel>().IgnoreAllNonExisting();
            CreateMap<CreateDocumentCommandModel, Document>().IgnoreAllNonExisting();
            CreateMap<CreateDocumentResultModel, DocumentResult>().IgnoreAllNonExisting();
            CreateMap<DocumentResult, DocumentResultModel>().IgnoreAllNonExisting();

        }
    }
}
