# PeachtreeBus
A Message Bus Library

## What's New

### March 15th, 2024

Child Scopes Update 0.10.1

The big change here is that the library was not starting a new Dependency injection scope before building the pipelines and handlers. This meant that the instances of handlers, and pipeline steps, and anything they depended on were not being newly created for each message. This meant that there could be weird side effects because objects were not in freshly initialized states. After this change, a new Dependency Injection scope is used for each message, ensuring that that objects used by handlers and pipeline steps are fresh and clean.

There was also a change to the QueueContext and SubscribedContext object to reduce what is directly accessible in the handlers. You while you can still access these properties by casting the context objects, you probably shouldn't. The now hidden context properties were only ever intended for internal use. Hopefully no one was using those properties and this won't break things for anyone.

Notice that the Minor version number has changed. Things are just different enough that a little bit of caution is warranted when taking this update.

### Fabruary 25th, 2024

Packages Update 0.9.10

One of the referenced packages Microsoft.Data.SqlClient version 5.1.2 had a known vulnerability so its been updated. Other packages have been updated too. 

### January 12th, 2024

Bug Fix Update 0.9.9

* Connections to the SQL Server from Asp.Net Core are not released ([1](https://github.com/wleader/PeachtreeBus/issues/1))

### December 18th, 2023

SqlClient Update 0.9.8

Out with the old, in with the new. Microsoft does all their SqlClient stuff in the Microsoft.Data.SqlClient package, and isn't updating System.Data.SqlClient anymore. So to keep up with the times, PeachtreeBus is switching too.

Other updates in this release:
* Update to the latest versions of dependent packages
* Use of the MissingInterfaceException when a message does not implement the required interface. 

### June 7th, 2023

Deadlock and Disconnect Recovery 0.9.7

When a message fails because of an unhandled exception, normally the current database transaction is rolled back, and the message is tried again. However if the unhandled exception is from a database deadlock, the current database transaction can not be rolled back. Now when the handling a failed message and the rollback fails, the transaction will be recreated allowing the processing of messages to continue.

Additionally, the way the connection to the database server is established has been changed, and so if after a failure the connection to the database is no longer usable a new connection will be established. This allows the process to continue to attempt to reconnect and to resume processing messages after a connection to the database server is broken.

### March 9th, 2023

Management Data Access & FailedMessageHandlers 0.9.6

A ManagementDataAccess class has been added. This class implements and interface with methods that will allow an application to look at the contents of the Pending, Completed, and Failed tables for both Queues and Subscription data. There are also methods for Cancelling pending messages, and Retrying failed messages.

While not a complete management system this should provide the needed functionality for an application to provide message management by whatever user interface it deems appropriate.

Failed Message Handlers Have been Added. By Providing IHandleFailedQueueMessages and IHandleFailedSubscriptionMessages to the the Dependency Injection provider, application specific code can be run when a message exceeds its maximum retries. Like regular message handlers, if the application specific code fails, there will be a database rollback. DefaultFailedQueueMessageHandler and DefaultFailedSubscribedMessageHandler which do nothing can be registered to not use this functionality.

### December 25th, 2022

Message Pipelines! 0.9.5

Message Pipelines are a way to insert code that can run before and/or after each message's handlers are invoked. 

To use message pipelines, you'll need to implement IQueuePipelineStep for code that you want to run on queued messages, and ISubscribedPipelineStep for subscription messages. Note that pipeline steps have a priority property so if you have more than one step, you can control the order they are used. Lower numbers are at the top, and invoke higher numbers, until the message handlers are invoked.

You'll also need the IFindQueuedPipelineSteps and IFindSubscribedPipelineSteps. An implementation of these for Simple Injector has been included in the PeachtreeBus.SimpleInjector package. 

The Example Project has been updated to include a very basic set of Pipeline steps.

### November 13th, 2022

There has been a minor update, 0.9.4.

This release doesn't really change much but there are a few changes. There have been some tweaks to some async-await code that hopefully will provide better exception data when something does go wrong.

Referenced packages have been updated.

Use of ApplicationException has been replaced with Library specific exceptions, You probably were not catching these before so you probably won't notice a difference in behavior here.

The big change is the change to using Microsoft.Extensions.Logging, and the use of structured logging internally. This means that your DI container will need to povide a Microsoft.Extensions.Logging.ILogger<>. This change should result in a tiny performance improvement, since logging code will waste less effort generating strings that go unused. You'll have better control over how much logging you want to capture from the library, since the Microsoft Logging Extensions give pretty good control over what namespaces you want to record log messages from.

### March 5th, 2022

There has been a major rework of things. Release 0.9.3! Woo!

Functionally the big change is that the Publish-Subscribe feature has been added. There were also some changes to the way that the main tasks are run. Its no longer a collection of Tasks with an AwaitAll, rather these main tasks are now run on their own threads. Neither of these changes is likely to break anything for any existing users.

The message context is not longer able to send messages. You'll need to inject the QueueWriter into your handlers and sagas.

The backing tables have had a lot of things renamed to make their purpose and usage clearer. Sorry. That's a big pain for anyone that needs to migrate something. But it really is better for the library to get this over with now while the number of affected users is low. (Fortunately  I think I am the only person using this library so I hope I'm only shooting my own foot.)

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
