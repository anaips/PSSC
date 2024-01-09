using Microsoft.AspNetCore.Mvc;
using ProiectPSSC.Domain.Commands;
using ProiectPSSC.Domain.Repositories;
using ProiectPSSC.Domain.Workflows;
using ProiectPSSC.Models;
using System.Text;
using CSharp.Choices;
using ProiectPSSC.Domain.WorkflowEvents;
using ProiectPSSC.Domain.Models;
using Newtonsoft.Json;
using LanguageExt.Common;
using static ProiectPSSC.Domain.Models.Comenzi;
using System.Net.Http;

namespace ProiectPSSC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommandsController : ControllerBase
    {
        private ILogger<CommandsController> logger;
        private readonly PublishComandaWorkflow publishOrderWorkflow;

        private readonly FacturareWorkflowv2 facturareWorkflowV2;
        private readonly IHttpClientFactory _httpClientFactory;

        public CommandsController(ILogger<CommandsController> logger, FacturareWorkflowv2 facturareWorkflowV2, PublishComandaWorkflow publishOrderWorkflow, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.publishOrderWorkflow = publishOrderWorkflow;
            this.facturareWorkflowV2 = facturareWorkflowV2;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("getAllOrders")]
        public async Task<IActionResult> GetAllOrders([FromServices] IComandaRepository ordersRepository) =>
            await ordersRepository.TryGetExistingOrders().Match(
               Succ: GetAllOrdersHandleSuccess,
               Fail: GetAllOrdersHandleError
            );

        private ObjectResult GetAllOrdersHandleError(Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return base.StatusCode(StatusCodes.Status500InternalServerError, "UnexpectedError");
        }

        private OkObjectResult GetAllOrdersHandleSuccess(List<ProiectPSSC.Domain.Models.CalculatedComanda> orders) =>
        Ok(orders.Select(order => new
        {
            Nume = order.Nume.Value,
            order.Adresa,
            order.NumeProdus,
            order.Cantitate,
            order.PretTotal
        }));

        [HttpPost("Place Order")]        
        public async Task<IActionResult> PublishOrders([FromBody] Input[] orders)
        {
            var unvalidatedOrders = orders.Select(MapInputOrderToUnvalidatedOrder)
                                          .ToList()
                                          .AsReadOnly();
            PublishComandaCommands command = new(unvalidatedOrders);
            var result = await publishOrderWorkflow.ExecuteAsync(command);

            return await result.MatchAsync(
                whenComandaPublishFaildEvent: HandleFailure,
                whenComandaPublishScucceededEvent: HandleSuccess
            );
        }

        private Task<IActionResult> HandleFailure(ComandaPublishedEvent.ComandaPublishFaildEvent failedEvent)
        {
            return Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status500InternalServerError, failedEvent.Reason));
        }

        private async Task<IActionResult> HandleSuccess(ComandaPublishedEvent.ComandaPublishScucceededEvent successEvent)
        {
            var w1 = TriggerReportGeneration(successEvent);
            var w2 = TriggerOrderCalculation(successEvent);
            await Task.WhenAll(w1, w2);
            return Ok();
        }

        private async Task<Boolean> TriggerReportGeneration(ComandaPublishedEvent.ComandaPublishScucceededEvent successEvent)
        {
            var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "https://localhost:7286/report/semester-report")
            {
                Content = new StringContent(JsonConvert.SerializeObject(successEvent), Encoding.UTF8, "application/json")
            };
            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(httpRequestMessage);
            return true;
        }

        private async Task<Boolean> TriggerOrderCalculation(ComandaPublishedEvent.ComandaPublishScucceededEvent successEvent)
        {
            var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "https://localhost:7286/report/scholarship")
            {
                Content = new StringContent(JsonConvert.SerializeObject(successEvent), Encoding.UTF8, "application/json")
            };
            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(httpRequestMessage);
            return true;
        }

        private static UnvalidatedComanda MapInputOrderToUnvalidatedOrder(Input order) => new UnvalidatedComanda(
            Nume: order.Nume,
            Adresa: order.Adresa,
            NumeProdus: order.NumeProdus,
            Cantitate: order.Cantitate
            );

        
    }
}