using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace MediLink.AI.Service.Agents
{
    public static class MedicalAgents
    {
        public const string DiagnosticianName = "Diagnostician";
        public const string PharmacistName = "Pharmacist";

        public static ChatClientAgent CreateDiagnostician(Kernel kernel)
        {
            return new ChatClientAgent(
                chatClient: kernel.GetRequiredService<IChatClient>(),
                name: DiagnosticianName,
                instructions: "You are a professional medical diagnostician. Analyze patient notes, search medical knowledge for context, and provide a potential diagnosis and rationale. If you need more information, ask.")
            {
            };
        }

        public static ChatClientAgent CreatePharmacist(Kernel kernel)
        {
            return new ChatClientAgent(
                chatClient: kernel.GetRequiredService<IChatClient>(),
                name: PharmacistName,
                instructions: "You are a safety-focused clinical pharmacist. Review the diagnosis and suggested treatments. Cross-reference them against the patient's existing medications for safety. Provide a final safety review.")
            {
            };
        }
    }
}
