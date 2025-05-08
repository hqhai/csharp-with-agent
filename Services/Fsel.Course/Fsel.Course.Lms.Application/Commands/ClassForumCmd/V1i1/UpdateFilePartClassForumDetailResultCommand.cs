// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumCmd.V1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.StorageServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Refit;

    public class UpdateFilePartClassForumDetailResultCommand : INotification
    {
        public Guid Id { get; set; }

        public IFormFile? FormFile { get; set; }
    }

    public class UpdateFilePartClassForumDetailResultCommandHandler : INotificationHandler<UpdateFilePartClassForumDetailResultCommand>
    {
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly IStorageService _storageService;

        public UpdateFilePartClassForumDetailResultCommandHandler(IClassForumDetailResultRepository classForumDetailResultRepository,
                                                                  IStorageService storageService)
        {
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _storageService = storageService;
        }

        public async Task Handle(UpdateFilePartClassForumDetailResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.FormFile);

            var classForumDetailResult = await _classForumDetailResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (classForumDetailResult == null)
            {
                return;
            }

            using var stream = request.FormFile.OpenReadStream();
            var streamPart = new StreamPart(stream, request.FormFile.FileName, request.FormFile.ContentType);

            var filePart = await _storageService.ConvertWav(streamPart);
            if (!filePart.IsSuccessStatusCode)
            {
                return;
            }

            classForumDetailResult.ClassForumResultFiles = new List<ClassForumResultFile> { new ClassForumResultFile { FilePath = filePart.Content?.Result } };
            _classForumDetailResultRepository.Update(classForumDetailResult);
            await _classForumDetailResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }
    }
}
