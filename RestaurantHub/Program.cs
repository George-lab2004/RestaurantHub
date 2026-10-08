using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantHub.Middleware;
using RestaurantHub.Models;
using RestaurantHub.Services;
using RestaurantHub.Middleware;                    

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;


}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}


app.UseMiddleware<RequestLoggingMiddleware>();       // FIRST (outermost)
app.UseMiddleware<ExceptionHandlingMiddleware>();    // SECOND

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();                                   // keep this; UseExceptionHandler is gone: our middleware replaces it
}



app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();   // "who are you?" reads the cookie
app.UseAuthorization();    // "are you allowed?" needs to know who you are first

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
   pattern: "{controller=Menu}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
