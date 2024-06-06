public enum InstructionType
{
    Mov = 0b100010,
}

public enum Register8
{
    AL,
    CL,
    DL,
    BL,
    AH,
    CH,
    DH,
    BH,
}

public enum Register16
{
    AX,
    CX,
    DX,
    BX,
    SP,
    BP,
    SI,
    DI,
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
