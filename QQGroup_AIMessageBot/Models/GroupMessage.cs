using System.Text.Json;

namespace QQGroup_AIMessageBot.Models
{

    internal class GroupMessage
    {

        public Dictionary<string, JsonElement> dict { get; set; }
        public string messageID { get; set; }
        public string nickname { get; set; }
        public string userID { get; set; }
        public string groupID { get; set; }
        public string messageType { get; set; }
        public string data { get; set; }
        public string timestamp { get; set; }


        public GroupMessage()
        {

        }
        private GroupMessage(string json)
        {

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, options);

            messageID = dict["message_id"].ToString() ?? "";
            nickname = dict["sender"].GetProperty("nickname").ToString() ?? "";
            userID = dict["sender"].GetProperty("user_id").ToString() ?? "";
            groupID = dict["group_id"].ToString() ?? "";
            timestamp = dict["time"].ToString() ?? "";
            var firstMsg = dict["message"][0];
            messageType = firstMsg.GetProperty("type").GetString() ?? "";
            if (messageType == "text")
                data = firstMsg.GetProperty("data").GetProperty("text").GetString() ?? "";
            else if (messageType == "image")
                data = firstMsg.GetProperty("data").GetProperty("url").GetString() ?? "";
            else
                data = firstMsg.GetProperty("data").ToString() ?? "";
        }

        static public bool IsGroupMessage(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Dictionary<string, JsonElement> dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, options);
            return
            dict.TryGetValue("message_type", out var messageType) &&
            messageType.GetString() == "group" &&
            dict.TryGetValue("post_type", out var postType) &&
            postType.GetString() == "message";
        }

        public static GroupMessage? Create(string json)
        {
            try
            {

                if (IsGroupMessage(json))
                {
                    var groupMessage = new GroupMessage(json);
                    return groupMessage;
                }
            }
            catch (Exception ex)
            {

            }
            return null;


        }


    }
}

