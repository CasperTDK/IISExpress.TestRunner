using System;
using System.Linq;
using System.Reflection;
using IISExpress.TestRunner.Attribute;

namespace IISExpress.TestRunner
{
    internal class CommandLineParser
    {
        public static void ParseArguments<T>(string[] args, T commandLineArguments)
        {
            var allProperties = commandLineArguments.GetType().GetProperties();

            var propertiesAndCsvAttributes = allProperties
                .Where(p => p.HasAttribute<ConsoleArgumentAttribute>())
                .Select(p => Tuple.Create(p, p.GetAttribute<ConsoleArgumentAttribute>()))
                .ToList();

            foreach (var propertyInfo in propertiesAndCsvAttributes)
            {
                Bind(commandLineArguments, propertyInfo, args);
            }
        }

        private static void Bind<T>(T commandLineArguments, Tuple<PropertyInfo, ConsoleArgumentAttribute> property, string[] args)
        {
            var attribute = property.Item2;
            var propertyInfo = property.Item1;
            attribute.BindCell(args, commandLineArguments, propertyInfo);
        }
    }
}