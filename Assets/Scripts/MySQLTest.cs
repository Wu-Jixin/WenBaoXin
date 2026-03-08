using UnityEngine;
using MySql.Data.MySqlClient;
using System;
using System.Data;

public class MySQLTest : MonoBehaviour
{
    [Header("数据库连接设置")]
    public string server = "127.0.0.1";
    public string database = "wenbaoxin";
    public string userId = "root";
    public string password = "root";
    public int port = 3306;

    [Header("调试选项")]
    public bool testOnStart = true;  // 是否在启动时自动测试

    private string connectionString;

    void Start()
    {
        // 构建连接字符串
        connectionString = $"Server={server};Port={port};Database={database};Uid={userId};Pwd={password};";

        if (testOnStart)
        {
            TestConnection();
            ReadGuidanceData();
        }
    }

    /// <summary>
    /// 测试数据库连接
    /// </summary>
    public void TestConnection()
    {
        Debug.Log("🔄 正在测试数据库连接...");

        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                Debug.Log("✅ 数据库连接成功！");
                Debug.Log($"📌 服务器版本: {conn.ServerVersion}");
                Debug.Log($"📌 数据库: {conn.Database}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ 连接失败：" + e.Message);
        }
    }

    /// <summary>
    /// 读取 interaction_guidance 表的所有数据
    /// </summary>
    public void ReadGuidanceData()
    {
        Debug.Log("🔄 正在读取 interaction_guidance 表...");

        string query = "SELECT * FROM interaction_guidance;";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        Debug.Log("========== 📋 interaction_guidance 表数据 ==========");

                        int count = 0;
                        while (reader.Read())
                        {
                            count++;

                            int id = reader.GetInt32("id");
                            string sceneName = reader.GetString("scene_name");
                            string triggerAction = reader.GetString("trigger_action");
                            string correctAction = reader.GetString("correct_action");
                            string feedbackText = reader.GetString("feedback_text");
                            int attemptCount = reader.GetInt32("attempt_count");
                            int severity = reader.GetInt32("severity");

                            Debug.Log($"【第{count}条】ID: {id}");
                            Debug.Log($"  场景: {sceneName}");
                            Debug.Log($"  触发行为: {triggerAction}");
                            Debug.Log($"  正确行为: {correctAction}");
                            Debug.Log($"  尝试次数: {attemptCount}");
                            Debug.Log($"  严重度: {severity}");
                            Debug.Log($"  💬 提示: {feedbackText}");
                            Debug.Log("----------------------------");
                        }

                        Debug.Log($"✅ 共读取 {count} 条数据");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ 读取失败：" + e.Message);
        }
    }

    /// <summary>
    /// 根据场景和触发行为查询特定的提示文本
    /// </summary>
    public string GetFeedbackByTrigger(string sceneName, string triggerAction)
    {
        string query = $"SELECT feedback_text FROM interaction_guidance WHERE scene_name='{sceneName}' AND trigger_action='{triggerAction}';";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        return result.ToString();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ 查询失败：" + e.Message);
        }

        return null; // 没找到返回空
    }

    /// <summary>
    /// 根据场景和触发行为查询完整的记录
    /// </summary>
    public DataRow GetGuidanceByTrigger(string sceneName, string triggerAction)
    {
        string query = $"SELECT * FROM interaction_guidance WHERE scene_name='{sceneName}' AND trigger_action='{triggerAction}' LIMIT 1;";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        return dt.Rows[0];
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ 查询失败：" + e.Message);
        }

        return null;
    }

    /// <summary>
    /// 更新数据（示例：增加尝试次数）
    /// </summary>
    public void IncrementAttemptCount(int id)
    {
        string query = $"UPDATE interaction_guidance SET attempt_count = attempt_count + 1 WHERE id = {id};";

        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    int rowsAffected = cmd.ExecuteNonQuery();
                    Debug.Log($"✅ 已更新 {rowsAffected} 条记录");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ 更新失败：" + e.Message);
        }
    }

    // 调试用：按空格键测试查询
    // 调试用：按 P 键测试查询，按 O 键重新读取全部数据
    void Update()
    {
        // 按 P 键测试查询第一条数据
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("🧪 按 P 键测试查询");

            string feedback = GetFeedbackByTrigger("tool_select", "choose_hand_trowel");
            if (!string.IsNullOrEmpty(feedback))
            {
                Debug.Log($"📢 查询结果: {feedback}");
            }
            else
            {
                Debug.Log("⚠️ 未找到匹配数据");
            }
        }

        // 按 O 键测试读取全部数据
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("🧪 按 O 键重新读取全部数据");
            ReadGuidanceData();
        }
    }
}
