using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // 用于操作会话（Session）
using 用户登录系统.BLL; // 引用BLL层的命名空间

namespace 用户登录系统.Controllers
{
    // 账户控制器：处理登录、注册、退出请求
    public class AccountController : Controller
    {
        // 私有字段：存储UserBLL实例（依赖注入传入）
        private readonly UserBLL _userBLL;

        // 构造函数：依赖注入UserBLL
        public AccountController(UserBLL userBLL)
        {
            _userBLL = userBLL;
        }

        // 1. 显示登录页面（GET请求，对应URL：/Account/Login）
        public IActionResult Login()
        {
            // 返回Login视图（后续创建的登录页面）
            return View();
        }

        // 2. 处理登录请求（POST请求，表单提交时触发）
        [HttpPost]
        public IActionResult Login(string userName, string password,bool remember=false)
        {
            // 调用BLL层验证登录
            bool isSuccess = _userBLL.ValidateLogin(userName, password);
            Console.WriteLine($"登录验证结果：{isSuccess}");
            Console.WriteLine($"输入的账号：{userName}，输入的密码：{password}");
            if (isSuccess)
            {
                // 登录成功：将当前用户名存入会话（表示用户已登录）
                HttpContext.Session.SetString("CurrentUser", userName);
                //========新增“记住我”记忆功能，时限为7天========//
                if (remember)//如果勾选了“记住我”
                {//写入3个cookie（账号，密码，记住我）
                    Response.Cookies.Append("SavedUserName", userName, new CookieOptions { Expires = DateTime.Now.AddDays(7) });
                    Response.Cookies.Append("SavedPassword", password, new CookieOptions { Expires = DateTime.Now.AddDays(7) });
                    Response.Cookies.Append("RememberMe", "true", new CookieOptions { Expires = DateTime.Now.AddDays(7) });
                }
                else
                {
                    //若未勾选“记住我”，清除Cookie
                    Response.Cookies.Delete("SavedUserName");
                    Response.Cookies.Delete("SavedPassword");
                    Response.Cookies.Delete("RememberMe");
                }
                    // 跳转到Home控制器的Index方法（后续可创建首页）
                    return RedirectToAction("Index", "Home");
            }
            else
            {
                // 登录失败：将错误信息传递到视图
                ViewBag.ErrorMsg = "用户名或密码错误";
                // 返回登录页面，显示错误信息
                return View();
            }
        }

        // 3. 显示注册页面（GET请求，对应URL：/Account/Register）
        public IActionResult Register()
        {
            // 返回Register视图（后续创建的注册页面）
            return View();
        }

        // 4. 处理注册请求（POST请求，表单提交时触发）
        [HttpPost]
        public IActionResult Register(string userName, string password, string email)
        {
            // 调用BLL层处理注册
            string result = _userBLL.RegisterUser(userName, password, email);
            // 将注册结果传递到视图
            ViewBag.ResultMsg = result;
            // 返回注册页面，显示结果
            return View();
        }

        // 5. 退出登录（GET请求，对应URL：/Account/Logout）
        public IActionResult Logout()
        {
            // 清除会话中的当前用户信息
            HttpContext.Session.Remove("CurrentUser");
            // 跳回登录页面
            return RedirectToAction("Login");
        }
    }
}