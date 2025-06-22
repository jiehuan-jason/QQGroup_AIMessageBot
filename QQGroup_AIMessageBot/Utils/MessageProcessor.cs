using QQGroup_AIMessageBot.Models;
using QQGroup_AIMessageBot.Utils;
using System.Text.Json;

public class MessageProcessor
{
    public MessageProcessor()
    {
        // 订阅 MaxCountReached 事件
        MessageCounter.MaxCountReached += OnMaxCountReached;
    }

    private void OnMaxCountReached()
    {
        // 新建线程处理
        new Thread(async() =>
        {
            var list = new SQLiteUtils("messages_backup.db").GetAllMessages();
            List<SimplifiedChatMessage> simplifiedMessages = list
                .Select(msg => new SimplifiedChatMessage
                {
                    Nickname = msg.nickname,
                    Data = msg.data
                })
                .ToList();
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = false, // 禁用美化输出
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 处理中文
            };
            // 序列化精简后的列表
            string jsonString = JsonSerializer.Serialize(simplifiedMessages, options);
            string text = await AIModelAPI.GetAIAnalyseAsync(jsonString);
            // 处理完成后通过事件返回结果
            MessageCounter.OnProcessedResultReady(text);
        }).Start();
    }
}
