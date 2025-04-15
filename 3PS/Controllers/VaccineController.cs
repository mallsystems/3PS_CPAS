using _3PS.Contexts;
using _3PS.Helpers;
using _3PS.Models.Vaccines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace _3PS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccineController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VaccineController> _logger;
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 50;

        public VaccineController(ApplicationDbContext context, ILogger<VaccineController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets records from the vw_VaccineBatchSend view with pagination and filtering.
        /// </summary>
        /// <param name="sendingBatchFilter">Optional: Filter by the sending batch number.</param>
        /// <param name="pageNumber">The page number to retrieve (default is 1).</param>
        /// <param name="pageSize">The number of items per page (default is 10, max is 50).</param>
        /// <returns>A paginated list of vaccine batch send records.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<VwVaccineBatchSend>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVaccineBatchSendData(
            [FromQuery] int? batch,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            // Validate pageSize
            if (pageSize < 1 || pageSize > MaxPageSize)
            {
                _logger.LogWarning("Invalid pageSize requested: {PageSize}. Using default/max.", pageSize);
                pageSize = Math.Clamp(pageSize, 1, MaxPageSize); // Clamp to valid range
            }
            if (pageNumber < 1)
            {
                _logger.LogWarning("Invalid pageNumber requested: {PageNumber}. Using default 1.", pageNumber);
                pageNumber = 1;
            }


            try
            {
                IQueryable<VwVaccineBatchSend> query = _context.VaccineBatchSends.AsNoTracking();

                // Apply filtering
                if (batch.HasValue)
                {
                    query = query.Where(d => d.SendingBatch == batch.Value);
                }

                query = query.OrderBy(d => d.Ticket);

                var paginatedData = await PaginatedList<VwVaccineBatchSend>.CreateAsync(query, pageNumber, pageSize);

                Response.Headers.Append("X-Pagination-Total-Count", paginatedData.TotalItems.ToString());
                Response.Headers.Append("X-Pagination-Page-Size", paginatedData.PageSize.ToString());
                Response.Headers.Append("X-Pagination-Current-Page", paginatedData.PageNumber.ToString());
                Response.Headers.Append("X-Pagination-Total-Pages", paginatedData.TotalPages.ToString());
                Response.Headers.Append("Access-Control-Expose-Headers", "X-Pagination-Total-Count, X-Pagination-Page-Size, X-Pagination-Current-Page, X-Pagination-Total-Pages");


                return Ok(paginatedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving paginated vaccine batch send data. Filter: {Filter}, Page: {Page}, Size: {Size}", batch, pageNumber, pageSize);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred. Please try again later.");
            }
        }
    }
}
