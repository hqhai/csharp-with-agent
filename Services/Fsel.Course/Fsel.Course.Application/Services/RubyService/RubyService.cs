// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Services.RubyService
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Infrastructure.Common.RubyHelpers;

    public class RubyService : IRubyService
    {
        public (string hash, int len) Snapshot(string baseTextNfc)
        {
            var si = new StringInfo(baseTextNfc);
            return (baseTextNfc, si.LengthInTextElements);
        }

        public int? TryReAnchor(string baseTextNfc, RubyAnnotation r, int searchWindow = 80)
        {
            var sel = r.SelectedText;
            if (string.IsNullOrEmpty(sel))
            {
                return null;

            }

            int charIdx = baseTextNfc.IndexOf(sel, StringComparison.Ordinal);

            if (charIdx >= 0)
            {
                var map = RubyTextNormalization.GraphemeMap(baseTextNfc);
                return RubyTextNormalization.CharOffsetToGrapheme(charIdx, map.charStarts);
            }

            //// nếu có Prefix/Suffix, tìm "Prefix + Selected + Suffix")
            //if (!string.IsNullOrEmpty(r.PrefixContext) && !string.IsNullOrEmpty(r.SuffixContext))
            //{
            //    var needle = r.PrefixContext + sel + r.SuffixContext;
            //    int idx = baseTextNfc.IndexOf(needle, StringComparison.Ordinal);

            //    if (idx >= 0)
            //    {
            //        var map = RubyTextNormalization.GraphemeMap(baseTextNfc);
            //        int startChar = idx + r.PrefixContext.Length;
            //        return RubyTextNormalization.CharOffsetToGrapheme(startChar, map.charStarts);
            //    }
            //}

            return null;
        }

        public string RenderHtml(string baseText, IEnumerable<RubyAnnotation> rubies)
        {
            var nfc = RubyTextNormalization.ToNfc(baseText);
            var si = new StringInfo(nfc);
            int total = si.LengthInTextElements;
            var textEls = new string[total];

            for (int i = 0; i < total; i++)
            {
                textEls[i] = WebUtility.HtmlEncode(si.SubstringByTextElements(i, 1));
            }

            var ordered = rubies.Where(r => !r.IsDeleted)
                                .OrderByDescending(r => r.StartGraphemeIndex)
                                .ToList();

            foreach (var r in ordered)
            {
                int s = r.StartGraphemeIndex;
                int e = s + r.LengthGraphemes;

                if (s < 0 || s >= total || e > total)
                {
                    continue;
                }

                string rb = string.Concat(Enumerable.Range(s, r.LengthGraphemes).Select(i => textEls[i]));
                string rt = WebUtility.HtmlEncode(r.Phonetic);

                string ruby = $"<ruby><rb>{rb}</rb><rt>{rt}</rt></ruby>";
                textEls[s] = ruby;

                for (int i = s + 1; i < e; i++)
                {
                    textEls[i] = string.Empty;
                }
            }
            return string.Concat(textEls);
        }
    }
}
