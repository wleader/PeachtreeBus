# PeachtreeBus
A Message Bus Library

## What's New

### July 14th 2025
The Dependency Injection Scopes Release

Changes:
* IWrappedScope and IWrappedScopeFactory have been replaces with IScopeFactory, IServiceProviderAccessor and IServiceProvider. This may cause upgrading users to have to make some code changes if they were using these interfaces.
* Some project Restructuring. The SimpleInjector and MicrosoftDependencyInjection packages have been reworked to better share code and tests, though this should be mostly transparent to users.
* Unit Test projects and Example Projects have been moved into sub folders.
* Enhancements to The Task Management System (mostly making the code easier to unit test.)
* Fixed https://github.com/wleader/PeachtreeBus/issues/32 - High CPU usage when idle.
* Fixed https://github.com/wleader/PeachtreeBus/issues/33 - Missing error handling on Estimators (and code called by Task Starters.)

### June 11th 2025
The Microsoft Dependency Injection Release

There have been a few updates since April:

* 0.11.1 - Allow Underscores in Saga Names.
* 0.11.2 - Messages for sagas that have not been started or have completed no longer throw. They now log a Info level message.
* 0.11.3 - Creation of the MessageInterfaces assembly/package. Fixed an issue with the messaging tags for telemetry.
* 0.11.4 - Reworked the 'main' loop to auto scale the number of Tasks that are running based on how many messages are available to be processed (up to a maximum).
* 0.11.5 - Added the IRunStartupTasks inteface which can be replaced in the DI Container to enable users to unit test their Main() code that would run PeachtreeBus. ISharedDatabase can now have its connection set allowing IQueueWriter or ISubscribedPublisher to write using an existing database connection. (Useful for Send-Only operations).
* 0.11.6 - Creation of the PeachtreeBus.MicrosoftDependencyInjection assembly/package. This offers extensions to make it easy to register PeachtreeBus with an IServiceCollection, or even run it as an IHostedService when using Microsoft.Extensions.Hosting.

### April 1st 2025
The 'Enterprise' Update

Oh Baby this is a big one. I feel bad for anyone upgrading from 0.10.5.(which as far as I know is only me at this time.) A lot has changed. Here is a summary:

* Simplified some SQL operations to make selecting the next message simpler.
* Constraints and Indexes on tables are now named.
* A priority can be supplied when sending or publishing a message. Higher priority messages get selected first, presuming they meet other requirements like the not before time.
* Some vulnerable 3rd party packages have been updated.
* The Dapper code internally now no longer attempts to set a Dapper.SqlMapper.TypeHandler for System.DateTime, in case users want to manage the mapping for System.DateTime themselves. PeachtreeBus now uses an internal PeachtreeBus.Data.UtcDateTime data type, and all the time values it stores in the database are stored as UTC values.
* Priority and other header values are exposed via the Context objects.
* Lots of things have strong types now instead of just being a System.String. SchemaName, QueueName, SagaName, and more.
* Users can supply an IRetryStrategy to the DI Container to control message retries.
* The library now targets .Net 8.0. Older versions, and .Net Standard 2.1 are no longer targeted.
* Publishing subscribed messages is now a single database operation and is much faster.
* Subscription 'Category' has been renamed to 'Topic'. This a more common terminology. 
* There is a new configuration scheme in place. Users of the library now create a new PeachtreeBus.BusConfiguation object to configure everything. 
* Contexts all have interfaces now.
* Outgoing messages now have support for Pipelines allowing running custom code before or after messages are sent.
* Anything that user code should have to reference has been moved into the PeachtreeBus.Abstractions assembly/package. This allows writing code that interacts with PeachtreeBus, but without creating a dependency on the Core Library, and its dependencies.
* Messages now have 'UserHeaders', which is really just a System.Collections.Generic.Dictionary<string,string> that is saved in the message headers. 
* Support for Telemetry systems via System.Diagnostics.Tracing, and System.Diagnostics.Metrics. This means that if you create a listener for the Tracing and Metrics sources, you can collect and direct this data. The Example application has code that can setup an OpenTelemetry Exporter. The names for spans and metrics attempts to follow established conventions for messaging platforms.
* Telemetry Context Propagation means that when one message causes another message, those messages will be automatically linked in Telemetry data. You can control this by specifying a new Conversation when sending or publishing.
* The Topic is now part of the subscribed message tables. This is used for setting attributes in received message telemetry, but its also just nicer to look at when peeking at the tables directly.
* DataAccess activity tracing can be used to measure and observe the performance of SQL Server activities from PeachtreeBus.
* Sagas have a MetaData column in the Saga Data tables. Currently this just contains the time the Saga instance was created and the last time a message was handled. It does not affect message processing, but may be useful when investigating if a defect in a user's saga code causes the saga to never complete and delete the row in the saga data table.
* Like previous releases, more and better test coverage.
* 20% Cooler than the previous release.
* Yes, I am aware that this is an April 1st release. How could I not?

### What's Old
You can also read the [Old News](WhatsOld.md) if you like.

## About

Another Message Bus? What gives? Aren't there enough already? No. :D

Though an explanation probably would help you understand why this exists. Yes there are some other really mature message bus libraries out there and you sure should have a look at them because they aren't bad either. But the reason I created this was that there was a very specific set of features that I wanted to have and really only one of the other libraries had that exact feature set, but that library was too expensive for my personal projects, and suffers a bit from Kitchen Sink features. Yeah, when your a commercial software package you want to be useful to a wide audience so that you have the most chances to sell your product. Thats both good and bad. Its good because it means the software is really useful, but its bad because the software ends up getting a lot of features that aren't really useful to everyone. For example, if you want to be able to use different message transports, you end up writing a lot of code to make it possible for different users to use different message transports. Except most users only ever use one, so all other transports are unneeded. Adding to that, all the middle-ware code to support that swapping is ultimately not useful either. I ramble.

I wanted a message bus that has the following features:
* SQL Server Message Transport
* SimpleInjector Friendly (I'm a fan.)
* Interface patterns that were simple to understand and promote unit testable code.
* Respects that the application owns the DI Container.
* Sagas!
* Publish and Subscribe!
* A Simple way to send a message from "send only" code such as Asp.Net Core.


## SQL Transport
A lot of message buses shy away from SQL server as a transport. It does sorta make sense in a lot of scenarios. If you are exchanging message with an unknown system you don't really want them to have access to queue tables. In this case its totally appropriate to pass the message through some kind of independant messaging system. And Indeed if thats what I was doing then I certainly would want to do that. However, the kind of message bus based software I typically work on, the vast majority of what goes on is the system sending messages to itself, and when that is the case, Its not a huge risk for two processes that are already sharing the same database, to use that database for the message transport. If you are installing my application do you want to have a database server and a messaging service just so the application can talk to itself, or would you rather just have just the database server? Eliminating the need for a messaging service reduces the deployment complexity of the application so thats a good thing. Of course this doesn't completely eliminate the ability to communicate with other messaging services. If you really had to send a message out of the system itself, you could always build in a broker that takes a message from the Database, forwards the message onto some other message queue service. And likewise if you need to recieve messages from some other queue service, a broker could read that other service, deduplicate, and insert the message into the database. 

But there is another piece of magic going on here when using SQL for the message transport: Exactly-Once processing of messages is much easier. Reading a message from the database, performing application logic on the database, delivery of new messages, and completion of the message is contained entierly in the database transaction, and as such, all of it happens or none of it happens. This greatly simplifies the application code in that most cases you don't need to check if a message was processed previously or if it partially succeeded. You don't need to workout if the the application data is in some unknown state. The message completed entirely or not at all. This greatly reduces the complexity of error recovery. For this reason, I find SQL transport to be an extremely compelling option.

Another great thing about SQL for the message transport, is that the messages get included in database backups and redundancy. If you are using SQL Always On, then you get the reliablity and recovery that offers automatically on the messages, which I think is just fantastic and way easier that trying to setup replication or quorum systems that other messaging platforms need for that level of reliabilty.

## Dependency Injection / Inversion Of Control
You'd think by now this would be something more libraries get right these days, but we are still very much dealing with walled garden mentality, where libraries are built to hide things and "keep you out of trouble", but I find that to be a fools errand. Why? Well every time you go out of your way to hide internals of things you end up cutting off access to things that people actually want to do, then you end up adding in inscrutable APIs with method names like .ReplaceSomeInternalThingInLibrary().... Makes me sad. I don't want to see or write lots of code where you have to build options objects and overide some bizzare builder class that probably doesn't even let you customize the thing you want. At the end of the day its way simpler for me to not write that junk, and its way simpler for you to just register something else in the container. Thats how its supposed to work isn't it? So yeah, PeachtreeBus has almost nothing to configure. Theres some helpers to populate the DI container, but you could totally do it all yourself. 

But yeah, a library really shouldn't be trying to tell you want container to use, or to pick one for you, or make you jump through hoops to replace the container it uses. Thats just a great way to get all wrapped up in DI Hell where different libraries use different containers that initiazie at different times... bleh.

***

So yeah, I wanted a message bus libary that wasn't everything to everyone, that used SQL Transport and used familiar testable patterns, respected my container, and was something I could afford for personal projects... I wasn't finding that so I had to make one. :D

***

Project Logo by [LilyFie](https://lilyfie.com/)
