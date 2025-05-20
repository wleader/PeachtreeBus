using System;

namespace PeachtreeBus.DatabaseSharing;

public class SharedDatabaseException(string message) : Exception(message);
