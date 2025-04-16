using _3PS.Contexts;
using _3PS.Helpers;
using _3PS.Models.Vaccines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace _3PS.Controllers
{
    /// <summary>
    /// Provides endpoints for accessing vaccine-related data, such as batch send information.
    /// </summary>
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
        /// Gets records from the vaccine batch send data source with pagination and optional filtering.
        /// </summary>
        /// <remarks>
        /// Retrieves data corresponding to the VwVaccineBatchSend model (sourced from DbContext configuration).
        /// Supports filtering by the 'SendingBatch' number.
        /// Results are ordered by Ticket number.
        /// <br/>
        /// **Pagination:**
        /// - Uses query parameters `pageNumber` and `pageSize`.
        /// - Default page size is 10, maximum is 50. Invalid sizes are clamped to the nearest valid value.
        /// - Pagination metadata is returned in custom 'X-Pagination-*' response headers. Ensure your client is configured to read these headers (requires 'Access-Control-Expose-Headers').
        /// </remarks>
        /// <param name="batch">Optional: Filter records by the 'SendingBatch' number.</param>
        /// <param name="pageNumber">The page number to retrieve (must be 1 or greater, defaults to 1 if invalid).</param>
        /// <param name="pageSize">The number of items per page (must be between 1 and 50, defaults to 10, clamped if invalid).</param>
        /// <returns>A paginated list of vaccine batch send records.</returns>
        /// <response code="200">Returns the requested page of vaccine batch send records. Pagination details are in the 'X-Pagination-*' headers.</response>
        /// <response code="400">Bad Request. Indicates an issue with the request parameters (though this implementation often defaults/clamps instead of returning 400 for simple range errors).</response>
        /// <response code="500">Internal Server Error. An error occurred while retrieving the data.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<VwVaccineBatchSend>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVaccineBatchSendData(
            [FromQuery] int? batch,
            [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1, 
            [FromQuery, Range(1, MaxPageSize)] int pageSize = DefaultPageSize)
        {
            int clampedPageSize = pageSize;
            if (pageSize < 1 || pageSize > MaxPageSize)
            {
                _logger.LogWarning("Invalid pageSize requested: {PageSize}. Clamping to range 1-{MaxPageSize}.", pageSize, MaxPageSize);
                clampedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);
            }
            if (pageNumber < 1)
            {
                _logger.LogWarning("Invalid pageNumber requested: {PageNumber}. Using default 1.", pageNumber);
                pageNumber = 1;
            }

            try
            {
                IQueryable<VwVaccineBatchSend> query = _context.VaccineBatchSends.AsNoTracking();

                if (batch.HasValue)
                {
                    query = query.Where(d => d.SendingBatch == batch.Value);
                }

                query = query.OrderBy(d => d.Ticket);

                var paginatedData = await PaginatedList<VwVaccineBatchSend>.CreateAsync(query, pageNumber, clampedPageSize);

                Response.Headers.Append("X-Pagination-Total-Count", paginatedData.TotalItems.ToString());
                Response.Headers.Append("X-Pagination-Page-Size", paginatedData.PageSize.ToString());
                Response.Headers.Append("X-Pagination-Current-Page", paginatedData.PageNumber.ToString());
                Response.Headers.Append("X-Pagination-Total-Pages", paginatedData.TotalPages.ToString());
                Response.Headers.Append("Access-Control-Expose-Headers", "X-Pagination-Total-Count, X-Pagination-Page-Size, X-Pagination-Current-Page, X-Pagination-Total-Pages");

                return Ok(paginatedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving paginated vaccine batch send data. Filter: {Filter}, Page: {Page}, Size: {Size}", batch, pageNumber, clampedPageSize);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred. Please try again later.");
            }
        }
    }
}