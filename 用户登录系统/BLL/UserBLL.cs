using System.Security.Cryptography;
using System.Text;
using 用户登录系统.DAL; // 引用DAL层的命名空间，以便调用UserDAL

namespace 用户登录系统.BLL
{
    // 用户业务逻辑类：处理登录、注册的业务规则
    public class UserBLL
    {
        // 私有字段：存储UserDAL实例（依赖注入传入）
        private readonly UserDAL _userDAL;

        // 构造函数：依赖注入UserDAL
        public UserBLL(UserDAL userDAL)
        {
            _userDAL = userDAL;
        }

        // 辅助方法：密码加密（用MD5示例，实际项目推荐用BCrypt）
        private string EncryptPassword(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                // 将密码转成字节数组，计算MD5哈希
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                // 把哈希字节转成16进制字符串
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // 业务功能1：登录验证
        public bool ValidateLogin(string userName, string password)
        {
            // 1. 调用DAL层，根据用户名查用户
            var userRow = _userDAL.GetUserByUserName(userName);
            if (userRow == null) // 用户名不存在，直接返回失败
            {
                return false;
            }

            // 2. 加密用户输入的密码，和数据库中存储的加密密码对比
            string encryptedPwd = EncryptPassword(password);
            return userRow["Password"].ToString() == encryptedPwd;
        }

        // 业务功能2：注册用户
        public string RegisterUser(string userName, string password, string email)
        {
            // 1. 验证参数是否为空
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
            {
                return "用户名、密码、邮箱不能为空";
            }

            // 2. 调用DAL层，检查邮箱是否已存在
            if (_userDAL.IsEmailExists(email))
            {
                return "该邮箱已被注册";
            }

            // 3. 加密密码
            string encryptedPwd = EncryptPassword(password);

            // 4. 调用DAL层，新增用户
            bool success = _userDAL.AddUser(userName, encryptedPwd, email);
            return success ? "注册成功" : "注册失败，请重试";
        }
    }
}