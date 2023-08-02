public class Question1Tests
{
    [Test]
    public void CopyRectWorks()
    {
        // note that source and destination have a different pitch
        var source = new byte[]
        {
            0, 0, 0, 0,
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0
        };
        var destination = new byte[]
        {
            0, 0, 0,
            0, 0, 0,
            0, 0, 0,
            0, 0, 0,
        };
        Question1.CopyRect(
            source,
            /* source pitch: */ 4,
            destination,
            /* destination pitch */ 3,
            /* source start x, y: */ 0, 1,
            /* source end x, y: */ 3, 4,
            /* destination start x, y: */ 0, 1
        );
        CollectionAssert.AreEqual(new byte[]
        {
            0, 0, 0,
            1, 2, 3,
            4, 5, 6,
            7, 8, 9,
        }, destination);
    }
}

public static class Question1
{
    public static void CopyRect(
        byte[] bufferA,
        int pitchA,
        byte[] bufferB,
        int pitchB,
        int fromMinX,
        int fromMinY,
        int fromMaxX,
        int fromMaxY,
        int toMinX,
        int toMinY
    )
    {
        for (var y = 0; y < fromMaxY - fromMinY; y++)
        {
            for (var x = 0; x < fromMaxX - fromMinX; x++)
            {
                bufferB[(y + toMinY) * pitchB + x + toMinX] = bufferA[(y + fromMinY) * pitchA + x + fromMinX];
            }
        }
    }
}
