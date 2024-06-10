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
                if (firstDifferingByteIndex != null) { errorMessage = $"first difference at byte {firstDifferingByteIndex}"; }
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
        var decoded = "bits 16\n" + string.Join('\n', InstructionParser.DecodeInstructions(binarySource));
        var newCompiledFilename = Guid.NewGuid().ToString();
        var newSourceFilename = $"{newCompiledFilename}.asm";
        File.WriteAllText(newSourceFilename, decoded);
        Run(NasmPath, newSourceFilename);
        var newBinarySource = File.ReadAllBytes(newCompiledFilename);
        var testSourcePath = testBinaryPath + ".asm";
        var recompiledTestBinaryFilename = Guid.NewGuid().ToString();
        Run(NasmPath, $"\"{testSourcePath}\" -o {recompiledTestBinaryFilename}");
        var recompiledTestBinary = File.ReadAllBytes(recompiledTestBinaryFilename);
        File.Delete(newCompiledFilename);
        File.Delete(newSourceFilename);
        File.Delete(recompiledTestBinaryFilename);
        if (recompiledTestBinary.Length != newBinarySource.Length) { return Math.Min(recompiledTestBinary.Length, newBinarySource.Length); }
        var firstDifferingByteIndex = Enumerable.Range(0, newBinarySource.Length)
            .FirstOrDefault(index => recompiledTestBinary[index] != newBinarySource[index], -1);
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
