namespace Titan.Tools.AssetProcessor.Parsers.WavefrontObj;

internal class ParserException(string message, in Token token) : Exception($"{token.Line}:{token.Column} - {message}");
