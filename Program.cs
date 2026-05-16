using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecureGate.Web.Data;
using SecureGate.Web.Filters;
using SecureGate.Web.Hubs;
using SecureGate.Web.Models.Auth;
using SecureGate.Web.Services.Implementations;
using SecureGate.Web.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ===== MVC =====
builder.Services.AddControllersWithViews();

// ===== SignalR =====
builder.Services.AddSignalR();

// ===== Database =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== Identity =====
builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ===== Authorization (permission policies) =====
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    foreach (var permission in Enum.GetValues<Permission>())
    {
        options.AddPolicy(HasPermissionAttribute.PolicyName(permission), policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

// ===== Services (DI) =====
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<ITurnstileService, TurnstileService>();
builder.Services.AddScoped<ICameraService, CameraService>();
builder.Services.AddScoped<IAccessLogService, AccessLogService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISettingService, SettingService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IPhotoStorageService, PhotoStorageService>();
builder.Services.AddScoped<ICameraUserService, CameraUserService>();
builder.Services.AddSingleton<ICameraCredentialProtector, CameraCredentialProtector>();

// ===== Python face-worker HTTP klienti =====
var faceWorkerUrl = builder.Configuration["FaceWorker:BaseUrl"] ?? "http://localhost:8001";
builder.Services.AddHttpClient<IFaceRecognitionClient, FaceRecognitionClient>(c =>
{
    c.BaseAddress = new Uri(faceWorkerUrl);
    c.Timeout = TimeSpan.FromSeconds(15); // Yuz aniqlash 1-3s, embedding ~500ms
});

var app = builder.Build();

// ===== Seed Database =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await IdentitySeeder.SeedAsync(userManager, roleManager);
}

// ===== Middleware Pipeline =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ===== MVC Route =====
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API controllers (attribute routing — /api/face-recognition/*)
app.MapControllers();

// ===== SignalR Hubs =====
app.MapHub<TurnstileHub>("/hubs/turnstile");
app.MapHub<CameraHub>("/hubs/camera");
app.MapHub<AlertHub>("/hubs/alert");
app.MapHub<DashboardHub>("/hubs/dashboard");

app.Run();
