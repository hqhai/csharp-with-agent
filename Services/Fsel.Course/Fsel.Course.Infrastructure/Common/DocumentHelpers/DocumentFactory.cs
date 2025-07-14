// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.DocumentHelpers
{
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.Documents;
    using Fsel.Course.Infrastructure.Common.ClassForumHelpers;

    public class DocumentFactory
    {
        private readonly CreateDocumentCommandModel _createRequest;

        public DocumentFactory(CreateDocumentCommandModel createRequest)
        {
            _createRequest = createRequest;
        }

        public Document Build(int version = 0, Guid? originalId = null)
        {
            var document = new Document
            {
                VersionStatus = EnumVersionStatus.LastVersion,
                Version = version,
                Files = _createRequest.Files
            };

            document.OriginalId = originalId.HasValue ? originalId.Value : document.Id;

            return document;
        }

        public static DocumentFactory Create(CreateDocumentCommandModel createRequest)
        {
            return new DocumentFactory(createRequest);
        }
    }
}
