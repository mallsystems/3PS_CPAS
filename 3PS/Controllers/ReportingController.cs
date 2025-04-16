using _3PS.Contexts;
using _3PS.Models.Reporting;
using Microsoft.AspNetCore.Mvc;

namespace _3PS.Controllers
{
    /// <summary>
    /// Handles operations related to reporting.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReportingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportingController> _logger;

        public ReportingController(ApplicationDbContext context, ILogger<ReportingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Submits a new vaccine report to be saved.
        /// </summary>
        /// <remarks>
        /// This endpoint receives vaccine report details and persists them.
        /// Note: This saves data using the VaccineReporting model, which is configured
        /// in the DbContext to map to the "DBA - VaccineBatchSendReport" table
        /// using the string 'RECNUM' property as the key.
        /// </remarks>
        /// <param name="report">The vaccine report data based on the VaccineReporting model.</param>
        /// <returns>A status code indicating the result of the operation.</returns>
        /// <response code="200">Report successfully created. Returns the RECNUM.</response>
        /// <response code="400">Bad Request. The report data was null or invalid.</response>
        /// <response code="500">Internal Server Error. An error occurred processing the request.</response>
        [HttpPost("Vaccine")] // Route is now POST /api/Reporting/Vaccine
        [ProducesResponseType(typeof((string, string)), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostVaccineReport(
            [FromBody] VaccineReporting report)
        {
            if (report == null)
            {
                _logger.LogWarning("Report not provided in request body.");
                return BadRequest("Report not provided in request body.");
            }

            try
            {
                await _context.VaccineReporting.AddAsync(report);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully saved vaccine report with RECNUM: {Recnum}", report.RECNUM);

                return Ok(("Report successfully created with RECNUM: {report.RECNUM}", report.RECNUM));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending vaccine report with RECNUM: {ReportRecnum}", report?.RECNUM.ToString() ?? "N/A");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred. Please try again later.");
            }
        }
    }
}