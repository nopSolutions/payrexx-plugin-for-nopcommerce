using System;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Payrexx.Domain
{
    /// <summary>
    /// Represents object to convert JSON string values into boolean data type
    /// </summary>
    public class BoolConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        /// <returns>True if this instance can convert the specified object type; otherwise, false</returns>
        public override bool CanConvert(Type objectType)
        {
            //handle only boolean
            return objectType == typeof(bool);
        }

        /// <summary>
        /// Reads the JSON representation of the object
        /// </summary>
        /// <param name="reader">The JsonReader to read from</param>
        /// <param name="objectType">Type of the object</param>
        /// <param name="existingValue">The existing value of object being read</param>
        /// <param name="serializer">The calling serializer</param>
        /// <returns>The object value</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //handle all available strings
            var value = reader.Value?.ToString().ToLower().Trim() ?? "0";
            switch (value)
            {
                case "true":
                case "yes":
                case "y":
                case "1":
                    return true;

                case "false":
                case "no":
                case "n":
                case "0":
                    return false;
            }

            return serializer.Deserialize(reader, objectType);
        }

        /// <summary>
        /// Writes the JSON representation of the object
        /// </summary>
        /// <param name="writer">The JsonWriter to write to</param>
        /// <param name="value">The value</param>
        /// <param name="serializer">The calling serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //write result as 0-1
            var stringValue = value is bool && (bool)value ? "1" : "0";
            serializer.Serialize(writer, stringValue);
        }
    }
}