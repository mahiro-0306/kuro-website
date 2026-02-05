// 引入必要的命名空间
using Microsoft.Data.SqlClient; // 新版SQL Server连接（已安装，无红色波浪线）
using System.Data; // 包含 DataTable、DataRow 等数据容器类

namespace 用户登录系统.DAL
{
    // 用户数据访问类：所有和 User 表相关的数据库操作都放在这里
    public class UserDAL
    {
        // 私有字段：存储数据库连接工厂实例（通过依赖注入传入）
        private readonly DbConnectionFactory _dbFactory;

        // 构造函数：依赖注入 DbConnectionFactory
        // 后续 Program.cs 注册服务时，会自动把 DbConnectionFactory 实例传入这里
        // 不需要手动 new，这就是依赖注入的便捷性（和 C 语言手动传参初始化工具类对应）
        public UserDAL(DbConnectionFactory dbFactory)
        {
            // 把传入的连接工厂保存起来，供后续创建数据库连接使用
            _dbFactory = dbFactory;
        }

        // 功能1：根据用户名查询用户（登录时验证用）
        // 返回 DataRow：相当于一条用户记录（对应 C 语言的 struct User 结构体）
        public DataRow GetUserByUserName(string userName)
        {
            // using 语句：自动释放资源（连接、命令等），避免内存泄漏（对应 C 语言的手动 free）
            using (SqlConnection conn = _dbFactory.CreateConnection()) // 从工厂获取数据库连接
            {
                // 编写 SQL 查询语句：参数化查询（@UserName），防止 SQL 注入攻击
                string sql = "SELECT * FROM [User] WHERE UserName = @UserName";

                // 创建 SQL 命令对象：绑定 SQL 语句和数据库连接
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // 给 SQL 参数赋值：避免直接拼接字符串导致 SQL 注入
                    cmd.Parameters.AddWithValue("@UserName", userName);

                    // 打开数据库连接
                    conn.Open();

                    // 创建数据适配器：负责执行 SQL 并填充数据到 DataTable
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        // DataTable：相当于内存中的一张临时表（对应 C 语言的结构体数组）
                        DataTable dt = new DataTable();

                        // 填充数据：执行 SQL，把查询结果存入 DataTable
                        adapter.Fill(dt);

                        // 判断：如果查询到数据，返回第一行（一个用户名唯一），否则返回 null
                        return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                    }
                }
            }
        }

        // 功能2：新增用户（注册时保存用户数据用）
        // 返回 bool：表示新增是否成功（true 成功，false 失败）
        public bool AddUser(string userName, string password, string email)
        {
            using (SqlConnection conn = _dbFactory.CreateConnection())
            {
                // 编写 SQL 插入语句：参数化查询，防止 SQL 注入
                string sql = "INSERT INTO [User](UserName, Password, Email) VALUES (@UserName, @Password, @Email)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // 给三个参数赋值（对应用户名、加密后的密码、邮箱）
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@Email", email);

                    // 打开数据库连接
                    conn.Open();

                    // 执行插入操作：ExecuteNonQuery() 返回受影响的行数
                    // 插入成功则返回 1（受影响 1 行），大于 0 即为成功
                    int affectedRows = cmd.ExecuteNonQuery();
                    return affectedRows > 0;
                }
            }
        }

        // 功能3：检查邮箱是否已存在（注册时验证，避免重复注册）
        // 返回 bool：true 表示已存在，false 表示未存在
        public bool IsEmailExists(string email)
        {
            using (SqlConnection conn = _dbFactory.CreateConnection())
            {
                // SQL 统计语句：查询该邮箱对应的记录数
                string sql = "SELECT COUNT(1) FROM [User] WHERE Email = @Email";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    conn.Open();

                    // 执行查询：ExecuteScalar() 返回查询结果的第一行第一列（这里是记录数）
                    int count = (int)cmd.ExecuteScalar();

                    // 记录数大于 0 表示邮箱已存在
                    return count > 0;
                }
            }
        }
    }
}