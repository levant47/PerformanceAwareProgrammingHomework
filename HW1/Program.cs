using System.Diagnostics;
using System.Text;

public static class Program
{
    private static string NasmPath;

    public static void Main(string[] _)
    {
        var programDirectory = AppDomain.CurrentDomain.BaseDirectory;
        NasmPath = $"{programDirectory}/Data/nasm.exe";
        var sources = new[] { "listing_0037_single_register_mov", "listing_0038_many_register_mov" };
        bool anyTestFailed = false;
        foreach (var source in sources)
        {
            if (!Verify($"{programDirectory}/Data/{source}"))
            {
                Console.WriteLine($"{source} test failed");
                anyTestFailed = true;
            }
        }
        if (!anyTestFailed)
        {
            Console.WriteLine("All tests passed!");
        }
    }

    public static bool Verify(string sourcePath)
    {
        var binarySource = File.ReadAllBytes(sourcePath);
        var decoded = DecodeInstructions(binarySource);
        var newCompiledFilename = Guid.NewGuid().ToString();
        var newSourceFilename = $"{newCompiledFilename}.asm";
        File.WriteAllText(newSourceFilename, decoded);
        var process = new Process { StartInfo = new() { FileName = NasmPath, Arguments = newSourceFilename } };
        process.Start();
        process.WaitForExit();
        var newBinarySource = File.ReadAllBytes(newCompiledFilename);
        File.Delete(newCompiledFilename);
        File.Delete(newSourceFilename);
        return binarySource.SequenceEqual(newBinarySource);
    }

    public static string DecodeInstructions(byte[] source)
    {
        var parser = new InstructionParser(source);
        var instructions = new List<Instruction>();
        while (!parser.IsDone())
        {
            instructions.Add(parser.ParseInstruction());
        }
        var result = new StringBuilder();
        foreach (var instruction in instructions)
        {
            result.Append(instruction);
            result.Append('\n');
        }
        return result.ToString();
    }

    public static bool GetBit(int index, byte value) => (value & (1 << index)) == 1;

    public class InstructionParser
    {
        private readonly byte[] _source;
        private int _i;

        public InstructionParser(byte[] source)
        {
            _source = source;
            _i = 0;
        }

        public bool IsDone() => _i == _source.Length;

        public byte Current() => _source[_i];

        public void Next() => _i++;

        public Instruction ParseInstruction()
        {
            var byte1 = Current();
            var opcode = byte1 >> 2;
            if (opcode != (byte)InstructionType.Mov) { throw new($"Unknown opcode: {Convert.ToString(opcode, 2).PadLeft(6, '0')}"); }
            var isDestinationInRegField = GetBit(1, byte1);
            var isWide = GetBit(0, byte1);
            Next();

            var byte2 = Current();
            var mod = byte2 >> 6;
            if (mod != 0b11) { throw new($"Only register to register moves are supported!"); }
            var reg = (byte2 >> 3) & 0b111;
            var rm = byte2 & 0b111;
            Next();

            var result = new Instruction();
            result.Type = InstructionType.Mov;
            result.IsWide = isWide;
            if (isWide)
            {
                result.Source16 = (Register16)(isDestinationInRegField ? rm : reg);
                result.Destination16 = (Register16)(isDestinationInRegField ? reg : rm);
            }
            else
            {
                result.Source8 = (Register8)(isDestinationInRegField ? rm : reg);
                result.Destination8 = (Register8)(isDestinationInRegField ? reg : rm);
            }
            return result;
        }
    }

    public enum InstructionType
    {
        Mov = 0b100010,
    }

    public enum Register8
    {
        AL,
        CL,
        DL,
        BL,
        AH,
        CH,
        DH,
        BH,
    }

    public enum Register16
    {
        AX,
        CX,
        DX,
        BX,
        SP,
        BP,
        SI,
        DI,
    }

    public struct Instruction
    {
        public InstructionType Type;
        public bool IsWide;
        public Register8 Source8;
        public Register8 Destination8;
        public Register16 Source16;
        public Register16 Destination16;

        public override string ToString() =>
            Type.ToString().ToLower()
                + " "
                + (IsWide ? Destination16.ToString() : Destination8.ToString()).ToLower()
                + ", "
                + (IsWide ? Source16.ToString() : Source8.ToString()).ToLower();
    }
}

