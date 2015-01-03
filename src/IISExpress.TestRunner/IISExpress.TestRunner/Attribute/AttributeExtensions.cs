using System.Linq;
using System.Reflection;

namespace IISExpress.TestRunner.Attribute
{
    static class AttributeExtensions
    {
        public static bool HasAttribute<TAttribute>(this ICustomAttributeProvider provider) where TAttribute : System.Attribute
        {
            return provider.GetCustomAttributes(typeof(TAttribute), false).Any();
        }

        public static TAttribute GetAttribute<TAttribute>(this ICustomAttributeProvider provider) where TAttribute : System.Attribute
        {
            return provider.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>().Single();
        }
    }
}