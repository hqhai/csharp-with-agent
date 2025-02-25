// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdateTeacherGradingInClassForumAndMockTestCommand : IRequest<VoidMethodResult>
    {
    }

    public class UpdateTeacherGradingInClassForumAndMockTestCommandHandler : IRequestHandler<UpdateTeacherGradingInClassForumAndMockTestCommand, VoidMethodResult>
    {
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public UpdateTeacherGradingInClassForumAndMockTestCommandHandler(IClassForumResultRepository classForumResultRepository, IMockTestResultRepository mockTestResultRepository)
        {
            _classForumResultRepository = classForumResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<VoidMethodResult> Handle(UpdateTeacherGradingInClassForumAndMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();
            var dateNow = DateTime.UtcNow;

            var classForumResult = await _classForumResultRepository.Queryable
                        .Where(x => x.GradingTeacherId != null && x.GradingStartDate!.Value.AddMinutes(30) < dateNow).ToListAsync(cancellationToken);

            foreach (var item in classForumResult)
            {
                item.GradingTeacherId = null;
                item.GradingStartDate = null;
            }

            _classForumResultRepository.UpdateList(classForumResult, false, x => x.LessonResultId, x => x.ClassForumId, x => x.StudentId);
            await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var mockTestResult = await _mockTestResultRepository.Queryable
                         .Where(x => x.GradingTeacherId != null && x.GradingStartDate!.Value.AddMinutes(30) < dateNow).ToListAsync(cancellationToken);

            foreach (var item in mockTestResult)
            {
                item.GradingTeacherId = null;
                item.GradingStartDate = null;
            }
            _mockTestResultRepository.UpdateList(mockTestResult, false, x => x.MockTestId, x => x.CourseId, x => x.UnitId, x => x.StudentId);
            await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return methodResult;
        }
    }
}
