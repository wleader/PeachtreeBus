﻿using System;

namespace PeachtreeBus;

/// <summary>
/// A base type for all exceptionions thrown by the library.
/// </summary>
/// <param name="message"></param>
public abstract class PeachtreeBusException(string message) : Exception(message);
