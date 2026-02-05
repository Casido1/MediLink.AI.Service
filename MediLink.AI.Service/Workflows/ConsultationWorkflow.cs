using MediLink.AI.Service.Models;
using Microsoft.Agents.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using System.Text.Json;

namespace MediLink.AI.Service.Workflows
{
    public class ConsultationWorkflow(Kernel kernel, [FromKeyedServices("Diagnostician")] ChatClientAgent diagnostician, [FromKeyedServices("Pharmacist")] ChatClientAgent pharmacist)
    {
        public async Task<MedicalAdvice> RunConsultationAsync(string patientNotes, string existingMeds)
        {
            //Diagnostician's evaluation
            var diagnosisResponse = await diagnostician.RunAsync(
                 $"Patient Notes: {patientNotes}\nExisting Meds: {existingMeds}\nPlease provide a diagnosis and treatment plan."
                );

            //Pahrmacist's review
            var pharmacistResponse = await pharmacist.RunAsync(
                 $"Diagnosis and Treatment Plan: {diagnosisResponse}\nExisting Meds: {existingMeds}\nPlease review for safety and drug interactions."
                );

            //Final decision
            var settings = new GeminiPromptExecutionSettings
            {
                ResponseMimeType = "application/json",
                ResponseSchema = typeof(MedicalAdvice)
            };
            var synthesisResult = await kernel.InvokePromptAsync<string>(
                $@"Synthesize the diagnosis and safety review into a final report.
                Diagnosis: {diagnosisResponse}
                Safety Review: {pharmacistResponse}",
                new KernelArguments(settings));

            return JsonSerializer.Deserialize<MedicalAdvice>(synthesisResult!)
                ?? new MedicalAdvice("Error", "Error", [], [], "Error");
        }
    }
}
