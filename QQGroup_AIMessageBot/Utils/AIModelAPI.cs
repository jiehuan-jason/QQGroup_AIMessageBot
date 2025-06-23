using OpenAI;
using OpenAI.Chat;
using QQGroup_AIMessageBot.Models;
using System.ClientModel;


namespace QQGroup_AIMessageBot.Utils
{
    public class AIModelAPI
    {

        private static AppSettings settings = new AppSettings();
        private static bool _isInit = false;
        static OpenAIClientOptions openAISettings;
        static ApiKeyCredential apiKey;
        static ChatClient chatClient;

        public AIModelAPI()
        {
        }

        private static async Task init()
        {
            _isInit = true;
            openAISettings = new()
            {
                Endpoint = new Uri(await GetBaseURL())
            };
            apiKey = new ApiKeyCredential(await GetApiKey());
            chatClient = new ChatClient(model: "gemini-2.5-flash-lite-preview-06-17", credential: apiKey, openAISettings);
        }

        private static async Task<string> GetApiKey()
        {
            return settings.ApiKey;
        }

        private static async Task<string> GetBaseURL()
        {
            return settings.BaseURL;
        }

        private static async Task<string> GetAIAnswer(string text)
        {
            if (!_isInit)
                init();
            ChatCompletion completion = await chatClient.CompleteChatAsync(text);
            return completion.Content[0].Text;
        }

        public static async Task<string> GetAIAnalyseAsync(string messageJson)
        {
            return await GetAIAnswer("作为QQ群聊总结者，提炼原始聊天记录为结构化摘要：识别主要主题、重复话题、重要讨论。\n**输出格式：**\nQQ群聊天内容总结\n1. [类别 1]\n\"[用户昵称]\"：[相关消息总结：简洁、核心，含关键词/群梗/显著行为。]\n\"[用户昵称]\"：[同类别另一用户总结。]...\n2. [类别 2]\n...\n**重点话题** (3-5个)\n[话题 1]：[讨论内容简述，含例子/重复子主题。]\n[话题 2]：[简述。]...\n**AI指令：**\n* **分类：** 按逻辑归类对话。\n* **总结：** 提取核心思想，简洁呈现，开头含用户昵称。\n* **关键词/短语：** 融入聊天特有词语、名字。\n* **重点话题：** 识别并简述3-5个核心话题。\n* **语气：** 中立客观。\n* **过滤：** 过滤敏感信息。消息的格式为json，sender代表发送者，message代表信息。\n以下是Json信息：" + messageJson);
        }

    }
}
