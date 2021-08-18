using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SampleShared;
using SampleShared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SampleBusService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private readonly IQueueService _azureBusService;
        private readonly IConfiguration _config;

        public MessageController(ILogger<MessageController> logger, IConfiguration config, IQueueService azureBusService)
        {
            _logger = logger;
            _config = config;
            _azureBusService = azureBusService;
        }

        /// <summary>
        /// Send message to person's email address
        /// </summary>
        /// <response code="200">Message successfully pushed to queue</response>
        /// <response code="401">Access denied</response>
        /// <response code="400">Model is not valid</response>
        /// <response code="500">Oops! something went wrong</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PersonModel), StatusCodes.Status400BadRequest)]
        [HttpPost("send-to-queue")]
        public async Task<IActionResult> SendMessageAsnc([FromBody] PersonModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Model not valid");
                }

                _azureBusService.ConnectionString = _config.GetValue<string>("AzureServiceBus:QueueConnectionString");
                _azureBusService.QueueName = _config.GetValue<string>("AzureServiceBus:QueueName");

                var person = new Person
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email
                };

                var payload = JsonSerializer.Serialize(person);

                await _azureBusService.SendMessageAsync(payload);

                return Ok(new { message = "Message successfully pushed to queue", data = person });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
