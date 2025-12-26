// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.RubyHelpers
{
    using System.Globalization;
    using System.Text;
    using System.Security.Cryptography;

    public static class RubyTextNormalization
    {
        public static string ToNfc(string s) => s is null ? "" : s.Normalize(NormalizationForm.FormC);

        public static (string[] elems, int[] charStarts) GraphemeMap(string nfc)
        {
            var si = new StringInfo(nfc);
            int n = si.LengthInTextElements;
            var elems = new string[n];
            var starts = new int[n];
            int charPos = 0;

            for (int i = 0; i < n; i++)
            {
                var e = si.SubstringByTextElements(i, 1);
                elems[i] = e;
                starts[i] = charPos;
                charPos += e.Length;
            }

            return (elems, starts);
        }

        public static int CharOffsetToGrapheme(int charOffset, int[] starts)
        {
            int lo = 0, hi = starts.Length - 1, ans = 0;

            while (lo <= hi)
            {
                int mid = lo + hi >> 1;
                if (starts[mid] <= charOffset)
                {
                    ans = mid;
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            return ans;
        }
    }
}
