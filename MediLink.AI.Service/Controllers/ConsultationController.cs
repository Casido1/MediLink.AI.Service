using MediLink.AI.Service.Models;
using MediLink.AI.Service.Services;
using MediLink.AI.Service.Workflows;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MediLink.AI.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationController(MedicalIngestionService ingestionService, ConsultationWorkflow consultationWorkflow) : ControllerBase
    {
        [HttpPost("upload-manual")]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> IngestAsync(IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are supported.");

            try
            {
                using var stream = file.OpenReadStream();

                await ingestionService.IngestManualAsync(stream, file.FileName, ct);

                return Ok(new
                {
                    Message = "Ingestion completed successfully",
                    Source = file.FileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("start")]
        public async Task<ActionResult<MedicalAdvice>> StartConsultationAsync([FromBody] ConsultationRequest request)
        {
            var response = await consultationWorkflow.RunConsultationAsync(request.PatientNotes, request.ExistingMeds);

            return Ok(response);
        }
    }
}