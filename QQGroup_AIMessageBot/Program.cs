using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using QQGroup_AIMessageBot.Models;
using QQGroup_AIMessageBot.Utils;
using Serilog.Core;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Websocket.Client;

public class Program
{
    const int maxCount = 300;
    const long groupID = 1043884719;

    public static bool SendMessage(String message, WebsocketClient client)
    {
        var echo = Guid.NewGuid().ToString("N");
        var payload = new
        {
            action = "send_group_msg",
            @params = new
            {
                group_id = groupID,
                message = message,
                auto_escape = false
            },
        };
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);
        byte[] buffer = Encoding.UTF8.GetBytes("Bot online");
        var isSendSucceed = client.Send(bytes);
        return isSendSucceed;
    }
    public static async Task Main(string[] args)
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
                options.UseUtcTimestamp = false;
            });

            builder.SetMinimumLevel(LogLevel.Information);
        });

        ILogger logger = factory.CreateLogger("");
        logger.LogInformation("Hello World! Logging is {Description}.", "fun");

        var client = new WebsocketClient(new Uri("ws://127.0.0.1:8081"));

        var isSuccess = SendMessage("Bot online", client);
        if(isSuccess)
            logger.LogInformation("Bot online message sent successfully.");
        else
            logger.LogError("Failed to send bot online message.");

        client.ReconnectTimeout = TimeSpan.FromSeconds(10);
        var count = 0;
        var database = new SQLiteUtils("messages.db");
        client.MessageReceived.Subscribe(async(msg) => 
        {
            if (!msg.Text.StartsWith("{\"interval\":5000"))
            {
                //logger.LogInformation("收到 WS 消息：" + msg.Text);
                var groupMessage = GroupMessage.Create(msg.Text);
                if (groupMessage != null)
                {
                    if (groupMessage.messageType.Equals("text") && groupMessage.groupID.Equals(groupID.ToString()))
                    {
                        if (groupMessage.data.StartsWith("!aibot"))
                        {
                            if (groupMessage.data.Equals("!aibot analyze"))
                            {
                                SQLiteUtils.CopyDatabaseFile("messages.db", "messages_backup.db");
                                SQLiteUtils.TruncateFile("messages.db");

                                MessageCounter.OnMaxCountReached();
                            }

                        }
                        else
                        {
                            logger.LogInformation("[GroupMessage][" + groupMessage.groupID + "] " + groupMessage.nickname + ": " + groupMessage.data);
                            database.InsertMessage(groupMessage);
                            count++;
                            if (count >= maxCount)
                            {
                                SQLiteUtils.CopyDatabaseFile("messages.db", "messages_backup.db");
                                SQLiteUtils.TruncateFile("messages.db");

                                MessageCounter.OnMaxCountReached();
                            }
                        }
                    }
                }
            }
        });
        await client.Start();

        // 实例化处理器
        var processor = new MessageProcessor();

        // 订阅处理结果事件
        MessageCounter.ProcessedResultReady += (result) =>
        {
            // 这里进行发送或其他操作
            logger.LogInformation("处理结果：" + result);
            SendMessage(result, client);
            // 例如：client.Send(result);
        };



        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("收到退出信号...");
            e.Cancel = true;
            tcs.TrySetResult(null);
        };

        await tcs.Task;
    }
}
