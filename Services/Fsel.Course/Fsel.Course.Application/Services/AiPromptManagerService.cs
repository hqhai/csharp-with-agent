// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services
{
    using AutoMapper;
    using Domain.Entities;
    using Domain.IRepositories;
    using Fsel.Common.Enums;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IAiPromptManagerService
    {
        Task<AiPromptManager> CreateNewVersionAsync(Guid originalAiPromptManagerId, CancellationToken cancellationToken);
        Task<AiPromptManager> CreateNewVersionWithUpdatedCriteriaAsync(
            Guid originalAiPromptManagerId,
            AICriteriaConfigs updatedCriteria, CancellationToken cancellationToken);
        Task<bool> HasDependenciesAsync(Guid aiCriteriaConfigId);
    }

    public class AiPromptManagerService : IAiPromptManagerService
    {
        private readonly IAiPromptManagerRepository _aiPromptManagerRepository;
        private readonly IAiCriteriaConfigRepository _aiCriteriaConfigRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AiPromptManagerService(
            IAiPromptManagerRepository aiPromptManagerRepository,
            IAiCriteriaConfigRepository aiCriteriaConfigRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _aiPromptManagerRepository = aiPromptManagerRepository;
            _aiCriteriaConfigRepository = aiCriteriaConfigRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AiPromptManager> CreateNewVersionAsync(Guid originalAiPromptManagerId, CancellationToken cancellationToken)
        {
            // Lấy phiên bản hiện tại
            var currentVersion = await _aiPromptManagerRepository.Queryable.FirstOrDefaultAsync(x => x.Id == originalAiPromptManagerId, cancellationToken);
            if (currentVersion == null)
            {
                return new AiPromptManager();
            }
            // Đánh dấu phiên bản hiện tại là cũ
            currentVersion.VersionStatus = EnumVersionStatus.OldVersion;
            _aiPromptManagerRepository.Update(currentVersion);

            // Lấy số phiên bản tiếp theo
            var nextVersionNumber = await _aiPromptManagerRepository.GetNextVersionNumberAsync(
                currentVersion.OriginalId ?? currentVersion.Id);

            // Tạo phiên bản mới sử dụng AutoMapper
            var newVersion = _mapper.Map<AiPromptManager>(currentVersion);
            newVersion.OriginalId = currentVersion.OriginalId ?? currentVersion.Id;
            newVersion.Version = nextVersionNumber;
            newVersion.VersionStatus = EnumVersionStatus.LastVersion;
            newVersion.VersionType = EnumVersion.V2;

            // Copy tất cả AICriteriaConfigs sang phiên bản mới sử dụng AutoMapper
            var allCriteria = await _aiCriteriaConfigRepository.GetAllByAiPromptManagerIdAsync(originalAiPromptManagerId);
            foreach (var criteria in allCriteria.Where(x => x.VersionStatus == EnumVersionStatus.LastVersion))
            {
                var newCriteria = _mapper.Map<AICriteriaConfigs>(criteria);
                newCriteria.AiPromptManagerId = newVersion.Id;
                newVersion.AICriteriaConfigs.Add(newCriteria);
            }

            _aiPromptManagerRepository.Add(newVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return newVersion;
        }

        public async Task<AiPromptManager> CreateNewVersionWithUpdatedCriteriaAsync(
            Guid originalAiPromptManagerId,
            AICriteriaConfigs updatedCriteria, CancellationToken cancellationToken)
        {
            // Tạo phiên bản mới
            var newVersion = await CreateNewVersionAsync(originalAiPromptManagerId, cancellationToken);

            // Validate input
            if (updatedCriteria == null)
            {
                return newVersion;
            }

            // Tìm criteria cần cập nhật trong phiên bản mới
            var criteriaToUpdate = newVersion.AICriteriaConfigs
                .FirstOrDefault(x => x.OriginalId == updatedCriteria.OriginalId || x.Id == updatedCriteria.Id);

            if (criteriaToUpdate != null)
            {
                // Sử dụng AutoMapper để cập nhật các thuộc tính
                _mapper.Map(updatedCriteria, criteriaToUpdate);
            }
            else
            {
                // Thêm mới criteria nếu chưa tồn tại
                var newCriteria = _mapper.Map<AICriteriaConfigs>(updatedCriteria);
                newCriteria.AiPromptManagerId = newVersion.Id;
                newCriteria.VersionStatus = EnumVersionStatus.LastVersion;
                newVersion.AICriteriaConfigs.Add(newCriteria);
            }

            _aiPromptManagerRepository.Update(newVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return newVersion;
        }

        public async Task<bool> HasDependenciesAsync(Guid aiCriteriaConfigId)
        {
            var criteria = await _aiCriteriaConfigRepository.Queryable.FirstOrDefaultAsync(x => x.Id == aiCriteriaConfigId);
            if (criteria == null)
            {
                return false;
            }
            // Kiểm tra xem có entity nào khác đang sử dụng criteria này không
            // Ví dụ: Test, ExamPractice, etc.
            // Đây là logic ví dụ, cần implement dựa trên business logic thực tế

            return false; // Tạm thời return false
        }
    }
}
