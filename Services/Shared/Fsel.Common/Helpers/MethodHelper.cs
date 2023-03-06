using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Common.Helpers
{
    public static class MethodHelper
    {
        private static ConcurrentDictionary<string, Dictionary<string, string>>? ErrorMessages;
        public static string GenerateErrorResult(string propertyName, object? propertyValue)
        {
            return $"{propertyName.Substring(0, 1).ToLower(CultureInfo.InvariantCulture)}{propertyName.Substring(1, propertyName.Length - 1)}: {propertyValue}";
        }

        public static string GetErrorMessage(string? errorCode, Assembly resourceAssembly)
        {
            return GetErrorMessage(errorCode, ref ErrorMessages, resourceAssembly);
        }

        public static string GetErrorMessage(string? errorCode, ref ConcurrentDictionary<string, Dictionary<string, string>>? errorMessages, Assembly resourceAssembly)
        {
            return string.Empty;
        }

        public static string GetExceptionMessage(Exception ex)
        {
            return "Message: " + ex.Message + ", InnerMessage: " + ex.InnerException?.Message;
        }

    }
}
