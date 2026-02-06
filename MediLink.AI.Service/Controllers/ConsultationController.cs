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
        public async Task<IActionResult> IngestAsync(IFormFile file)
        {
            if (file.Length == 0) return BadRequest("File is empty");

            //save temporarily
            var tempPath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(tempPath))
            {
                await file.CopyToAsync(stream);
            }

            await ingestionService.IngestManualAsync(tempPath, "Pharmacy");

            return Ok($"Successfully ingested {file.FileName} in the vector database.");
        }

        [HttpPost("start")]
        public async Task<ActionResult<MedicalAdvice>> StartConsultationAsync([FromBody] ConsultationRequest request)
        {
            var response = await consultationWorkflow.RunConsultationAsync(request.PatientNotes, request.ExistingMeds);

            return Ok(response);
        }
    }
}
