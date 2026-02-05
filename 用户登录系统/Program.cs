// 引入必要的命名空间
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// 引用项目的 DAL 和 BLL 命名空间，以便注册服务
using 用户登录系统.DAL;
using 用户登录系统.BLL;

// 构建 Web 应用程序构建器（框架内置，负责创建应用和注册服务）
var builder = WebApplication.CreateBuilder(args);

// ===================== 第一部分：注册服务（依赖注入）=====================
// 1. 注册 MVC 控制器和视图服务（启用 MVC 功能，必须注册，否则无法处理控制器请求）
builder.Services.AddControllersWithViews();

// 2. 注册会话（Session）服务（用于保存用户登录状态，比如登录后存储当前用户名）
builder.Services.AddSession(options =>
{
    // 会话超时时间：30分钟（用户30分钟无操作，会话失效，需要重新登录）
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    // 会话Cookie仅服务器可访问（防止前端JS篡改，提升安全性）
    options.Cookie.HttpOnly = true;
    // 会话Cookie是必要的（确保会话功能正常工作，符合隐私规范）
    options.Cookie.IsEssential = true;
});

// 3. 注册数据库连接工厂（单例模式：整个项目生命周期只创建一个实例，全局共享）
// 注意：替换这里的数据库连接字符串为你自己的（和你创建 User 表的数据库一致）
builder.Services.AddSingleton<DbConnectionFactory>(new DbConnectionFactory(
    "Data Source=rm-wz9sj5xd9k30726dj5o.sqlserver.rds.aliyuncs.com,1433;Initial Catalog=kuro;User ID=kuro_1231;Password=mahiro_0306;Encrypt=True;TrustServerCertificate=True"
));

// 4. 注册 DAL 层服务（作用域模式：每个 HTTP 请求创建一个实例，请求结束释放，安全无数据混乱）
builder.Services.AddScoped<UserDAL>();

// 5. 注册 BLL 层服务（作用域模式：和 DAL 对应，每个请求一个实例）
builder.Services.AddScoped<UserBLL>();

// ===================== 第二部分：构建应用并配置 HTTP 请求管道（中间件）=====================
var app = builder.Build();

// 1. 环境判断：开发环境 vs 生产环境
if (!app.Environment.IsDevelopment())
{
    // 生产环境：自定义错误页面（不暴露详细错误给用户，更安全）
    app.UseExceptionHandler("/Home/Error");
    // 启用 HSTS（强制客户端后续用 HTTPS 访问，提升安全性）
    app.UseHsts();
}

// 2. 启用 HTTPS 重定向（将 HTTP 请求自动重定向到 HTTPS，提升安全性）
app.UseHttpsRedirection();

// 3. 启用静态文件访问（允许访问 wwwroot 文件夹下的 CSS、JS、图片等静态资源）
app.UseStaticFiles();

// 4. 启用会话（Session）功能（必须在 UseRouting 之前，UseEndpoints 之前，顺序不能乱）
app.UseSession();

// 5. 启用路由（负责匹配 URL 到对应的控制器和方法，比如 /Account/Login 匹配 AccountController 的 Login 方法）
app.UseRouting();

// 6. 启用授权（后续可以扩展权限控制，比如某些页面需要登录后才能访问）
app.UseAuthorization();

// 7. 配置默认路由（URL 匹配规则，项目的入口路由）
app.MapControllerRoute(
    name: "default", // 路由名称（自定义，唯一即可）
    pattern: "{controller=Account}/{action=Login}/{id?}"); // 路由模板

// 8. 运行 Web 应用（启动项目，监听请求）
app.Run();