public class Question3Tests
{
    [Test]
    public void ContainsColorWorks()
    {
        var allColors = new byte[] { 0b00, 0b01, 0b10, 0b11 };
        var allBytes = Enumerable.Range(0, 256).Select(n => (byte)n).ToList();
        var allInputs = allColors.SelectMany(color => allBytes.Select(pixel => new { Pixel = pixel, Color = color })).ToList();
        CollectionAssert.AreEqual(
            allInputs.Select(input => new { input.Pixel, input.Color, Result = Question3.ContainsColorTrivial(input.Pixel, input.Color) }),
            allInputs.Select(input => new { input.Pixel, input.Color, Result = Question3.ContainsColor(input.Pixel, input.Color) })
        );
    }
}

public static class Question3
{
    // I don't think this version is really any faster than the loop one (assuming the compiler unrolls the loop in the loop version)
    public static bool ContainsColor(byte pixel, byte color)
    {
        // pack the color into a byte with 4 pixels of the same color
        var color4 = color | (color << 2) | (color << 4) | (color << 6);
        // compute the difference between all 4 pairs of pixels
        var difference = pixel ^ color4;
        // if all four pixels in the difference have either their odd or their even bit set to 1, then the pixel did not contain our target color;
        // so we are looking to see if there is at least one pixel with both its odd and even bits set to 0, and in order to do that, we perform a
        // logical OR on all 4 pairs of bits which we do by processing odd bits as one argument of the OR and even bits as the other
        var mask = 0b10101010;
        return ((difference & mask) | ((difference << 1) & mask)) != mask;
    }

    public static bool ContainsColorTrivial(byte pixel, byte color)
    {
        for (var i = 0; i < 4; i++, pixel >>= 2)
        {
            if ((pixel & 0b11) == color)
            {
                return true;
            }
        }
        return false;
    }
}
