using QQGroup_AIMessageBot.Models;
using QQGroup_AIMessageBot.Utils;
using System.Text.Json;

public class MessageProcessor
{
    public MessageProcessor()
    {
        // ���� MaxCountReached �¼�
        MessageCounter.MaxCountReached += OnMaxCountReached;
    }

    private void OnMaxCountReached()
    {
        // �½��̴߳���
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
                WriteIndented = false, // �����������
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // ��������
            };
            // ���л��������б�
            string jsonString = JsonSerializer.Serialize(simplifiedMessages, options);
            string text = await AIModelAPI.GetAIAnalyseAsync(jsonString);
            // ������ɺ�ͨ���¼����ؽ��
            MessageCounter.OnProcessedResultReady(text);
        }).Start();
    }
}
