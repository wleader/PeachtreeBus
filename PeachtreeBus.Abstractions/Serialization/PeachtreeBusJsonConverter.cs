using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PeachtreeBus.Serialization;

/// <summary>
/// An abstract class that makes it very easy to define a converter for our custom types.
/// </summary>
/// <typeparam name="TDomain"></typeparam>
/// <typeparam name="TSystem"></typeparam>
/// <param name="ToDomain"></param>
/// <param name="ToSystem"></param>
public abstract class PeachtreeBusJsonConverter<TDomain, TSystem>(
    Func<TSystem?, TDomain> ToDomain,
    Func<TDomain?, TSystem> ToSystem) : JsonConverter<TDomain>
{
    private readonly Func<TSystem?, TDomain> toDomain = ToDomain;
    private readonly Func<TDomain, TSystem> toSystem = ToSystem;

    private readonly JsonConverter<TSystem> converter =
        (JsonConverter<TSystem>)JsonSerializerOptions.Default.GetConverter(typeof(TSystem));

    public override TDomain? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return toDomain(converter.Read(ref reader, typeof(TSystem), options));
    }

    public override void Write(Utf8JsonWriter writer, TDomain value, JsonSerializerOptions options)
    {
        converter.Write(writer, toSystem(value), options);
    }
}
