public enum InstructionOpcode : byte
{
    MoveRegisterOrMemoryToOrFromMemory = 0b100010,
    MoveImmediateToRegister = 0b1011,
    MoveImmediateToRegisterOrMemory = 0b1100011,
    MoveMemoryToAccumulator = 0b1010000,
    MoveAccumulatorToMemory = 0b1010001,
    AddRegisterOrMemoryWithRegisterToEither = 0b000000,
    AddOrSubtractOrCompareImmediateToRegisterOrMemory = 0b100000,
    AddImmediateToAccumulator = 0b0000010,
    SubtractRegisterOrMemoryAndRegisterToEither = 0b001010,
    SubtractImmediateFromAccumulator = 0b0010110,
    CompareRegisterOrMemoryWithRegister = 0b001110,
    CompareImmediateWithAccumulator = 0b0011110,
    JumpOnNotEqual = 0b01110101,
    JumpOnEqual = 0b01110100,
    JumpOnLess = 0b01111100,
    JumpOnLessOrEqual = 0b01111110,
    JumpOnBelow = 0b01110010,
    JumpOnBelowOrEqual = 0b01110110,
    JumpOnParity = 0b01111010,
    JumpOnOverflow = 0b01110000,
    JumpOnSign = 0b01111000,
    JumpOnGreaterOrEqual = 0b01111101,
    JumpOnGreater = 0b01111111,
    JumpOnNotBelow = 0b01110011,
    JumpOnAbove = 0b01110111,
    JumpOnNotParity = 0b01111011,
    JumpOnNotOverflow = 0b01110001,
    JumpOnNotSign = 0b01111001,
    JumpOnCxZero = 0b11100011,
    Loop = 0b11100010,
    LoopWhileZero = 0b11100001,
    LoopWhileNotZero = 0b11100000,
}

public enum InstructionType
{
    MOV,
    ADD,
    SUB,
    CMP,
    JMP,
}

public enum JumpType
{
    JNE,
    JE,
    JL,
    JLE,
    JB,
    JBE,
    JP,
    JO,
    JS,
    JGE,
    JG,
    JNB,
    JA,
    JNP,
    JNO,
    JNS,
    JCXZ,
    LOOP,
    LOOPZ,
    LOOPNZ,
}

public static class CpuModelsGlobals
{
    public static readonly Dictionary<InstructionOpcode, JumpType> JumpInstructionOpcodesToJumpTypeMap = new()
    {
        { InstructionOpcode.JumpOnNotEqual, JumpType.JNE },
        { InstructionOpcode.JumpOnEqual, JumpType.JE },
        { InstructionOpcode.JumpOnLess, JumpType.JL },
        { InstructionOpcode.JumpOnLessOrEqual, JumpType.JLE },
        { InstructionOpcode.JumpOnBelow, JumpType.JB },
        { InstructionOpcode.JumpOnBelowOrEqual, JumpType.JBE },
        { InstructionOpcode.JumpOnParity, JumpType.JP },
        { InstructionOpcode.JumpOnOverflow, JumpType.JO },
        { InstructionOpcode.JumpOnSign, JumpType.JS },
        { InstructionOpcode.JumpOnGreaterOrEqual, JumpType.JGE },
        { InstructionOpcode.JumpOnGreater, JumpType.JG },
        { InstructionOpcode.JumpOnNotBelow, JumpType.JNB },
        { InstructionOpcode.JumpOnAbove, JumpType.JA },
        { InstructionOpcode.JumpOnNotParity, JumpType.JNP },
        { InstructionOpcode.JumpOnNotOverflow, JumpType.JNO },
        { InstructionOpcode.JumpOnNotSign, JumpType.JNS },
        { InstructionOpcode.JumpOnCxZero, JumpType.JCXZ },
        { InstructionOpcode.Loop, JumpType.LOOP },
        { InstructionOpcode.LoopWhileZero, JumpType.LOOPZ },
        { InstructionOpcode.LoopWhileNotZero, JumpType.LOOPNZ },
    };
}

public enum InstructionOperands : byte
{
    RegisterToRegister,
    ImmediateToRegister,
    MemoryToRegister,
    RegisterToMemory,
    MemoryToRegisterWithDisplacement,
    RegisterToMemoryWithDisplacement,
    Immediate8ToMemory,
    Immediate8ToMemoryWithDisplacement,
    Immediate16ToMemory,
    Immediate16ToMemoryWithDisplacement,
    Immediate8ToDirectAddress,
    Immediate16ToDirectAddress,
    RegisterToDirectAddress,
    DirectAddressToRegister,
    DirectAddressToAccumulator8,
    DirectAddressToAccumulator16,
    Accumulator8ToDirectAddress,
    Accumulator16ToDirectAddress,
}

public struct Register
{
    public bool IsWide;
    public Register8 Register8;
    public Register16 Register16;

    public Register(bool isWide, byte register)
    {
        IsWide = isWide;
        if (IsWide) { Register16 = ToEnum<Register16>(register); }
        else { Register8 = ToEnum<Register8>(register); }
    }

    [Pure]
    public string GetName() => IsWide ? Register16.ToString() : Register8.ToString();
}

public enum Register8 : byte
{
    AL = 0b000,
    CL = 0b001,
    DL = 0b010,
    BL = 0b011,
    AH = 0b100,
    CH = 0b101,
    DH = 0b110,
    BH = 0b111,
}

public enum Register16 : byte
{
    AX = 0b000,
    CX = 0b001,
    DX = 0b010,
    BX = 0b011,
    SP = 0b100,
    BP = 0b101,
    SI = 0b110,
    DI = 0b111,
}

public enum EffectiveAddressCalculation
{
    BxSi = 0b000,
    BxDi = 0b001,
    BpSi = 0b010,
    BpDi = 0b011,
    Si = 0b100,
    Di = 0b101,
    Bp = 0b110,
    Bx = 0b111,
}

public struct Instruction
{
    public InstructionType Type;
    public int InstructionAddress;
    public JumpType JumpType;
    public int JumpAddress;
    public InstructionOperands Operands;
    public Register SourceRegister;
    public Register DestinationRegister;
    public EffectiveAddressCalculation Address;
    public short Immediate;
    public short Displacement;
}
