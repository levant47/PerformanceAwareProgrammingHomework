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
        var result = new Instruction();

        var byte1 = Next();
        if (GetBits(2, 8, byte1) == (byte)InstructionOpcode.MoveRegisterOrMemoryToOrFromMemory)
        {
            result.Type = InstructionType.MOV;
            ParseRegisterOrMemoryToOrFromMemory(byte1, ref result);
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
            result.Type = InstructionType.MOV;
            ParseImmediateToRegisterOrMemory(isWide: GetBit(0, byte1), byte2: Next(), ref result);
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
        else if (GetBits(2, 8, byte1) == (byte)InstructionOpcode.AddRegisterOrMemoryWithRegisterToEither)
        {
            result.Type = InstructionType.ADD;
            ParseRegisterOrMemoryToOrFromMemory(byte1, ref result);
        }
        else if (GetBits(2, 8, byte1) == (byte)InstructionOpcode.AddOrSubtractImmediateToRegisterOrMemory)
        {
            var byte2 = Next();
            var secondOpcode = GetBits(2, 5, byte2);
            result.Type = secondOpcode == 101 ? InstructionType.SUB : InstructionType.ADD;
            ParseImmediateToRegisterOrMemory(isWide: GetBit(0, byte1) && !GetBit(1, byte1), byte2, ref result);
        }
        else if (GetBits(1, 8, byte1) == (byte)InstructionOpcode.AddImmediateToAccumulator)
        {
            result.Type = InstructionType.ADD;
            var isWide = GetBit(0, byte1);
            result.Operands = InstructionOperands.ImmediateToRegister;
            result.Immediate = isWide ? ParseSignedWord() : ParseSignedByte();
            result.DestinationRegister = new(isWide, isWide ? (byte)Register8.AL : (byte)Register16.AX);
        }
        else if (GetBits(2, 8, byte1) == (byte)InstructionOpcode.SubtractRegisterOrMemoryAndRegisterToEither)
        {
            result.Type = InstructionType.SUB;
            ParseRegisterOrMemoryToOrFromMemory(byte1, ref result);
        }
        else if (GetBits(1, 8, byte1) == (byte)InstructionOpcode.SubtractImmediateFromAccumulator)
        {
            result.Type = InstructionType.SUB;
            var isWide = GetBit(0, byte1);
            result.Operands = InstructionOperands.ImmediateToRegister;
            result.Immediate = isWide ? ParseSignedWord() : ParseSignedByte();
            result.DestinationRegister = new(isWide, isWide ? (byte)Register8.AL : (byte)Register16.AX);
        }
        else { throw new($"Unrecognized beginning of an instruction: {Convert.ToString(byte1, 2).PadLeft(8, '0')}"); }

        return result;
    }

    private void ParseRegisterOrMemoryToOrFromMemory(byte byte1, ref Instruction result)
    {
        var isDestinationInRegField = GetBit(1, byte1);
        var isWide = GetBit(0, byte1);

        var byte2 = Next();
        var mod = ToEnum<MovMode>(GetBits(6, 8, byte2));
        var reg = GetBits(3, 6, byte2);
        var rm = GetBits(0, 3, byte2);

        var interpretedMovMode = InterpretMovMode(mod, rm);

        switch (isDestinationInRegField, interpretedMovMode)
        {
            case (false, { Type: InterpretedMovModeType.Register }):
            {
                result.Operands = InstructionOperands.RegisterToRegister;
                result.DestinationRegister = new(isWide, rm);
                result.SourceRegister = new(isWide, reg);
                break;
            }
            case (true, { Type: InterpretedMovModeType.Register }):
            {
                result.Operands = InstructionOperands.RegisterToRegister;
                result.DestinationRegister = new(isWide, reg);
                result.SourceRegister = new(isWide, rm);
                break;
            }
            case (true, { Type: InterpretedMovModeType.AddressWithNoDisplacement }):
            {
                result.Operands = InstructionOperands.MemoryToRegister;
                result.DestinationRegister = new(isWide, reg);
                result.Address = ToEnum<EffectiveAddressCalculation>(rm);
                break;
            }
            case (false, { Type: InterpretedMovModeType.AddressWithNoDisplacement }):
            {
                result.Operands = InstructionOperands.RegisterToMemory;
                result.Address = ToEnum<EffectiveAddressCalculation>(rm);
                result.SourceRegister= new(isWide, reg);
                break;
            }
            case (true, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }):
            {
                result.Operands = InstructionOperands.MemoryToRegisterWithDisplacement;
                result.DestinationRegister = new(isWide, reg);
                result.Address = ToEnum<EffectiveAddressCalculation>(rm);
                result.Displacement = displacement;
                break;
            }
            case (false, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }):
            {
                result.Operands = InstructionOperands.RegisterToMemoryWithDisplacement;
                result.Address = ToEnum<EffectiveAddressCalculation>(rm);
                result.SourceRegister = new(isWide, reg);
                result.Displacement = displacement;
                break;
            }
            case (true, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }):
            {
                result.Operands = InstructionOperands.DirectAddressToRegister;
                result.DestinationRegister= new(isWide, reg);
                result.Displacement = displacement;
                break;
            }
            case (false, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }):
            {
                result.Operands = InstructionOperands.RegisterToDirectAddress;
                result.SourceRegister = new(isWide, reg);
                result.Displacement = displacement;
                break;
            }
            default: throw new();
        };
    }

    private void ParseImmediateToRegisterOrMemory(bool isWide, byte byte2, ref Instruction result)
    {
        var mod = ToEnum<MovMode>(GetBits(6, 8, byte2));
        var rm = GetBits(0, 3, byte2);

        var interpretedMovMode = InterpretMovMode(mod, rm);
        var data = isWide ? ParseSignedWord() : ParseSignedByte();

        switch (isWide, interpretedMovMode)
        {
            case (false, { Type: InterpretedMovModeType.Register }):
            {
                result.Operands = InstructionOperands.ImmediateToRegister;
                result.Immediate = data;
                result.DestinationRegister = new(isWide, rm);
                break;
            }
            case (false, { Type: InterpretedMovModeType.AddressWithNoDisplacement }):
            {
                result.Operands = InstructionOperands.Immediate8ToMemory;
                result.Immediate = data;
                result.Address = ToEnum<EffectiveAddressCalculation>(rm);
                break;
            }
            case (false, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }):
            {
                result.Operands = InstructionOperands.Immediate8ToMemoryWithDisplacement;
                result.Immediate = data;
                result.Address = ToEnum<EffectiveAddressCalculation>(rm);
                result.Displacement = displacement;
                break;
            }
            case (false, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }):
            {
                result.Operands = InstructionOperands.Immediate8ToDirectAddress;
                result.Immediate = data;
                result.Displacement = displacement;
                break;
            }
            case (true, { Type: InterpretedMovModeType.Register }):
            {
                result.Operands = InstructionOperands.ImmediateToRegister;
                result.Immediate = data;
                result.DestinationRegister = new(isWide, rm);
                break;
            }
            case (true, { Type: InterpretedMovModeType.AddressWithNoDisplacement }):
            {
                result.Operands = InstructionOperands.Immediate16ToMemory;
                result.Immediate = data;
                result.Address = ToEnum<EffectiveAddressCalculation>(rm);
                break;
            }
            case (true, { Type: InterpretedMovModeType.AddressWithDisplacement, Displacement: var displacement }):
            {
                result.Operands = InstructionOperands.Immediate16ToMemoryWithDisplacement;
                result.Immediate = data;
                result.Address = ToEnum<EffectiveAddressCalculation>(rm);
                result.Displacement = displacement;
                break;
            }
            case (true, { Type: InterpretedMovModeType.DirectAddress, Displacement: var displacement }):
            {
                result.Operands = InstructionOperands.Immediate16ToDirectAddress;
                result.Immediate = data;
                result.Displacement = displacement;
                break;
            }
            default: throw new();
        };
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
