public enum InstructionOpcode : byte
{
    MoveRegisterMemoryToFromMemory = 0b100010,
    MoveImmediateToRegister = 0b1011,
    MoveImmediateToRegisterOrMemory = 0b1100011,
    MoveMemoryToAccumulator = 0b1010000,
    MoveAccumulatorToMemory = 0b1010001,
}

public enum InstructionType
{
    MOV,
    ADD,
}

public enum InstructionOperands : byte
{
    MoveRegisterToRegister,
    MoveImmediateToRegister,
    MoveMemoryToRegister,
    MoveRegisterToMemory,
    MoveMemoryToRegisterWithDisplacement,
    MoveRegisterToMemoryWithDisplacement,
    MoveImmediate8ToMemory,
    MoveImmediate8ToMemoryWithDisplacement,
    MoveImmediate16ToMemory,
    MoveImmediate16ToMemoryWithDisplacement,
    MoveImmediate8ToDirectAddress,
    MoveImmediate16ToDirectAddress,
    MoveRegisterToDirectAddress,
    MoveDirectAddressToRegister,
    MoveDirectAddressToAccumulator8,
    MoveDirectAddressToAccumulator16,
    MoveAccumulator8ToDirectAddress,
    MoveAccumulator16ToDirectAddress,
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
        InstructionOperands.MoveRegisterToRegister => $"{Type} {DestinationRegister.GetName()}, {SourceRegister.GetName()}",
        InstructionOperands.MoveImmediateToRegister => $"{Type} {DestinationRegister.GetName()}, {Immediate}",
        InstructionOperands.MoveMemoryToRegister => $"{Type} {DestinationRegister.GetName()}, [{EffectiveAddressCalculationToString(Address)}]",
        InstructionOperands.MoveRegisterToMemory => $"{Type} [{EffectiveAddressCalculationToString(Address)}], {SourceRegister.GetName()}",
        InstructionOperands.MoveMemoryToRegisterWithDisplacement => $"{Type} {DestinationRegister.GetName()}, [{EffectiveAddressCalculationToString(Address)} + {Displacement}]",
        InstructionOperands.MoveRegisterToMemoryWithDisplacement => $"{Type} [{EffectiveAddressCalculationToString(Address)} + {Displacement}], {SourceRegister.GetName()}",
        InstructionOperands.MoveImmediate8ToMemory => $"{Type} [{EffectiveAddressCalculationToString(Address)}], byte {Immediate}",
        InstructionOperands.MoveImmediate8ToMemoryWithDisplacement => $"{Type} [{EffectiveAddressCalculationToString(Address)} + {Displacement}], byte {Immediate}",
        InstructionOperands.MoveImmediate16ToMemory => $"{Type} [{EffectiveAddressCalculationToString(Address)}], word {Immediate}",
        InstructionOperands.MoveImmediate16ToMemoryWithDisplacement => $"{Type} [{EffectiveAddressCalculationToString(Address)} + {Displacement}], word {Immediate}",
        InstructionOperands.MoveImmediate8ToDirectAddress => $"{Type} [{Displacement}], byte {Immediate}",
        InstructionOperands.MoveImmediate16ToDirectAddress => $"{Type} [{Displacement}], word {Immediate}",
        InstructionOperands.MoveRegisterToDirectAddress => $"{Type} [{Displacement}], {SourceRegister.GetName()}",
        InstructionOperands.MoveDirectAddressToRegister => $"{Type} {DestinationRegister.GetName()}, [{Displacement}]",
        InstructionOperands.MoveDirectAddressToAccumulator8 => $"{Type} AL, [{Displacement}]",
        InstructionOperands.MoveDirectAddressToAccumulator16 => $"{Type} AX, [{Displacement}]",
        InstructionOperands.MoveAccumulator8ToDirectAddress => $"{Type} [{Displacement}], AL",
        InstructionOperands.MoveAccumulator16ToDirectAddress => $"{Type} [{Displacement}], AX",
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
