using NUnit.Framework;
using Titan.Core;

namespace Titan.Tests;

public unsafe class TitanBufferSizeTests
{
    [Test]
    public void SizeOf_TitanArray_AlwaysReturn12()
    {
        var result = sizeof(TitanArray<byte>);

        Assert.That(result, Is.EqualTo(12));
    }

    [Test]
    public void SizeOf_TitanBuffer_AlwaysReturn12()
    {
        var result = sizeof(TitanBuffer);

        Assert.That(result, Is.EqualTo(12));
    }
}