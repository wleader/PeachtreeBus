using System;

namespace PeachtreeBus.Testing;

/// <summary>
/// An Exception object that can be tested for.
/// A test can setup a throw that throws this, and verify the thrown exception.
/// </summary>
public class TestException : Exception;
