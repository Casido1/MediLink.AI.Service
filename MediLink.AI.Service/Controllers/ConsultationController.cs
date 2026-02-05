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
        [HttpPost("ingest")]
        public async Task<IActionResult> IngestAsync([FromBody] IngestionRequest request)
        {
            await ingestionService.IngestManualAsync(request.Content, request.Category);

            return Ok("Ingestion complete");
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartConsultationAsync([FromBody] ConsultationRequest request)
        {
            var response = await consultationWorkflow.RunConsultationAsync(request.PatientNotes, request.ExistingMeds);

            return Ok(response);
        }
    }
}
