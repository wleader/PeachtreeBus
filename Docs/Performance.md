# Peachtree Bus Performance

The performance of PeachtreeBus, like any software package is very dependent on the environment where it is being used.
However, there is support for various kinds of observability, that can help a user of the library determine what 
the performance of a system built with PeachtreeBus is like in their environment.

## Meters
There are several Meters that are available from PeachtreeBus. Conceptually, Meters are numbers that can be read to give
insight into what the process is doing. There are several that PeachtreeBus exposes.

### Using dotnet-counters
These meters can be read using the dotnet-counters tool.
https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters

### Using Open Telemetry or Aspire
Alternatively, a process can be made to export these meters via Open Telemetry:

```csharp
using var metersProvider = Sdk.CreateMeterProviderBuilder()
    .ConfigureResource(r => r.AddService("PeachtreeBus"))
    .AddMeter([PeachtreeBus.Telemetry.Meters.Messaging.Name])
    .AddOtlpExporter()
    .Build();
````

If the process is being run using .Net Aspire, Aspire will also read these meters and make them available via it's 
Browser Interface.

### Counters
Counters are numbers that only go up.

* messaging.client.sent.messages - A count of messages sent.
* messaging.client.consumed.messages - A count of successfully processed messages.
* peachtreebus.client.attempted.messages - A count of attempts to handle a message.
* peachtreebus.client.failed.messages - A count of messages sent to the Failed queue.
* peachtreebus.client.retry.messages - A count of messages that got scheduled for a retry.
* peachtreebus.client.blockedsaga.messsage - A count of times when a message was re-queued because it's saga was locked.

### Up-Down Counters
Up-Down Counters, as the name implies increase and decrease while the system is running.

* peachtreebus.client.active.messages - The number of messages that are currently being handled.
* peachtreebus.client.active.tasks - A count of all tasks that have started and have not completed.

## Traces
PeachtreeBus also makes use of Traces, using System.Diagnostics.ActivitySource and System.Diagnostics.Activity. 
This allows the creation and recording of timing information.

### Using Open Telemetry or Aspire
A process can be made to export traces to open telemetry. .Net Aspire will aslo recieve and present the trace data if
Aspire is in use.

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerBuilder => tracerBuilder
        .AddSource("PeachtreeBus.Messaging")
        .AddSource("PeachtreeBus.User")
        .AddOtlpExporter());
````

### Trace Sources
There are multiple trace sources that can be listend  to to get insight into what an application built on PeachtreeBus
is spending its time on.

* PeachtreeBus.Messaging - Core Messaging Activities
* PeachtreeBus.User - Wraps calls around the library user's code, such as Pipeline Steps, and Message Handlers.
* PeachtreeBus.Internal - Wraps around code that is mostly of interest to Library Maintainers. These are likely to
    Change over time, and users of the library should not rely on them to exist in future version.
* PeachtreeBus.DataAccess - Wraps around the code where PeachtreeBus is communicating with the Database. These are
    useful for determining if there is a concern with the performance of the database server, or specific queries and
    statements. These are unlikely to change, but should be considered internal to PeachtreeBus.
