# Using SqlPackage on Linux

This document describes how to use the SqlPackage tool on linux.
PeachtreeBus provides example database structures as .sqlproj projects.
These projects can be compiled into a .dacpac file.

## Installation
You will have to have the dotnet SDK installed. 
If the SDK is installed, the tool can be installed by
using the following command.

```bash
dotnet tool install -g microsoft.sqlpackage
```

## Usage
To use the tool you will need to know a connection string for the database
that will be updated.

It is assumed that you are running Microsoft SQL Server on linux, and have
created a database and configured SQL Authentication with a user that has 
the appropriate permissions to that database.

### Generate an Upgrade Script
The following command can be used to generate a .sql file that will contain
the command that will be run on the server. This is a good way to preview
what the affect of publishing the .dacpac will be.

You will want to modify the paths, server, database, user id, and password.

```
sqlpackage /Action:Script /SourceFile:"/path/to/database.dacpac" /TargetConnectionString:"Server=YOURSERVER;Database=YOURDB;User Id=YOURUSER;Password=YOURPASS;TrustServerCertificate=True" /OutputPath:"/path/to/deployment_script.sql"
```

### Publish - Apply Changes
Once you are satisified that the script will do what you expect, you can publish.
Publish will effectively run the same statements that the upgrade script contains.

```
sqlpackage /Action:Publish /SourceFile:"/path/to/database.dacpac" /TargetConnectionString:"Server=YOURSERVER;Database=YOURDB;User Id=YOURUSER;Password=YOURPASS;TrustServerCertificate=True"
```

If the above command completes without error, then your database has been updated to
match the specification contained in the .dacpac, and the database is ready for use.
