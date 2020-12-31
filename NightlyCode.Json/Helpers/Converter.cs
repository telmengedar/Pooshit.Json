// copied from NightlyCode.Core Library

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace NightlyCode.Json.Helpers {

    /// <summary>
    /// converter used to convert data types
    /// </summary>
    static class Converter {
        static readonly Dictionary<Tuple<Type,Type>, Func<object, object>> specificconverters = new Dictionary<Tuple<Type,Type>, Func<object, object>>();

        /// <summary>
        /// cctor
        /// </summary>
        static Converter() {
            specificconverters[new Tuple<Type,Type>(typeof(double), typeof(string))] = o => ((double)o).ToString(CultureInfo.InvariantCulture);
            specificconverters[new Tuple<Type,Type>(typeof(string), typeof(int))] = o => int.Parse((string)o);
            specificconverters[new Tuple<Type,Type>(typeof(string), typeof(int[]))] = o => ((string)o).Split(';').Select(int.Parse).ToArray();
            specificconverters[new Tuple<Type,Type>(typeof(double), typeof(TimeSpan))] = o => TimeSpan.FromTicks((long)(double)o);
            specificconverters[new Tuple<Type,Type>(typeof(long), typeof(TimeSpan))] = o => TimeSpan.FromTicks((long)o);
            specificconverters[new Tuple<Type,Type>(typeof(TimeSpan), typeof(long))] = v => ((TimeSpan)v).Ticks;
            specificconverters[new Tuple<Type,Type>(typeof(string), typeof(Type))] = o => Type.GetType((string)o);
            specificconverters[new Tuple<Type,Type>(typeof(long), typeof(DateTime))] = v => new DateTime((long)v);
            specificconverters[new Tuple<Type,Type>(typeof(DateTime), typeof(long))] = v => ((DateTime)v).Ticks;
            specificconverters[new Tuple<Type,Type>(typeof(Version), typeof(string))] = o => o.ToString();
            specificconverters[new Tuple<Type,Type>(typeof(string), typeof(Version))] = s => Version.Parse((string)s);
            specificconverters[new Tuple<Type,Type>(typeof(string), typeof(TimeSpan))] = s => TimeSpan.Parse((string)s);
            specificconverters[new Tuple<Type,Type>(typeof(long), typeof(Version))] = l => new Version((int)((long)l >> 48), (int)((long)l >> 32) & 65535, (int)((long)l >> 16) & 65535, (int)(long)l & 65535);
            specificconverters[new Tuple<Type,Type>(typeof(Version), typeof(long))] = v => (long)((Version)v).Major << 48 | ((long)((Version)v).Minor << 32) | ((long)((Version)v).Build << 16) | (long)((Version)v).Revision;
            specificconverters[new Tuple<Type,Type>(typeof(string), typeof(bool))] = v => (string)v != "" && (string)v != "0" && ((string)v).ToLower() != "false";
            specificconverters[new Tuple<Type,Type>(typeof(string), typeof(byte[]))] = v => System.Convert.FromBase64String((string)v);
            specificconverters[new Tuple<Type,Type>(typeof(string), typeof(Guid))] = o => Guid.Parse((string)o);
            specificconverters[new Tuple<Type,Type>(typeof(byte[]), typeof(Guid))] = o => new Guid((byte[])o);
        }

        /// <summary>
        /// registers a specific converter to be used for a specific conversion
        /// </summary>
        /// <param name="target">type to convert value to</param>
        /// <param name="converter">delegate used to convert value</param>
        /// <param name="source">type of value from which to convert</param>
        public static void RegisterConverter(Type source, Type target, Func<object, object> converter) {
            specificconverters[new Tuple<Type, Type>(source, target)] = converter;
        }

        static object ConvertToEnum(object value, Type targettype, bool allownullonvaluetypes = false) {
            Type valuetype;
            if(value is string) {
                if(((string)value).Length == 0) {
                    if(allownullonvaluetypes)
                        return null;
                    throw new ArgumentException("Empty string is invalid for an enum type");
                }

                if(((string)value).All(char.IsDigit)) {
                    valuetype = Enum.GetUnderlyingType(targettype);
                    return Convert(value, valuetype, allownullonvaluetypes);
                }
                return Enum.Parse(targettype, (string)value, true);
            }
            valuetype = Enum.GetUnderlyingType(targettype);
            return Enum.ToObject(targettype, Convert(value, valuetype, allownullonvaluetypes));
        }

        /// <summary>
        /// converts the value to a specific target type
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="targettype">type to convert value to</param>
        /// <param name="allownullonvaluetypes">determines whether to allow null when converting to value types. If this is set to true, the default value for the target type is returned on null</param>
        /// <returns>converted value</returns>
        public static object Convert(object value, Type targettype, bool allownullonvaluetypes = false) {
            if(value == null || value is DBNull) {

                if(targettype.IsValueType && !(targettype.IsGenericType && targettype.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                    if(allownullonvaluetypes)
                        return Activator.CreateInstance(targettype);
                    throw new InvalidOperationException("Unable to convert null to a value type");
                }
                return null;
            }

            if(value.GetType() == targettype || targettype.IsInstanceOfType(value))
                return value;

            if(targettype.IsEnum)
                return ConvertToEnum(value, targettype, allownullonvaluetypes);

            Tuple<Type,Type> key = new Tuple<Type,Type>(value.GetType(), targettype);
            if(specificconverters.TryGetValue(key, out Func<object, object> specificconverter))
                return specificconverter(value);


            if(targettype.IsGenericType && targettype.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                // the value is never null at this point
                return new NullableConverter(targettype).ConvertFrom(Convert(value, targettype.GetGenericArguments()[0], true));
            }
            return System.Convert.ChangeType(value, targettype, CultureInfo.InvariantCulture);
        }
    }
}