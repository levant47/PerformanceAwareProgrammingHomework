public enum InstructionOpcode : byte
{
    RegisterMemoryToFromMemory = 0b100010,
    ImmediateToRegister = 0b1011,
}

public enum InstructionType : byte
{
    MoveRegisterToRegister8,
    MoveRegisterToRegister16,
    MoveImmediateToRegister8,
    MoveImmediateToRegister16,
    MoveMemoryToRegister8,
    MoveMemoryToRegister16,
    MoveRegister8ToMemory,
    MoveRegister16ToMemory,
    MoveMemoryToRegister8WithDisplacement,
    MoveMemoryToRegister16WithDisplacement,
    MoveRegister8ToMemoryWithDisplacement,
    MoveRegister16ToMemoryWithDisplacement,
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
    public Register8 SourceRegister8;
    public Register8 DestinationRegister8;
    public Register16 SourceRegister16;
    public Register16 DestinationRegister16;
    public EffectiveAddressCalculation Address;
    public ushort Immediate;
    public ushort Displacement;

    public override string ToString() => Type switch
    {
        InstructionType.MoveRegisterToRegister8 => $"mov {DestinationRegister8}, {SourceRegister8}",
        InstructionType.MoveRegisterToRegister16 => $"mov {DestinationRegister16}, {SourceRegister16}",
        InstructionType.MoveImmediateToRegister8 => $"mov {DestinationRegister8}, {Immediate}",
        InstructionType.MoveImmediateToRegister16 => $"mov {DestinationRegister16}, {Immediate}",
        InstructionType.MoveMemoryToRegister8 => $"mov {DestinationRegister8}, [{EffectiveAddressCalculationToString(Address)}]",
        InstructionType.MoveMemoryToRegister16 => $"mov {DestinationRegister16}, [{EffectiveAddressCalculationToString(Address)}]",
        InstructionType.MoveRegister8ToMemory => $"mov [{EffectiveAddressCalculationToString(Address)}], {SourceRegister8}",
        InstructionType.MoveRegister16ToMemory => $"mov [{EffectiveAddressCalculationToString(Address)}], {SourceRegister16}",
        InstructionType.MoveMemoryToRegister8WithDisplacement => $"mov {DestinationRegister8}, [{EffectiveAddressCalculationToString(Address)} + {Displacement}]",
        InstructionType.MoveMemoryToRegister16WithDisplacement => $"mov {DestinationRegister16}, [{EffectiveAddressCalculationToString(Address)} + {Displacement}]",
        InstructionType.MoveRegister8ToMemoryWithDisplacement => $"mov [{EffectiveAddressCalculationToString(Address)} + {Displacement}], {SourceRegister8}",
        InstructionType.MoveRegister16ToMemoryWithDisplacement => $"mov [{EffectiveAddressCalculationToString(Address)} + {Displacement}], {SourceRegister16}",
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
