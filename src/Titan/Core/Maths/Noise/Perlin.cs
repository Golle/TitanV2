

using System.Runtime.CompilerServices;

namespace Titan.Core.Maths.Noise;

public struct Perlin
{
    private Inline512<int> Permutations;
    public static Perlin Create(int seed)
    {
        const int PermutationCount = 256;
        Unsafe.SkipInit<Perlin>(out var perlin);
        var random = new Random(seed);


        Span<int> permutations = stackalloc int[PermutationCount];

        // Fill permutation with random values
        for (var i = 0; i < PermutationCount; i++)
        {
            permutations[i] = i;
        }

        // Shuffle the array
        for (var i = PermutationCount - 1; i > 0; i--)
        {
            var swapIndex = random.Next(i + 1);
            (permutations[i], permutations[swapIndex]) = (permutations[swapIndex], permutations[i]);
        }

        // Duplicate the array for overflow handling
        for (var i = 0; i < PermutationCount * 2; i++)
        {
            //NOTE(Jens): modulo operator can probably be replaced with a mask
            perlin.Permutations[i] = permutations[i % 256];
        }

        return perlin;
    }
    public readonly unsafe float Noise(float x, float y)
    {
        // Find unit square containing the point
        var X = (int)Math.Floor(x) & 255;
        var Y = (int)Math.Floor(y) & 255;

        // Relative coordinates in the square
        x -= (float)Math.Floor(x);
        y -= (float)Math.Floor(y);

        // Compute fade curves
        var u = Fade(x);
        var v = Fade(y);
        
        var p = Permutations.AsPointer();
        // Hash corners of the square
        var aa = p[p[X] + Y];
        var ab = p[p[X] + Y + 1];
        var ba = p[p[X + 1] + Y];
        var bb = p[p[X + 1] + Y + 1];

        // Blend results from the corners
        var result = Lerp(v,
            Lerp(u, Grad(aa, x, y), Grad(ba, x - 1, y)),
            Lerp(u, Grad(ab, x, y - 1), Grad(bb, x - 1, y - 1))
        );

        // Normalize result to range [0, 1]
        return (result + 1) / 2;
    }

    public readonly unsafe float Noise(float x, float y, float z)
    {
        //NOTE(Jens): This is a ChatGPT implementation. Revisit this later.
        // Find the unit cube that contains the point
        var X = (int)Math.Floor(x) & 255;
        var Y = (int)Math.Floor(y) & 255;
        var Z = (int)Math.Floor(z) & 255;

        // Find the relative coordinates in the cube
        x -= (float)Math.Floor(x);
        y -= (float)Math.Floor(y);
        z -= (float)Math.Floor(z);

        // Compute fade curves for each coordinate
        var u = Fade(x);
        var v = Fade(y);
        var w = Fade(z);

        var p = Permutations.AsPointer();
        // Hash coordinates of the cube corners
        var aaa = p[p[p[X] + Y] + Z];
        var aba = p[p[p[X] + Y + 1] + Z];
        var aab = p[p[p[X] + Y] + Z + 1];
        var abb = p[p[p[X] + Y + 1] + Z + 1];
        var baa = p[p[p[X + 1] + Y] + Z];
        var bba = p[p[p[X + 1] + Y + 1] + Z];
        var bab = p[p[p[X + 1] + Y] + Z + 1];
        var bbb = p[p[p[X + 1] + Y + 1] + Z + 1];

        // Blend results from the cube corners
        var result = Lerp(w,
            Lerp(v,
                Lerp(u, Grad(aaa, x, y, z), Grad(baa, x - 1, y, z)),
                Lerp(u, Grad(aba, x, y - 1, z), Grad(bba, x - 1, y - 1, z))),
            Lerp(v,
                Lerp(u, Grad(aab, x, y, z - 1), Grad(bab, x - 1, y, z - 1)),
                Lerp(u, Grad(abb, x, y - 1, z - 1), Grad(bbb, x - 1, y - 1, z - 1)))
        );

        // Normalize result to range [0, 1]
        return (result + 1) / 2;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Fade(float t)
    {
        // Improved smoothstep function
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Lerp(float t, float a, float b)
    {
        // Linear interpolation
        return a + t * (b - a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Grad(int hash, float x, float y, float z = 0f)
    {
        // Gradient calculation based on hash value
        var h = hash & 15; // Only 16 possible gradients
        var u = h < 8 ? x : y;
        var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}
