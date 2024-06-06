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

    public static List<Instruction> DecodeInstructions(byte[] source)
    {
        var parser = new InstructionParser(source);
        var result = new List<Instruction>();
        while (!parser.IsDone())
        {
            try { result.Add(parser.ParseInstruction()); }
            catch (Exception exception)
            { throw new($"Encountered an error parsing byte {parser._i}: {exception.Message}"); }
        }
        return result;
    }

    private bool IsDone() => _i == source.Length;

    private byte Current() => source[_i];

    private void Next() => _i++;

    private Instruction ParseInstruction()
    {
        var byte1 = Current();
        if (GetBits(2, 8, byte1) == (byte)InstructionOpcode.RegisterMemoryToFromMemory)
        {
            var isDestinationInRegField = GetBit(1, byte1);
            var isWide = GetBit(0, byte1);
            Next();

            var byte2 = Current();
            var mod = ToEnum<MovMode>(GetBits(6, 8, byte2));
            var reg = GetBits(3, 6, byte2);
            var rm = GetBits(0, 3, byte2);
            Next();

            if (mod == MovMode.Register)
            {
                if (isWide)
                {
                    return new()
                    {
                        Type = InstructionType.MoveRegisterToRegister16,
                        DestinationRegister16 = ToEnum<Register16>(isDestinationInRegField ? reg : rm),
                        SourceRegister16 = ToEnum<Register16>(isDestinationInRegField ? rm : reg),
                    };
                }
                else
                {
                    return new()
                    {
                        Type = InstructionType.MoveRegisterToRegister8,
                        DestinationRegister8 = ToEnum<Register8>(isDestinationInRegField ? reg : rm),
                        SourceRegister8 = ToEnum<Register8>(isDestinationInRegField ? rm : reg),
                    };
                }
            }
            if (mod == MovMode.MemoryNoDisplacement)
            {
                return (isWide, isDestinationInRegField) switch
                {
                    (true, true) => new()
                    {
                        Type = InstructionType.MoveMemoryToRegister16,
                        DestinationRegister16 = ToEnum<Register16>(reg),
                        Address = ToEnum<EffectiveAddressCalculation>(rm),
                    },
                    (false, true) => new()
                    {
                        Type = InstructionType.MoveMemoryToRegister8,
                        DestinationRegister8 = ToEnum<Register8>(reg),
                        Address = ToEnum<EffectiveAddressCalculation>(rm),
                    },
                    (true, false) => new()
                    {
                        Type = InstructionType.MoveRegister16ToMemory,
                        Address = ToEnum<EffectiveAddressCalculation>(rm),
                        SourceRegister16 = ToEnum<Register16>(reg),
                    },
                    (false, false) => new()
                    {
                        Type = InstructionType.MoveRegister8ToMemory,
                        SourceRegister8 = ToEnum<Register8>(reg),
                        Address = ToEnum<EffectiveAddressCalculation>(rm),
                    },
                };
            }

            ushort displacement = Current();
            Next();
            if (mod == MovMode.Memory16BitDisplacement)
            {
                displacement = (ushort)((Current() << 8) | displacement);
                Next();
            }
            return (isWide, isDestinationInRegField) switch
            {
                (true, true) => new()
                {
                    Type = InstructionType.MoveMemoryToRegister16WithDisplacement,
                    DestinationRegister16 = ToEnum<Register16>(reg),
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    Displacement = displacement,
                },
                (false, true) => new()
                {
                    Type = InstructionType.MoveMemoryToRegister8WithDisplacement,
                    DestinationRegister8 = ToEnum<Register8>(reg),
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    Displacement = displacement,
                },
                (true, false) => new()
                {
                    Type = InstructionType.MoveRegister16ToMemoryWithDisplacement,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    SourceRegister16 = ToEnum<Register16>(reg),
                    Displacement = displacement,
                },
                (false, false) => new()
                {
                    Type = InstructionType.MoveRegister8ToMemoryWithDisplacement,
                    SourceRegister8 = ToEnum<Register8>(reg),
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    Displacement = displacement,
                },
            };
        }
        else if (GetBits(4, 8, byte1) == (byte)InstructionOpcode.ImmediateToRegister)
        {
            var reg = GetBits(0, 3, byte1);
            var isWide = GetBit(3, byte1);
            Next();
            ushort data = Current();
            Next();
            if (isWide)
            {
                data = (ushort)((Current() << 8) | data);
                Next();
            }
            if (isWide)
            {
                return new()
                {
                    Type = InstructionType.MoveImmediateToRegister16,
                    DestinationRegister16 = ToEnum<Register16>(reg),
                    Immediate = data,
                };
            }
            else
            {
                return new()
                {
                    Type = InstructionType.MoveImmediateToRegister8,
                    DestinationRegister8 = ToEnum<Register8>(reg),
                    Immediate = data,
                };
            }
        }
        else { throw new($"Unrecognized beginning of an instruction: {Convert.ToString(byte1, 2).PadLeft(8, '0')}"); }
    }

    private static bool GetBit(int index, byte value) => GetBits(index, index + 1, value) == 1;

    private static byte GetBits(int startIndex, int endIndex, byte value)
    {
        var mask = 0b11111111 >> (8 - endIndex);
        return (byte)((value & mask) >> startIndex);
    }

}
