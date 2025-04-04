﻿namespace PeachtreeBus.Sagas;

/// <summary>
/// A base class for all sagas to inherit
/// </summary>
/// <typeparam name="TSagaData">A class that holds Saga data that will automatically be saved and loaded when processing saga messages.</typeparam>
public abstract class Saga<TSagaData> where TSagaData : class, new()
{
    /// <summary>
    /// Each saga must have a unique name in the system.
    /// This is used to build the table name for the saga data.
    /// </summary>
    public abstract SagaName SagaName { get; }

    /// <summary>
    /// Holds Arbitrary data that will be persisted between saga message handlers.
    /// </summary>
    public TSagaData Data { get; set; } = new TSagaData();

    /// <summary>
    /// Saga Handler code will set this to true to inform the bus that the saga is compelte, and persisted data may be discarded.
    /// </summary>
    public bool SagaComplete { get; set; } = false;

    /// <summary>
    /// Each message that a saga will handle must be convertable to a saga key.
    /// This is used to map any given message to an instance of the persisted saga data.
    /// Two related messages should produce the same key, allowing the bus to use the same
    /// saga data instance for both messages.
    /// </summary>
    /// <example>
    /// {
    ///     mapper.Add<MessageA>(m => new SagaKey(m.Id.ToString()));
    ///     mapper.Add<MessageB>(m => new SagaKey(m.Id.ToString()));
    /// }
    /// </example>
    /// <param name="mapper"></param>
    public abstract void ConfigureMessageKeys(ISagaMessageMap mapper);
}
