// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.RubyService
{
    using Fsel.Course.Domain.Entities;

    public interface IRubyService
    {
        string RenderHtml(string baseText, IEnumerable<RubyAnnotation> rubies);
        int? TryReAnchor(string baseTextNfc, RubyAnnotation r, int searchWindow = 80);
        (string hash, int len) Snapshot(string baseTextNfc);
    }
}
