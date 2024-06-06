public static class Program
{
    private static readonly string DataFolderPath = $@"{AppDomain.CurrentDomain.BaseDirectory}Data";
    private static readonly string NasmPath = $@"{DataFolderPath}\nasm.exe";

    public static void Main()
    {
        var anyTestFailed = false;
        foreach (var testBinary in GetAllFilesWithoutExtension(DataFolderPath))
        {
            string? errorMessage = null;
            try
            {
                var firstDifferingByteIndex = Verify(testBinary);
                if (firstDifferingByteIndex != null) { errorMessage = "first difference at byte {firstDifferingByteIndex}"; }
            }
            catch (Exception exception) { errorMessage = exception.Message; }
            if (errorMessage != null)
            {
                anyTestFailed = true;
                Console.WriteLine($"{Path.GetFileName(testBinary)} test failed: {errorMessage}");
            }
        }
        if (!anyTestFailed) { Console.WriteLine("All tests passed!"); }
    }

    private static int? Verify(string testBinaryPath)
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
        var firstDifferingByteIndex = Enumerable.Range(0, binarySource.Length)
            .FirstOrDefault(index => binarySource[index] != newBinarySource[index], -1);
        if (firstDifferingByteIndex == -1) { return null; }
        return firstDifferingByteIndex;
    }

    private static void Run(string program, string arguments)
    {
        var process = new Process { StartInfo = new() { FileName = program, Arguments = arguments } };
        process.Start();
        process.WaitForExit();
    }

    private static IEnumerable<string> GetAllFilesWithoutExtension(string folderPath) =>
        Directory.EnumerateFiles(folderPath).Where(filePath => !Path.GetFileName(filePath).Contains('.'));
}
