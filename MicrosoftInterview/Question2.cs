public class Question2Tests
{
    [Test]
    public void CopyStringWorks()
    {
        var source = new byte[] { 1, 2, 3, 0 };
        var destination = new byte[] { 255, 255, 255, 255 };
        Question2.CopyString(source, destination);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 0 }, destination);
    }
}

public static class Question2
{
    public static void CopyString(byte[] from, byte[] to)
    {
        for (var i = 0; ; i++)
        {
            to[i] = from[i];
            if (from[i] == 0) { return; }
        }
    }
}
