using System.Text.Json.Serialization;

namespace MediLink.AI.Service.Models
{
    public record OpenFdaLabelResponse(
    [property: JsonPropertyName("results")] OpenFdaLabelResult[]? Results
);

    public record OpenFdaLabelResult(
        [property: JsonPropertyName("drug_interactions")] string[]? DrugInteractions,
        [property: JsonPropertyName("openfda")] OpenFdaMetadata? OpenFda
    );

    public record OpenFdaMetadata(
        [property: JsonPropertyName("brand_name")] string[]? BrandName,
        [property: JsonPropertyName("generic_name")] string[]? GenericName
    );

}
