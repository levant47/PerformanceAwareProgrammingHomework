﻿public class Question4Tests
{
    [Test]
    public void OutlineCircleWorks()
    {
        var side = 51;
        var bitmap = new byte[side * side];
        Question4.OutlineCircle(side / 2, side / 2, side / 2, bitmap, side);
        // you can view the .pbm file as an image in Gimp
        CollectionAssert.AreEqual(ParsePbm(File.ReadAllText(@"test files\circle51x51.pbm")), bitmap);
    }

    private static byte[] ParsePbm(string source) =>
        source.Split("\r\n")
            .Skip(2)
            .Where(line => line != "")
            .SelectMany(line => line.Select(c => c == '1' ? (byte)1 : (byte)0))
            .ToArray();
}

public static class Question4
{
    // midpoint circle algorithm
    public static void OutlineCircle(int cX, int cY, int r, byte[] bitmap, int pitch)
    {
        var signedDistance = 0;
        for (int x = r, y = 0; y < x; )
        {
            bitmap[(y + cY) * pitch + x + cX] = 1;
            bitmap[(-y + cY) * pitch + x + cX] = 1;
            bitmap[(y + cY) * pitch - x + cX] = 1;
            bitmap[(-y + cY) * pitch - x + cX] = 1;
            bitmap[(x + cY) * pitch + y + cX] = 1;
            bitmap[(-x + cY) * pitch + y + cX] = 1;
            bitmap[(x + cY) * pitch - y + cX] = 1;
            bitmap[(-x + cY) * pitch - y + cX] = 1;

            signedDistance += 2 * y + 1;
            if (Math.Abs(signedDistance - 2 * x + 1) < Math.Abs(signedDistance))
            {
                signedDistance += -2 * x + 1;
                x--;
            }
            y++;
        }
    }

    /* my first implementation
    public static void OutlineCircle(int cX, int cY, int r, byte[] bitmap, int pitch)
    {
        // x^2 + y^2 = r^2 -> y^2 = r^2 - x^2 -> y = sqrt(r^2 - x^2)
        var nextY = 0;
        for (var x = -r; x <= 0; x++)
        {
            var y = nextY;
            nextY = (int)Math.Round(Math.Sqrt(r * r - (x + 1) * (x + 1)));
            do
            {
                // fill all 4 quarters
                bitmap[(y + cY) * pitch + x + cX] = 1;
                bitmap[(-y + cY) * pitch + x + cX] = 1;
                bitmap[(y + cY) * pitch - x + cX] = 1;
                bitmap[(-y + cY) * pitch - x + cX] = 1;
                y++; // more than one iteration if there is a gap between points
            }
            while (y < nextY);
        }
    }
    */
}
