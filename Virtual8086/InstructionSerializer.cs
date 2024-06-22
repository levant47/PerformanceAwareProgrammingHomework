global using static InstructionSerializer;

public static class InstructionSerializer
{
    public static string SerializeInstructions(List<Instruction> instructions)
    {
        var result = new StringBuilder();
        var labels = instructions.Where(instruction => instruction.Type == InstructionType.JMP)
            .ToDictionaryIgnoringDuplicates(instruction => instruction.JumpAddress, instruction => $"label{instruction.JumpAddress}");
        foreach (var instruction in instructions)
        {
            if (labels.TryGetValue(instruction.InstructionAddress, out var labelName))
            {
                result.Append(labelName);
                result.AppendLine(":");
            }
            result.AppendLine(instruction.Type switch
            {
                InstructionType.JMP => $"{instruction.JumpType} {labels[instruction.JumpAddress]}",
                _ => instruction.Operands switch
                {
                    InstructionOperands.RegisterToRegister => $"{instruction.Type} {instruction.DestinationRegister.GetName()}, {instruction.SourceRegister.GetName()}",
                    InstructionOperands.ImmediateToRegister => $"{instruction.Type} {instruction.DestinationRegister.GetName()}, {instruction.Immediate}",
                    InstructionOperands.MemoryToRegister => $"{instruction.Type} {instruction.DestinationRegister.GetName()}, [{EffectiveAddressCalculationToString(instruction.Address)}]",
                    InstructionOperands.RegisterToMemory => $"{instruction.Type} [{EffectiveAddressCalculationToString(instruction.Address)}], {instruction.SourceRegister.GetName()}",
                    InstructionOperands.MemoryToRegisterWithDisplacement => $"{instruction.Type} {instruction.DestinationRegister.GetName()}, [{EffectiveAddressCalculationToString(instruction.Address)} + {instruction.Displacement}]",
                    InstructionOperands.RegisterToMemoryWithDisplacement => $"{instruction.Type} [{EffectiveAddressCalculationToString(instruction.Address)} + {instruction.Displacement}], {instruction.SourceRegister.GetName()}",
                    InstructionOperands.Immediate8ToMemory => $"{instruction.Type} [{EffectiveAddressCalculationToString(instruction.Address)}], byte {instruction.Immediate}",
                    InstructionOperands.Immediate8ToMemoryWithDisplacement => $"{instruction.Type} [{EffectiveAddressCalculationToString(instruction.Address)} + {instruction.Displacement}], byte {instruction.Immediate}",
                    InstructionOperands.Immediate16ToMemory => $"{instruction.Type} [{EffectiveAddressCalculationToString(instruction.Address)}], word {instruction.Immediate}",
                    InstructionOperands.Immediate16ToMemoryWithDisplacement => $"{instruction.Type} [{EffectiveAddressCalculationToString(instruction.Address)} + {instruction.Displacement}], word {instruction.Immediate}",
                    InstructionOperands.Immediate8ToDirectAddress => $"{instruction.Type} [{instruction.Displacement}], byte {instruction.Immediate}",
                    InstructionOperands.Immediate16ToDirectAddress => $"{instruction.Type} [{instruction.Displacement}], word {instruction.Immediate}",
                    InstructionOperands.RegisterToDirectAddress => $"{instruction.Type} [{instruction.Displacement}], {instruction.SourceRegister.GetName()}",
                    InstructionOperands.DirectAddressToRegister => $"{instruction.Type} {instruction.DestinationRegister.GetName()}, [{instruction.Displacement}]",
                    InstructionOperands.DirectAddressToAccumulator8 => $"{instruction.Type} AL, [{instruction.Displacement}]",
                    InstructionOperands.DirectAddressToAccumulator16 => $"{instruction.Type} AX, [{instruction.Displacement}]",
                    InstructionOperands.Accumulator8ToDirectAddress => $"{instruction.Type} [{instruction.Displacement}], AL",
                    InstructionOperands.Accumulator16ToDirectAddress => $"{instruction.Type} [{instruction.Displacement}], AX",
                    _ => throw new(),
                }
            });
        }
        return result.ToString();
    }

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
