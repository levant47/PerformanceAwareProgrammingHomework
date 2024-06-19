public enum InstructionOpcode : byte
{
    MoveRegisterOrMemoryToOrFromMemory = 0b100010,
    MoveImmediateToRegister = 0b1011,
    MoveImmediateToRegisterOrMemory = 0b1100011,
    MoveMemoryToAccumulator = 0b1010000,
    MoveAccumulatorToMemory = 0b1010001,
    AddRegisterOrMemoryWithRegisterToEither = 0b000000,
    AddOrSubtractImmediateToRegisterOrMemory = 0b100000,
    AddImmediateToAccumulator = 0b0000010,
    SubtractRegisterOrMemoryAndRegisterToEither = 0b001010,
    SubtractImmediateFromAccumulator = 0b0010110,
}

public enum InstructionType
{
    MOV,
    ADD,
    SUB,
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
    public InstructionOperands Operands;
    public Register SourceRegister;
    public Register DestinationRegister;
    public EffectiveAddressCalculation Address;
    public short Immediate;
    public short Displacement;

    public override string ToString() => Operands switch
    {
        InstructionOperands.RegisterToRegister => $"{Type} {DestinationRegister.GetName()}, {SourceRegister.GetName()}",
        InstructionOperands.ImmediateToRegister => $"{Type} {DestinationRegister.GetName()}, {Immediate}",
        InstructionOperands.MemoryToRegister => $"{Type} {DestinationRegister.GetName()}, [{EffectiveAddressCalculationToString(Address)}]",
        InstructionOperands.RegisterToMemory => $"{Type} [{EffectiveAddressCalculationToString(Address)}], {SourceRegister.GetName()}",
        InstructionOperands.MemoryToRegisterWithDisplacement => $"{Type} {DestinationRegister.GetName()}, [{EffectiveAddressCalculationToString(Address)} + {Displacement}]",
        InstructionOperands.RegisterToMemoryWithDisplacement => $"{Type} [{EffectiveAddressCalculationToString(Address)} + {Displacement}], {SourceRegister.GetName()}",
        InstructionOperands.Immediate8ToMemory => $"{Type} [{EffectiveAddressCalculationToString(Address)}], byte {Immediate}",
        InstructionOperands.Immediate8ToMemoryWithDisplacement => $"{Type} [{EffectiveAddressCalculationToString(Address)} + {Displacement}], byte {Immediate}",
        InstructionOperands.Immediate16ToMemory => $"{Type} [{EffectiveAddressCalculationToString(Address)}], word {Immediate}",
        InstructionOperands.Immediate16ToMemoryWithDisplacement => $"{Type} [{EffectiveAddressCalculationToString(Address)} + {Displacement}], word {Immediate}",
        InstructionOperands.Immediate8ToDirectAddress => $"{Type} [{Displacement}], byte {Immediate}",
        InstructionOperands.Immediate16ToDirectAddress => $"{Type} [{Displacement}], word {Immediate}",
        InstructionOperands.RegisterToDirectAddress => $"{Type} [{Displacement}], {SourceRegister.GetName()}",
        InstructionOperands.DirectAddressToRegister => $"{Type} {DestinationRegister.GetName()}, [{Displacement}]",
        InstructionOperands.DirectAddressToAccumulator8 => $"{Type} AL, [{Displacement}]",
        InstructionOperands.DirectAddressToAccumulator16 => $"{Type} AX, [{Displacement}]",
        InstructionOperands.Accumulator8ToDirectAddress => $"{Type} [{Displacement}], AL",
        InstructionOperands.Accumulator16ToDirectAddress => $"{Type} [{Displacement}], AX",
        _ => throw new ArgumentOutOfRangeException()
    };

    private static string EffectiveAddressCalculationToString(EffectiveAddressCalculation address) => address switch
    {
        EffectiveAddressCalculation.BxSi => "BX + SI",
        EffectiveAddressCalculation.BxDi => "BX + DI",
        EffectiveAddressCalculation.BpSi => "BP + SI",
        EffectiveAddressCalculation.BpDi => "BP + DI",
        EffectiveAddressCalculation.Si => "SI",
        EffectiveAddressCalculation.Di => "DI",
        EffectiveAddressCalculation.Bp => "BP",
        EffectiveAddressCalculation.Bx => "BX",
        _ => throw new(),
    };
}
