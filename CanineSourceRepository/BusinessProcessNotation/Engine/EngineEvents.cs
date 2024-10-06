using CanineSourceRepository.BusinessProcessNotation.BpnEventStore;
using Hangfire.States;
using Hangfire;
using System.Linq.Expressions;
using System;
using Hangfire.Storage;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EngineEvents;

public interface IEngineEvents
{
  Guid CorrelationId { get; }
  Guid ContextId { get; }
  Guid FeatureId { get; }
  long FeatureVersion { get; }
}

public record BpnFeatureStarted(Guid ContextId, Guid FeatureId, long FeatureVersion, DateTimeOffset StarTime, Guid CorrelationId) : IEngineEvents;
public record BpnFeatureError(Guid CorrelationId, Guid ContextId, Guid FeatureId, long FeatureVersion, ErrorEvent Exception) : IEngineEvents;
public record BpnFeatureCompleted(Guid CorrelationId, Guid ContextId, Guid FeatureId, long FeatureVersion, DateTimeOffset EndTime, double DurationMs) : IEngineEvents;
public record BpnTaskInitialized(Guid CorrelationId, Guid ContextId, Guid FeatureId, long FeatureVersion, Guid TaskId, string Input) : IEngineEvents;
public record BpnTaskFailed(Guid CorrelationId, Guid ContextId, Guid FeatureId, long FeatureVersion, Guid TaskId, ErrorEvent Exception, double ExecutionTimeMs) : IEngineEvents;
public record BpnFailedTaskReInitialized(Guid CorrelationId, Guid ContextId, Guid FeatureId, long FeatureVersion, string NewInput, double ExecutionTimeMs) : IEngineEvents;
public record BpnTaskSucceeded(Guid CorrelationId, Guid ContextId, Guid FeatureId, long FeatureVersion, Guid TaskId, double ExecutionTimeMs) : IEngineEvents;
public record BpnTransitionUsed(Guid CorrelationId, Guid ContextId, Guid FeatureId, long FeatureVersion, Guid FromBpn, Guid ToBpn) : IEngineEvents;
public record BpnTransitionSkipped(Guid CorrelationId, Guid ContextId, Guid FeatureId, long FeatureVersion, Guid FromBpn, Guid ToBpn) : IEngineEvents;
public record ErrorEvent(string Message, string Details);


public class EventLogJsonConverter : JsonConverter<IEngineEvents>
{
  public override IEngineEvents Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    using var jsonDoc = JsonDocument.ParseValue(ref reader);
    var jsonObject = jsonDoc.RootElement;

    // Assuming there is a "Type" property to differentiate between Bpn types
    var typeProperty = jsonObject.GetProperty("Type").GetString();

    // Determine the type to instantiate based on the typeName
    IEngineEvents engineEvent = typeProperty switch
    {
      nameof(BpnFeatureStarted) => JsonSerializer.Deserialize<BpnFeatureStarted>(jsonObject.GetRawText(), options)!,
      nameof(BpnFeatureError) => JsonSerializer.Deserialize<BpnFeatureError>(jsonObject.GetRawText(), options)!,
      nameof(BpnFeatureCompleted) => JsonSerializer.Deserialize<BpnFeatureCompleted>(jsonObject.GetRawText(), options)!,
      nameof(BpnTaskInitialized) => JsonSerializer.Deserialize<BpnTaskInitialized>(jsonObject.GetRawText(), options)!,
      nameof(BpnTaskFailed) => JsonSerializer.Deserialize<BpnTaskFailed>(jsonObject.GetRawText(), options)!,
      nameof(BpnFailedTaskReInitialized) => JsonSerializer.Deserialize<BpnFailedTaskReInitialized>(jsonObject.GetRawText(), options)!,
      nameof(BpnTaskSucceeded) => JsonSerializer.Deserialize<BpnTaskSucceeded>(jsonObject.GetRawText(), options)!,
      nameof(BpnTransitionUsed) => JsonSerializer.Deserialize<BpnTransitionUsed>(jsonObject.GetRawText(), options)!,
      nameof(BpnTransitionSkipped) => JsonSerializer.Deserialize<BpnTransitionSkipped>(jsonObject.GetRawText(), options)!,

      _ => throw new JsonException($"Unknown Bpn type: {typeProperty}")
    };

    return engineEvent;
  }

  public override void Write(Utf8JsonWriter writer, IEngineEvents value, JsonSerializerOptions options)
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