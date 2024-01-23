using Titan.Core.Logging;

using var _ = Logger.Start<ConsoleLogger>();

Logger.Error<Program>($"Hello, World! From {typeof(Program).Assembly.GetName().Name}");
