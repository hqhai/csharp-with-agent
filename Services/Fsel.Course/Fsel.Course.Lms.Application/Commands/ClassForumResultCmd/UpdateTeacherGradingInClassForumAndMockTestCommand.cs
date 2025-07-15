// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
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

            await _classForumResultRepository.BulkUpdateList(classForumResult, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.LessonResultId, c.ClassForumId };
            });

            var mockTestResult = await _mockTestResultRepository.Queryable
                         .Where(x => x.GradingTeacherId != null && x.GradingStartDate!.Value.AddMinutes(30) < dateNow).ToListAsync(cancellationToken);

            foreach (var item in mockTestResult)
            {
                item.GradingTeacherId = null;
                item.GradingStartDate = null;
            }
            await _mockTestResultRepository.BulkUpdateList(mockTestResult, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.MockTestId, c.CourseId, c.UnitId, c.StudentId };
            });
            return methodResult;
        }
    }
}
