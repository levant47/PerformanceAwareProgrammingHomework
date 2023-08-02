public class Question3Tests
{
    [Test]
    [TestCase(0b10, 0b10, true)]
    [TestCase(0b01010110, 0b10, true)]
    [TestCase(0b10_11_11_11, 0b10, true)]
    [TestCase(0b11_01_11_01, 0b10, false)]
    [TestCase(0, 0, true)]
    [TestCase(0b00101101, 0, true)]
    public void ContainsColorWorks(byte pixel, byte color, bool expected) => Assert.AreEqual(expected, Question3.ContainsColor(pixel, color));
}

public static class Question3
{
    // I don't think this version is really any faster than the loop one (assuming the compiler unrolls the loop in the)
    public static bool ContainsColor(byte pixel, byte color)
    {
        // pack the color into a byte with 4 pixels of the same color
        var color4 = color | (color << 2) | (color << 4) | (color << 6);
        // compute the difference between all 4 pairs of pixels
        var difference = pixel ^ color4;
        // if all four pixels in the difference have either their odd or their even bit set to 1, then the pixel did not contain our target color;
        // so we are looking to see if there is at least one pixel with both its odd and even bits set to 0
        var oddBits = 0b10101010; var evenBits = 0b01010101;
        return (difference & oddBits) != oddBits && (difference & evenBits) != evenBits;
    }

    /* first version:
    public static bool ContainsColor(byte pixel, byte color)
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
    */
}
