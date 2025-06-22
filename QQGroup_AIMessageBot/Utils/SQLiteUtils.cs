using Microsoft.Data.Sqlite;
using QQGroup_AIMessageBot.Models;
using SQLitePCL;

namespace QQGroup_AIMessageBot.Utils
{
    internal class SQLiteUtils
    {
        private string _connectionString;

        public SQLiteUtils(string databasePath)
        {
            _connectionString = $"Data Source={databasePath}";
            Batteries.Init();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Messages (
                    MessageID TEXT PRIMARY KEY,
                    Nickname TEXT,
                    UserID TEXT,
                    GroupID TEXT,
                    MessageType TEXT,
                    Data TEXT,
                    Timestamp TEXT
                );"; // TEXT for DateTime storage
                command.ExecuteNonQuery();
            }
        }

        public void InsertMessage(GroupMessage message)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT INTO Messages (MessageID, Nickname, UserID, GroupID, MessageType, Data, Timestamp)
            VALUES (@MessageID, @Nickname, @UserID, @GroupID, @MessageType, @Data, @Timestamp);";

                // 使用 AddWithValue 方法添加参数，Sqlite 会自动处理类型映射
                command.Parameters.AddWithValue("@MessageID", message.messageID);
                command.Parameters.AddWithValue("@Nickname", message.nickname ?? (object)DBNull.Value); // 处理可能的 null 值
                command.Parameters.AddWithValue("@UserID", message.userID ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@GroupID", message.groupID ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@MessageType", message.messageType ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Data", message.data ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Timestamp", message.timestamp ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
            }
        }
        public GroupMessage GetMessageByID(string messageID)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MessageID, Nickname, UserID, GroupID, MessageType, Data, Timestamp FROM Messages WHERE MessageID = @MessageID;";
                command.Parameters.AddWithValue("@MessageID", messageID);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new GroupMessage
                        {
                            messageID = reader.GetString(0),
                            nickname = reader.IsDBNull(1) ? null : reader.GetString(1),
                            userID = reader.IsDBNull(2) ? null : reader.GetString(2),
                            groupID = reader.IsDBNull(3) ? null : reader.GetString(3),
                            messageType = reader.IsDBNull(4) ? null : reader.GetString(4),
                            data = reader.IsDBNull(5) ? null : reader.GetString(5),
                            timestamp = reader.IsDBNull(6) ? null : reader.GetString(6)
                            // dict 属性会被忽略，因为我们没有存储它
                        };
                    }
                    return null; // 没有找到对应的消息
                }
            }
        }

        public List<GroupMessage> GetAllMessages()
        {
            var messages = new List<GroupMessage>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT MessageID, Nickname, UserID, GroupID, MessageType, Data, Timestamp FROM Messages;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        messages.Add(new GroupMessage
                        {
                            messageID = reader.GetString(0),
                            nickname = reader.IsDBNull(1) ? null : reader.GetString(1),
                            userID = reader.IsDBNull(2) ? null : reader.GetString(2),
                            groupID = reader.IsDBNull(3) ? null : reader.GetString(3),
                            messageType = reader.IsDBNull(4) ? null : reader.GetString(4),
                            data = reader.IsDBNull(5) ? null : reader.GetString(5),
                            timestamp = reader.IsDBNull(6) ? null : reader.GetString(6)
                        });
                    }
                }
            }
            return messages;
        }

        public static void CopyDatabaseFile(string sourceFilePath, string destinationFilePath)
        {
            if (!File.Exists(sourceFilePath))
            {
                Console.WriteLine($"错误：源文件 '{sourceFilePath}' 不存在。");
                return;
            }

            try
            {
                // 确保目标目录存在
                string? destinationDirectory = Path.GetDirectoryName(destinationFilePath);
                if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                // 执行文件复制
                // overwrite: true 表示如果目标文件已存在，则覆盖它
                File.Copy(sourceFilePath, destinationFilePath, true);
                Console.WriteLine($"数据库文件已成功从 '{sourceFilePath}' 复制到 '{destinationFilePath}'。");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"文件复制过程中发生I/O错误: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"没有权限访问文件: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"复制文件时发生未知错误: {ex.Message}");
            }
        }
        public static void TruncateFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"文件 '{filePath}' 不存在，无法清空。");
                return;
            }

            try
            {
                // 打开文件，使用 FileAccess.Write 模式，并确保共享模式允许其他读取者
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                {
                    fs.SetLength(0); // 将文件长度设置为0，从而清空其内容
                }
                Console.WriteLine($"原始文件 '{filePath}' 的内容已成功清空。");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"清空文件时发生I/O错误: {ex.Message}");
                Console.WriteLine("请确保文件没有被其他进程占用。");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"没有权限清空文件: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清空文件时发生未知错误: {ex.Message}");
            }
        }
    }
}
