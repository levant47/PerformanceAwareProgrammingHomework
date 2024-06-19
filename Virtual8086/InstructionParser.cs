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

    private record InterpretedMovMode(InterpretedMovModeType Type, short Displacement);

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

            return (isDestinationInRegField, interpretedMovMode) switch
            {
                (false, { Type: InterpretedMovModeType.Register }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.RegisterToRegister,
                    DestinationRegister = new(isWide, rm),
                    SourceRegister = new(isWide, reg),
                },
                (true, { Type: InterpretedMovModeType.Register }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.RegisterToRegister,
                    DestinationRegister = new(isWide, reg),
                    SourceRegister = new(isWide, rm),
                },
                (true, { Type: InterpretedMovModeType.AddressWithNoDisplacement }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.MemoryToRegister,
                    DestinationRegister = new(isWide, reg),
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                },
                (false, { Type: InterpretedMovModeType.AddressWithNoDisplacement }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.RegisterToMemory,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    SourceRegister= new(isWide, reg),
                },
                (true, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.MemoryToRegisterWithDisplacement,
                    DestinationRegister = new(isWide, reg),
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    Displacement = displacement,
                },
                (false, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.RegisterToMemoryWithDisplacement,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    SourceRegister = new(isWide, reg),
                    Displacement = displacement,
                },
                (true, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.DirectAddressToRegister,
                    DestinationRegister= new(isWide, reg),
                    Displacement = displacement,
                },
                (false, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.RegisterToDirectAddress,
                    SourceRegister = new(isWide, reg),
                    Displacement = displacement,
                },
                _ => throw new(),
            };
        }
        else if (GetBits(4, 8, byte1) == (byte)InstructionOpcode.MoveImmediateToRegister)
        {
            var reg = GetBits(0, 3, byte1);
            var isWide = GetBit(3, byte1);
            var data = isWide ? ParseSignedWord() : ParseSignedByte();
            return new()
            {
                Type = InstructionType.MOV,
                Operands = InstructionOperands.ImmediateToRegister,
                DestinationRegister = new(isWide, reg),
                Immediate = data,
            };
        }
        else if (GetBits(1, 8, byte1) == (byte)InstructionOpcode.MoveImmediateToRegisterOrMemory)
        {
            var isWide = GetBit(0, byte1);
            var byte2 = Next();

            var mod = ToEnum<MovMode>(GetBits(6, 8, byte2));
            var rm = GetBits(0, 3, byte2);

            var interpretedMovMode = InterpretMovMode(mod, rm);
            var data = isWide ? ParseSignedWord() : ParseSignedByte();

            return (isWide, interpretedMovMode) switch
            {
                (false, { Type: InterpretedMovModeType.Register }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.ImmediateToRegister,
                    Immediate = data,
                    DestinationRegister = new(isWide, rm),
                },
                (false, { Type: InterpretedMovModeType.AddressWithNoDisplacement }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.Immediate8ToMemory,
                    Immediate = data,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                },
                (false, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.Immediate8ToMemoryWithDisplacement,
                    Immediate = data,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    Displacement = displacement,
                },
                (false, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.Immediate8ToDirectAddress,
                    Immediate = data,
                    Displacement = displacement,
                },
                (true, { Type: InterpretedMovModeType.Register }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.ImmediateToRegister,
                    Immediate = data,
                    DestinationRegister = new(isWide, rm),
                },
                (true, { Type: InterpretedMovModeType.AddressWithNoDisplacement }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.Immediate16ToMemory,
                    Immediate = data,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                },
                (true, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.Immediate16ToMemoryWithDisplacement,
                    Immediate = data,
                    Address = ToEnum<EffectiveAddressCalculation>(rm),
                    Displacement = displacement,
                },
                (true, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }) => new()
                {
                    Type = InstructionType.MOV,
                    Operands = InstructionOperands.Immediate16ToDirectAddress,
                    Immediate = data,
                    Displacement = displacement,
                },
                _ => throw new()
            };
        }
        else if (GetBits(1, 8, byte1) == (byte)InstructionOpcode.MoveMemoryToAccumulator)
        {
            var isWide = GetBit(0, byte1);
            var address = ParseSignedWord();
            return new()
            {
                Type = InstructionType.MOV,
                Operands = isWide ? InstructionOperands.DirectAddressToAccumulator16 : InstructionOperands.DirectAddressToAccumulator8,
                Displacement = address,
            };
        }
        else if (GetBits(1, 8, byte1) == (byte)InstructionOpcode.MoveAccumulatorToMemory)
        {
            var isWide = GetBit(0, byte1);
            var address = ParseSignedWord();
            return new()
            {
                Type = InstructionType.MOV,
                Operands = isWide ? InstructionOperands.Accumulator16ToDirectAddress : InstructionOperands.Accumulator8ToDirectAddress,
                Displacement = address,
            };
        }
        else { throw new($"Unrecognized beginning of an instruction: {Convert.ToString(byte1, 2).PadLeft(8, '0')}"); }
    }

    private InterpretedMovMode InterpretMovMode(MovMode mod, byte rm) => (mod, rm) switch
    {
        (MovMode.Register, _) => new(InterpretedMovModeType.Register, 0),
        (MovMode.MemoryNoDisplacement, 0b110) => new(InterpretedMovModeType.DirectAddress, ParseSignedWord()),
        (MovMode.MemoryNoDisplacement, _) => new(InterpretedMovModeType.AddressWithNoDisplacement, 0),
        (MovMode.Memory8BitDisplacement, _) => new(InterpretedMovModeType.AddressWithDisplacement, ParseSignedByte()),
        (MovMode.Memory16BitDisplacement, _) => new(InterpretedMovModeType.AddressWithDisplacement, ParseSignedWord()),
        _ => throw new(),
    };

    private short ParseSignedByte() => (sbyte)Next();

    private short ParseSignedWord()
    {
        var low = Next();
        var high = Next();
        return (short)((high << 8) | low);
    }

    private static bool GetBit(int index, byte value) => GetBits(index, index + 1, value) == 1;

    private static byte GetBits(int startIndex, int endIndex, byte value)
    {
        var mask = 0b11111111 >> (8 - endIndex);
        return (byte)((value & mask) >> startIndex);
    }

}
