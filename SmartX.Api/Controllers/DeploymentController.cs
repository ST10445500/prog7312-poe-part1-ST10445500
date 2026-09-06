using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;
using SmartX.Shared.Models;

//ST10445500 - PROG7312 - SmartX POE
//DeploymentController

//.....................................o0oSTART OF FILEo0o........................................//

// The API controller receives HTTP requests and sends the work to services.

namespace SmartX.Api.Controllers
{
    //handles checking a deployment hierarchy before the gateway accepts it
    [ApiController]
    [Route("api/deployment")]
    public class DeploymentController : ControllerBase
    {
        private static readonly JsonSerializerOptions TreeJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            MaxDepth = 256
        };

        private readonly DeploymentTreeValidator _validator;

        public DeploymentController(DeploymentTreeValidator validator)
        {
            _validator = validator;
        }

        //..............................................................................//

        //validates a deployment tree such as zone, sub-zone, sensor
        [HttpPost("validate")]
        public async Task<ActionResult<DeploymentValidationResult>> Validate()
        {
            // The tree is read here rather than through a method parameter.
            // Model binding gives up on a deeply nested body and fails with a
            // 500 before the validator ever runs.
            string body;

            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return BadRequest("No deployment tree was supplied.");
            }

            DeploymentNode? root;

            try
            {
                root = JsonSerializer.Deserialize<DeploymentNode>(body, TreeJsonOptions);
            }
            catch (JsonException)
            {
                return BadRequest("The deployment tree could not be read. It is either malformed or nested far deeper than a real deployment would be.");
            }

            if (root == null)
            {
                return BadRequest("No deployment tree was supplied.");
            }

            return _validator.Validate(root);
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
