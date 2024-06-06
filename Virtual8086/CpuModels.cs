public enum InstructionType : byte
{
    Mov = 0b100010,
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
