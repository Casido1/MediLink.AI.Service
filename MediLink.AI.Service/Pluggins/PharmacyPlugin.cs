using MediLink.AI.Service.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace MediLink.AI.Service.Pluggins
{
    public class PharmacyPlugin(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        [KernelFunction, Description("Check if a newly diagnosed condition or suggested medication conflicts with the patient's existing medications using the openFDA drug label API.")]
        public async Task<string> CheckDrugInteractions(
        [Description("The patient's existing medications (comma or semicolon separated)")] string existingMeds,
        [Description("The suggested new treatments or medications (comma or semicolon separated)")] string newTreatments)
        {
            var existingList = SplitMeds(existingMeds);
            var newList = SplitMeds(newTreatments);
            var allMeds = existingList.Concat(newList).Distinct().ToList();

            if (allMeds.Count < 2)
            {
                return "Insufficient medication data found to perform an interaction check. Please ensure drug names are spelled correctly.";
            }

            var results = new List<string>();
            foreach (var med in newList)
            {
                var labelData = await GetDrugLabelAsync(med);
                if (labelData == null) continue;

                foreach (var existingMed in existingList)
                {
                    if (CheckLabelForInteraction(labelData, existingMed))
                    {
                        results.Add($"Potential interaction found between **{med}** and **{existingMed}** in the FDA labeling information.");
                    }
                }
            }

            if (results.Count == 0)
            {
                return "No explicit drug interaction warnings were found in the openFDA labels for the provided medications. However, always consult a healthcare professional.";
            }

            return "Based on openFDA label data, the following notes were found:\n\n" + string.Join("\n", results);
        }

        private List<string> SplitMeds(string meds) =>
            meds.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(m => m.Trim())
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .ToList();

        private async Task<OpenFdaLabelResult?> GetDrugLabelAsync(string medName)
        {
            try
            {
                // Search by brand_name OR generic_name
                var requestUri = QueryHelpers.AddQueryString("", new Dictionary<string, string>
                {
                    ["search"] = $"openfda.brand_name:\"{medName}\"+openfda.generic_name:\"{medName}\"",
                    ["limit"] = "1"
                });

                var response = await _httpClient.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<OpenFdaLabelResponse>(content);

                return data?.Results?.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private bool CheckLabelForInteraction(OpenFdaLabelResult label, string targetMed)
        {
            if (label.DrugInteractions == null) return false;

            foreach (var interactionText in label.DrugInteractions)
            {
                if (interactionText.Contains(targetMed, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
