// 确保这行引用存在
using Microsoft.Data.SqlClient;

namespace 用户登录系统.DAL
{
    // 类名正确拼写
    public class DbConnectionFactory
    {
        // 私有只读字段
        private readonly string _connectionString;

        // 构造函数（与类名一致）
        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 方法完整写法
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}