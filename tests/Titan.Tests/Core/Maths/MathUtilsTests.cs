using System.Numerics;
using NUnit.Framework;
using Titan.Core.Maths;

namespace Titan.Tests.Core.Maths;
internal class MathUtilsTests
{
    [TestCase(0, 0)]
    [TestCase(50, 25)]
    [TestCase(100, 50)]
    public void IsWithin_CursorIsWithin_ReturnTrue(int x, int y)
    {
        var result = MathUtils.IsWithin(Vector2.Zero, new(100, 50), new Point(x, y));

        Assert.That(result, Is.True);
    }


    [TestCase(100, 500)]
    [TestCase(150, 550)]
    [TestCase(200, 600)]
    public void IsWithin_CursorIsWithinWithOffset_ReturnTrue(int x, int y)
    {
        var result = MathUtils.IsWithin(new(100, 500), new(100, 100), new Point(x, y));

        Assert.That(result, Is.True);
    }
}
