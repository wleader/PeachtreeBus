## What's Old

### July 19th, 2024
SQL Performance Update

The last couple weeks have forced a level-up in SQL skills, and as a result some of those lessons learned have been applied to PeachtreeBus.

The bus now creates a new connection object between each message processed. This means that the connection is properly reset between messages. Normally this is not needed but hypothetically the application code could do something like create an SQL temporary table and expect it to be cleaned up automatically, but since before the SQL session did not end between messages the temporary table did not get cleaned up. Anyway, you can expect the sp_reset_connection system stored procedure to be run in between messages now, which is automagically done for us by the connection pooling in SqlClient.

The second change is that all the SQL batches that the library uses have been reviewed and had locking hints added to them. Locking hints help SQL server understand how much data should be locked by a given client. By locking less data, it is less likely that one message processing thread will lock data that is needed by another message processing thread and cause the second message processor to wait for the data to unlock. This can improve performance in applications with a higher volume of messages. A low volume application might not notice any difference.

The third change is improvements to operations where a message is moved between tables. Previously these were multi-step operations where the data was read into variables, inserted into the destination, then deleted from the origin. It works, but there is a small chance for a race-condition between operations. These have been re-worked. It is somewhat upside down from how programmers usually think. First it does the delete operation, outputting the values of the delete operation, and the output data is selected, and provided to an insert operation. The upside here is that it is now a single operation so there is no time in between for weirdness, and SQL doesn't have to deal with any variables, so SQL should be able to perform this task a bit more efficiently.

SQL Statements for the most part now use explicit column names. This is a bit of future proofing in case there is ever a future upgrade that changes the column order.

Dependencies have also been updated to their latest versions, including at least one package that had a known security vulnerability.

### March 31st, 2024

Easter Update 0.10.3

It turns out that there is a use for the application code to access the message object via the QueueContext and SubscribedContext in pipeline steps. Since the message itself really does belong to the application code, the Message property has been added back to the context objects.

### March 16th, 2024

Child Scopes Update 0.10.2

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