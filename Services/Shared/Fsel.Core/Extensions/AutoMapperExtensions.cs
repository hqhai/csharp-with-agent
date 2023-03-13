using System.Reflection;
using AutoMapper;

namespace Fsel.Core.Extensions
{
    public static class AutoMapperExtensions
    {
        public static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
        {
            Type typeFromHandle = typeof(TSource);
            PropertyInfo[] properties = typeof(TDestination).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo[] array = properties;
            foreach (PropertyInfo property in array)
            {
                PropertyInfo? propertyInfo = typeFromHandle.GetProperties().FirstOrDefault((PropertyInfo p) => p.Name == property.Name);
                if (propertyInfo == null)
                {
                    expression.ForMember(property.Name, delegate (IMemberConfigurationExpression<TSource, TDestination, object> opt)
                    {
                        opt.Ignore();
                    });
                }
            }

            return expression;
        }
    }
}
