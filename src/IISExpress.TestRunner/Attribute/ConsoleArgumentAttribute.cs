using System;
using System.Reflection;

namespace IISExpress.TestRunner.Attribute
{
    internal class ConsoleArgumentAttribute : System.Attribute
    {
        private readonly int _index;
        private readonly bool _optional;

        public ConsoleArgumentAttribute(int index, bool optional = false)
        {
            _index = index;
            _optional = optional;
        }

        public void BindCell<T>(string[] args, T commandLineArguments, PropertyInfo property)
        {
            if (_index >= args.Length)
            {
                if (!_optional)
                {
                    throw new Exception(string.Format("Property '{0}' is configured to be bound to argument index {1}, but there's only {2} arguements",
                        property.Name, _index, args.Length));
                }
            }

            property.SetValue(commandLineArguments, args[_index], new object[0]);
        }
    }
}