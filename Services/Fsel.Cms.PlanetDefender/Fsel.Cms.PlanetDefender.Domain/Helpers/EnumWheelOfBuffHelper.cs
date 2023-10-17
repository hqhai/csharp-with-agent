// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Helpers
{
    using System.ComponentModel;
    using System.Reflection;
    using Fsel.Cms.PlanetDefender.Domain.Enums;

    public class EnumWheelOfBuffHelper
    {
        public static string GetDescription(EnumWheelOfBuffType enumValue)
        {
            FieldInfo fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes != null && attributes.Length > 0)
                {
                    return attributes[0].Description;
                }
            }

            return enumValue.ToString();
        }
    }
}
