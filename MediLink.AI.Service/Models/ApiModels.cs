namespace MediLink.AI.Service.Models
{
    public record ConsultationRequest(string PatientNotes, string ExistingMeds);

    public record IngestionRequest(string Content, string Category);
}
