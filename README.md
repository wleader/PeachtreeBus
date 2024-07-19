# PeachtreeBus
A Message Bus Library

## What's New

### July 19th, 2024
SQL Performance Update

The last couple weeks have forced a level-up in SQL skills, and as a result some of those lessons learned have been applied to PeachtreeBus.

The bus now creates a new connection object between each message processed. This means that the connection is properly reset between messages. Normally this is not needed but hypothetically the application code could do something like create an SQL temporary table and expect it to be cleaned up automatically, but since before the SQL session did not end between messages the temporary table did not get cleaned up. Anyway, you can expect the sp_reset_connection system stored procedure to be run in between messages now, which is automagically done for us by the connection pooling in SqlClient.

The second change is that all the SQL batches that the library uses have been reviewed and had locking hints added to them. Locking hints help SQL server understand how much data should be locked by a given client. By locking less data, it is less likely that one message processing thread will lock data that is needed by another message processing thread and cause the second message processor to wait for the data to unlock. This can improve performance in applications with a higher volume of messages. A low volume application might not notice any difference.

The third change is improvements to operations where a message is moved between tables. Previously these were multi-step operations where the data was read into variables, inserted into the destination, then deleted from the origin. It works, but there is a small chance for a race-condition between operations. These have been re-worked. It is somewhat upside down from how programmers usually think. First it does the delete operation, outputting the values of the delete operation, and the output data is selected, and provided to an insert operation. The upside here is that it is now a single operation so there is no time in between for weirdness, and SQL doesn't have to deal with any variables, so SQL should be able to perform this task a bit more efficiently.

SQL Statements for the most part now use explicit column names. This is a bit of future proofing in case there is ever a future upgrade that changes the column order.

Dependencies have also been updated to their latest versions, including at least one package that had a known security vulnerability.

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
