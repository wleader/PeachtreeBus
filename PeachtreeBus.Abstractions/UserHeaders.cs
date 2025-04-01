using System.Collections.Generic;

namespace PeachtreeBus;

public interface IReadOnlyUserHeaders : IReadOnlyDictionary<string, string>;

/// <summary>
/// Storage for user headers.
/// Users can store anything they like with the message,
/// and these will be available when the message is handled.
/// </summary>
public class UserHeaders : Dictionary<string, string>, IReadOnlyUserHeaders;
