// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LoadPTTestResultsHandler
{
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;

    public interface IBaseLoadPTTestResultHandler
    {
        Task Handle(LoadPTTestResultContext context);

        IBaseLoadPTTestResultHandler SetNex(IBaseLoadPTTestResultHandler next);
    }

    public abstract class BaseLoadPTTestResultHandler : IBaseLoadPTTestResultHandler
    {
        public IBaseLoadPTTestResultHandler Next { get; set; }

        public abstract Task Handle(LoadPTTestResultContext context);

        public virtual IBaseLoadPTTestResultHandler SetNex(IBaseLoadPTTestResultHandler next)
        {
            Next = next;
            return next;
        }
    }

    public class LoadPTTestResultContext
    {
        public Guid? StudentId { get; set; }

        public Guid ProgramId { get; set; }

        public PTStateModel PTState { get; set; }

        public int? NumberOfModules { get; set; }

        public Flow FlowOfPT { get; set; }
    }
}
