using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EngineEvents;

public interface IEvent;

//TODO: CorrelationId => event?
//TODO: CausationId => event?
//TODO: Headers => ?
//TODO: Archived => ?
public record FeatureStarted(DateTimeOffset StarTime, Guid FeatureId, long FeatureVersion, Guid CorrelationId) : IEvent;
public record FeatureError(ErrorEvent Exception) : IEvent;
public record BpnFeatureCompleted(DateTimeOffset EndTime, TimeSpan Duration) : IEvent;
public record TaskInitialized(Guid TaskId, string Input) : IEvent;
public record TaskFailed(Guid TaskId, ErrorEvent Exception, TimeSpan ExecutionDuration) : IEvent;
public record FailedTaskReInitialized(string NewInput, TimeSpan ExecutionDuration) : IEvent;
public record TaskSucceeded(Guid TaskId, TimeSpan ExecutionDuration) : IEvent;
public record TransitionUsed(Guid FromBpn, Guid ToBpn) : IEvent;
public record TransitionSkipped(Guid FromBpn, Guid ToBpn) : IEvent;
public record ErrorEvent(string Message, string Details);


public class EventLogJsonConverter : JsonConverter<IEvent>
{
  public override IEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    throw new NotImplementedException(); // You can implement reading if necessary
  }

  public override void Write(Utf8JsonWriter writer, IEvent value, JsonSerializerOptions options)
  {
    // Determine the actual type of the object
    var type = value.GetType();
    var typeName = type.Name;

    // Write start of the object
    writer.WriteStartObject();

    // Write the type information
    writer.WriteString("Type", typeName);

    // Serialize the properties of the object manually
    var typeProperties = type.GetProperties();
    foreach (var property in typeProperties)
    {
      var propertyValue = property.GetValue(value);
      if (propertyValue != null)
      {
        writer.WritePropertyName(property.Name);
        JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, options);
      }
    }

    // Write end of the object
    writer.WriteEndObject();
  }
}


public static class InputLogger
{
  public static string LogInput(object input)
  {
    var logDetails = LogInputRecursive(input);
    return JsonSerializer.Serialize(logDetails, new JsonSerializerOptions { WriteIndented = true });
  }

  private static object? LogInputRecursive(object input)
  {
    if (input == null) return null;

    Type inputType = input.GetType();

    if (IsSimpleType(inputType))
    {
      // Directly return the simple value
      return input;
    }

    var logDetails = new Dictionary<string, object?>();

    foreach (var prop in inputType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    {
      var value = prop.GetValue(input);
      logDetails[prop.Name] = value == null ? value  : TruncateIfNeeded(value);
    }

    return logDetails;
  }

  private static object? TruncateIfNeeded(object value)
  {
    if (value == null) return null;

    if (value is string strValue)
    {
      return TruncateString(strValue);
    }
    else if (value is byte[] byteArrayValue)
    {
      return TruncateBytes(byteArrayValue);
    }
    else if (!IsSimpleType(value.GetType()))
    {
      // Handle nested objects recursively
      return LogInputRecursive(value);
    }
    else
    {
      // For simple types (e.g., int, double), return the value as is
      return value;
    }
  }

  private static bool IsSimpleType(Type type)
  {
    return type.IsPrimitive ||
           type.IsEnum ||
           type == typeof(string) ||
           type == typeof(bool) ||
           type == typeof(long) ||
           type == typeof(decimal) ||
           type == typeof(DateTimeOffset) ||
           type == typeof(DateOnly) ||
           type == typeof(TimeOnly) ||
           type == typeof(Guid) ||
           type == typeof(byte[]);
  }

  private static string TruncateString(string str)
  {
    if (str.Length <= 100)
    {
      return str;
    }
    return $"{str.Substring(0, 40)} ... {str.Substring(str.Length - 40, 40)} ({str.Length} total length)";
  }

  private static string TruncateBytes(byte[] byteArray)
  {
    if (byteArray.Length <= 256)
    {
      return BitConverter.ToString(byteArray);
    }
    var first40Bytes = BitConverter.ToString(byteArray.Take(40).ToArray());
    var last40Bytes = BitConverter.ToString(byteArray.Skip(byteArray.Length - 40).ToArray());
    return $"{first40Bytes} ... {last40Bytes} ({byteArray.Length} total bytes)";
  }
}