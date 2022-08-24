using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

[assembly: FunctionsStartup(typeof(ParrotBot.Startup))]

namespace ParrotBot
{
  public class Startup : FunctionsStartup
  {
    public override void Configure(IFunctionsHostBuilder builder)
    {
      builder.Services.AddLogging();

      string token = Environment.GetEnvironmentVariable("token", EnvironmentVariableTarget.Process) ??
        throw new ArgumentException("Can not get token. Set token in environment setting");

      builder.Services.AddHttpClient("tgclient")
             .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(token, httpClient));

      builder.Services.AddScoped<UpdateService>();
    }
  }
}