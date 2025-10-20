// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public static class FeatureCommon
    {
        public static string DisplayName(this Enum e) =>
        e.GetType().GetMember(e.ToString())[0]
         .GetCustomAttributes(typeof(DisplayAttribute), false)
         is DisplayAttribute[] { Length: > 0 } attrs
         ? (attrs[0].GetName() ?? e.ToString())
         : e.ToString();

        public static string GroupName(this Enum e) =>
            e.GetType().GetMember(e.ToString())[0]
             .GetCustomAttributes(typeof(DisplayAttribute), false)
             is DisplayAttribute[] { Length: > 0 } attrs
             ? (attrs[0].GetGroupName() ?? "")
             : "";

        public static int Order(this Enum e) =>
            e.GetType().GetMember(e.ToString())[0]
             .GetCustomAttributes(typeof(DisplayAttribute), false)
             is DisplayAttribute[] { Length: > 0 } attrs
             ? (attrs[0].GetOrder() ?? 0)
             : 0;
    }
}
