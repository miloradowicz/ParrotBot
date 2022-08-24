using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ParrotBot
{
  public class UpdateService
  {
    private readonly ITelegramBotClient botClient;
    private readonly ILogger<UpdateService> logger;

    public UpdateService(ITelegramBotClient botClient, ILogger<UpdateService> logger)
    {
      this.botClient = botClient;
      this.logger = logger;
    }

    public async Task Update(Update update)
    {
      if(update.Message is not { } message)
      {
        return;
      }

      this.logger
          .LogInformation(
        $"Message received. Id: {message.MessageId}, Type: {message.Type}, ChatId: {message.Chat}, From: {message.From?.Id ?? long.MinValue}");

      try
      {
        Message? sentMessage = null;

        string signature = message.ForwardFrom is not null
                           ? $"\nот <a href=\"tg://user?id={message.ForwardFrom!.Id}\">{WebUtility.HtmlEncode(message.ForwardFrom!.FirstName + " " + message.ForwardFrom!.LastName)}</a>" + $"\nпереслано <a href=\"tg://user?id={message.From!.Id}\">{WebUtility.HtmlEncode(message.From!.FirstName + " " + message.From!.LastName)}</a>"
                           : $"\nот <a href=\"tg://user?id={message.From!.Id}\">{WebUtility.HtmlEncode(message.From!.FirstName + " " + message.From!.LastName)}</a>";

        switch(message.Type)
        {
          case MessageType.Text:
          {
            sentMessage = await this.botClient
                                    .SendTextMessageAsync(
                                  chatId: message.Chat.Id,
                                  text: WebUtility.HtmlEncode(message.Text)! + signature,
                                  replyToMessageId: message.ReplyToMessage?.MessageId,
                                  parseMode: ParseMode.Html);
            break;
          }

          case MessageType.Photo:
          {
            sentMessage = await this.botClient
                                    .SendPhotoAsync(
                                  chatId: message.Chat.Id,
                                  photo: message.Photo!.Last().FileId,
                                  caption: message.Caption is not null
                                           ? $"{WebUtility.HtmlEncode(message.Caption)!}" + signature
                                           : signature,
                                  replyToMessageId: message.ReplyToMessage?.MessageId,
                                  parseMode: ParseMode.Html);
            break;
          }

          case MessageType.Video:
          {
            sentMessage = await this.botClient
                                    .SendVideoAsync(
                                  chatId: message.Chat.Id,
                                  video: message.Video!.FileId,
                                  caption: message.Caption is not null
                                           ? $"{WebUtility.HtmlEncode(message.Caption)!}" + signature
                                           : signature,
                                  replyToMessageId: message.ReplyToMessage?.MessageId,
                                  parseMode: ParseMode.Html);
            break;
          }

          case MessageType.Document:
          {
            sentMessage = await this.botClient
                                    .SendDocumentAsync(
                                  chatId: message.Chat.Id,
                                  document: message.Document!.FileId,
                                  caption: message.Caption is not null
                                           ? $"{WebUtility.HtmlEncode(message.Caption)!}" + signature
                                           : signature,
                                  replyToMessageId: message.ReplyToMessage?.MessageId,
                                  parseMode: ParseMode.Html);
            break;
          }
        }

        if(sentMessage is not null)
        {
          this.logger
              .LogInformation(
            $"Message sent. Id: {sentMessage.MessageId}, Type: {sentMessage.Type}, ChatId: {sentMessage.Chat}");

          await this.botClient.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId);

          this.logger
              .LogInformation($"Message deleted. Id: {message.MessageId}, Type: {message.Type}, ChatId: {message.Chat}");
        }
      }
      catch(ApiRequestException ex)
      {
        this.logger.LogError(ex.Message);
      }
    }
  }
}