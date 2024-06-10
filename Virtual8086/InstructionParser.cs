public class InstructionParser(byte[] _source)
{
    private enum MovMode : byte
    {
        MemoryNoDisplacement = 0b00,
        Memory8BitDisplacement = 0b01,
        Memory16BitDisplacement = 0b10,
        Register = 0b11,
    }

    private enum InterpretedMovModeType
    {
        Register,
        AddressWithNoDisplacement,
        AddressWithDisplacement,
        DirectAddress,
    }

    private record InterpretedMovMode(InterpretedMovModeType Type, ushort Displacement);

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

    private bool IsDone() => _i == _source.Length;

    private byte Next()
    {
        var result = _source[_i];
        _i++;
        return result;
    }

    private Instruction ParseInstruction()
    {
        var byte1 = Next();
        if (GetBits(2, 8, byte1) == (byte)InstructionOpcode.MoveRegisterMemoryToFromMemory)
        {
            var isDestinationInRegField = GetBit(1, byte1);
            var isWide = GetBit(0, byte1);

            var byte2 = Next();
            var mod = ToEnum<MovMode>(GetBits(6, 8, byte2));
            var reg = GetBits(3, 6, byte2);
            var rm = GetBits(0, 3, byte2);

            var interpretedMovMode = InterpretMovMode(mod, rm);

            return (isWide, isDestinationInRegField, interpretedMovMode) switch
            {
                (true, false, { Type: InterpretedMovModeType.Register }) => new()
                {
                    Type = InstructionType.MoveRegisterToRegister16,
                    DestinationRegister16 = ToEnum<Register16>(rm),
                    SourceRegister16 = ToEnum<Register16>(reg),
                },
                (true, true, { Type: InterpretedMovModeType.Register }) => new()
                {
                    Type = InstructionType.MoveRegisterToRegister16,
                    DestinationRegister16 = ToEnum<Register16>(reg),
                    SourceRegister16 = ToEnum<Register16>(rm),
                },
                (false, false, { Type: InterpretedMovModeType.Register }) => new()
                {
                    Type = InstructionType.MoveRegisterToRegister8,
                    DestinationRegister8 = ToEnum<Register8>(rm),
                    SourceRegister8 = ToEnum<Register8>(reg),
                },
                (false, true, { Type: InterpretedMovModeType.Register }) => new()
                {
                    Type = InstructionType.MoveRegisterToRegister8,
                    DestinationRegister8 = ToEnum<Register8>(reg),
                    SourceRegister8 = ToEnum<Register8>(rm),
                },
                (true, true, { Type: InterpretedMovModeType.AddressWithNoDisplacement }) => new()
                {
                    Type = InstructionType.MoveMemoryToRegister16,
                    DestinationRegister16 = ToEnum<Register16>(reg),
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                },
                (false, true, { Type: InterpretedMovModeType.AddressWithNoDisplacement }) => new()
                {
                    Type = InstructionType.MoveMemoryToRegister8,
                    DestinationRegister8 = ToEnum<Register8>(reg),
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                },
                (true, false, { Type: InterpretedMovModeType.AddressWithNoDisplacement }) => new()
                {
                    Type = InstructionType.MoveRegister16ToMemory,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    SourceRegister16 = ToEnum<Register16>(reg),
                },
                (false, false, { Type: InterpretedMovModeType.AddressWithNoDisplacement }) => new()
                {
                    Type = InstructionType.MoveRegister8ToMemory,
                    SourceRegister8 = ToEnum<Register8>(reg),
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                },
                (true, true, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MoveMemoryToRegister16WithDisplacement,
                    DestinationRegister16 = ToEnum<Register16>(reg),
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    Displacement = displacement,
                },
                (false, true, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MoveMemoryToRegister8WithDisplacement,
                    DestinationRegister8 = ToEnum<Register8>(reg),
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    Displacement = displacement,
                },
                (true, false, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MoveRegister16ToMemoryWithDisplacement,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    SourceRegister16 = ToEnum<Register16>(reg),
                    Displacement = displacement,
                },
                (false, false, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MoveRegister8ToMemoryWithDisplacement,
                    SourceRegister8 = ToEnum<Register8>(reg),
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    Displacement = displacement,
                },
                (true, true, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MoveDirectAddressToRegister16,
                    DestinationRegister16 = ToEnum<Register16>(reg),
                    Displacement = displacement,
                },
                (false, true, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MoveDirectAddressToRegister8,
                    DestinationRegister8 = ToEnum<Register8>(reg),
                    Displacement = displacement,
                },
                (true, false, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MoveRegister16ToDirectAddress,
                    SourceRegister16 = ToEnum<Register16>(reg),
                    Displacement = displacement,
                },
                (false, false, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MoveRegister8ToDirectAddress,
                    SourceRegister8 = ToEnum<Register8>(reg),
                    Displacement = displacement,
                },
                _ => throw new(),
            };
        }
        else if (GetBits(4, 8, byte1) == (byte)InstructionOpcode.MoveImmediateToRegister)
        {
            var reg = GetBits(0, 3, byte1);
            var isWide = GetBit(3, byte1);
            ushort data = Next();
            if (isWide) { data = (ushort)((Next() << 8) | data); }
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
        else if (GetBits(1, 8, byte1) == (byte)InstructionOpcode.MoveImmediateToRegisterOrMemory)
        {
            var isWide = GetBit(0, byte1);
            var byte2 = Next();

            var mod = ToEnum<MovMode>(GetBits(6, 8, byte2));
            var rm = GetBits(0, 3, byte2);

            var interpretedMovMode = InterpretMovMode(mod, rm);
            ushort data = Next();
            if (isWide) { data = (ushort)((Next() << 8) | data); }

            return (isWide, interpretedMovMode) switch
            {
                (false, { Type: InterpretedMovModeType.Register }) => new()
                {
                    Type = InstructionType.MoveImmediateToRegister8,
                    Immediate = data,
                    DestinationRegister8 = ToEnum<Register8>(rm),
                },
                (false, { Type: InterpretedMovModeType.AddressWithNoDisplacement }) => new()
                {
                    Type = InstructionType.MoveImmediate8ToMemory,
                    Immediate = data,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                },
                (false, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MoveImmediate8ToMemoryWithDisplacement,
                    Immediate = data,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    Displacement = displacement,
                },
                (false, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MoveImmediate8ToDirectAddress,
                    Immediate = data,
                    Displacement = displacement,
                },
                (true, { Type: InterpretedMovModeType.Register }) => new()
                {
                    Type = InstructionType.MoveImmediateToRegister16,
                    Immediate = data,
                    DestinationRegister16 = ToEnum<Register16>(rm),
                },
                (true, { Type: InterpretedMovModeType.AddressWithNoDisplacement }) => new()
                {
                    Type = InstructionType.MoveImmediate16ToMemory,
                    Immediate = data,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                },
                (true, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MoveImmediate16ToMemoryWithDisplacement,
                    Immediate = data,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    Displacement = displacement,
                },
                (true, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MoveImmediate16ToDirectAddress,
                    Immediate = data,
                    Displacement = displacement,
                },
                _ => throw new()
            };
        }
        else if (GetBits(1, 8, byte1) == (byte)InstructionOpcode.MoveMemoryToAccumulator)
        {
            var isWide = GetBit(0, byte1);
            var address = NextWord();
            return new()
            {
                Type = isWide ? InstructionType.MoveDirectAddressToAccumulator16 : InstructionType.MoveDirectAddressToAccumulator8,
                Displacement = address,
            };
        }
        else if (GetBits(1, 8, byte1) == (byte)InstructionOpcode.MoveAccumulatorToMemory)
        {
            var isWide = GetBit(0, byte1);
            var address = NextWord();
            return new()
            {
                Type = isWide ? InstructionType.MoveAccumulator16ToDirectAddress : InstructionType.MoveAccumulator8ToDirectAddress,
                Displacement = address,
            };
        }
        else { throw new($"Unrecognized beginning of an instruction: {Convert.ToString(byte1, 2).PadLeft(8, '0')}"); }
    }

    private InterpretedMovMode InterpretMovMode(MovMode mod, byte rm) => (mod, rm) switch
    {
        (MovMode.Register, _) => new(InterpretedMovModeType.Register, 0),
        (MovMode.MemoryNoDisplacement, 0b110) => new(InterpretedMovModeType.DirectAddress, NextWord()),
        (MovMode.MemoryNoDisplacement, _) => new(InterpretedMovModeType.AddressWithNoDisplacement, 0),
        (MovMode.Memory8BitDisplacement, _) => new(InterpretedMovModeType.AddressWithDisplacement, Next()),
        (MovMode.Memory16BitDisplacement, _) => new(InterpretedMovModeType.AddressWithDisplacement, NextWord()),
        _ => throw new(),
    };

    private ushort NextWord()
    {
        var low = Next();
        var high = Next();
        return (ushort)((high << 8) | low);
    }

    private static bool GetBit(int index, byte value) => GetBits(index, index + 1, value) == 1;

    private static byte GetBits(int startIndex, int endIndex, byte value)
    {
        var mask = 0b11111111 >> (8 - endIndex);
        return (byte)((value & mask) >> startIndex);
    }

}
