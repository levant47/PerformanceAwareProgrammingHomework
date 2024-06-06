public class InstructionParser(byte[] source)
{
    private enum MovMode : byte
    {
        MemoryNoDisplacement = 0b00,
        Memory8BitDisplacement = 0b01,
        Memory16BitDisplacement = 0b10,
        Register = 0b11,
    }

    private int _i = 0;

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

    private bool IsDone() => _i == source.Length;

    private byte Current() => source[_i];

    private void Next() => _i++;

    private Instruction ParseInstruction()
    {
        var byte1 = Current();
        var opcode = ToEnum<InstructionType>(GetBits(2, 8, byte1));
        if (opcode == InstructionType.Mov)
        {
            var isDestinationInRegField = GetBit(1, byte1);
            var isWide = GetBit(0, byte1);
            Next();

            var byte2 = Current();
            var mod = ToEnum<MovMode>(GetBits(6, 8, byte2));
            if (mod != MovMode.Register) { throw new("Only register to register moves are supported!"); }
            var reg = GetBits(3, 6, byte2);
            var rm = GetBits(0, 3, byte2);
            Next();

            var result = new Instruction { Type = InstructionType.Mov, IsWide = isWide };
            if (isWide)
            {
                result.Source16 = ToEnum<Register16>(isDestinationInRegField ? rm : reg);
                result.Destination16 = ToEnum<Register16>(isDestinationInRegField ? reg : rm);
            }
            else
            {
                result.Source8 = ToEnum<Register8>(isDestinationInRegField ? rm : reg);
                result.Destination8 = ToEnum<Register8>(isDestinationInRegField ? reg : rm);
            }
            return result;
        }
        else { throw new($"Unknown opcode: {Convert.ToString((byte)opcode, 2).PadLeft(6, '0')}"); }
    }

    private static bool GetBit(int index, byte value) => GetBits(index, index + 1, value) == 1;

    private static byte GetBits(int startIndex, int endIndex, byte value)
    {
        var mask = 0b11111111 >> (8 - endIndex);
        return (byte)((value & mask) >> startIndex);
    }

}
