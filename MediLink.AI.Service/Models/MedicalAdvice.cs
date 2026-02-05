namespace MediLink.AI.Service.Models
{
    public record MedicalAdvice(
    string Diagnosis,
    string Rationale,
    List<string> RecommendedActions,
    List<string> SafetyWarnings,
    string PharmacistReview
);
}
