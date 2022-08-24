using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace ParrotBot
{
  public class TelegramBotFunction
  {
    private readonly UpdateService updateService;

    public TelegramBotFunction(UpdateService updateService)
    {
      this.updateService = updateService;
    }

    [FunctionName("TelegramBot")]
    public async Task<IActionResult> Update(
                                       [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request,
                                       ILogger logger)
    {
      try
      {
        string body = await request.ReadAsStringAsync();
        Update update = JsonConvert.DeserializeObject<Update>(body);

        await this.updateService.Update(update);

        return new OkResult();
      }
      catch(JsonException ex)
      {
        logger.LogError(ex.Message);

        return new BadRequestResult();
      }
    }
  }
}
