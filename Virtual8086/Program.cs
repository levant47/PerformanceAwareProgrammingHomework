public static class Program
{
    private static readonly string DataFolderPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\Data";
    private static readonly string NasmPath = $@"{DataFolderPath}\nasm.exe";
    private static readonly string[] TestBinaries =
    [
        $@"{DataFolderPath}\listing_0037_single_register_mov",
        $@"{DataFolderPath}\listing_0038_many_register_mov",
    ];

    public static void Main()
    {
        var anyTestFailed = false;
        foreach (var testBinary in TestBinaries)
        {
            if (!Verify(testBinary))
            {
                Console.WriteLine($"{testBinary} test failed");
                anyTestFailed = true;
            }
        }
        if (!anyTestFailed) { Console.WriteLine("All tests passed!"); }
    }

    private static bool Verify(string testBinaryPath)
    {
        var binarySource = File.ReadAllBytes(testBinaryPath);
        var decoded = InstructionParser.DecodeInstructions(binarySource);
        var newCompiledFilename = Guid.NewGuid().ToString();
        var newSourceFilename = $"{newCompiledFilename}.asm";
        File.WriteAllText(newSourceFilename, decoded);
        Run(NasmPath, newSourceFilename);
        var newBinarySource = File.ReadAllBytes(newCompiledFilename);
        File.Delete(newCompiledFilename);
        File.Delete(newSourceFilename);
        return binarySource.SequenceEqual(newBinarySource);
    }

    private static void Run(string program, string arguments)
    {
        var process = new Process { StartInfo = new() { FileName = program, Arguments = arguments } };
        process.Start();
        process.WaitForExit();
    }
}
