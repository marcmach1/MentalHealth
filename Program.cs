var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<MentalHealthSupport.Services.IMentalHealthRepository, MentalHealthSupport.Services.InMemoryMentalHealthRepository>();

// Escolher provedor de IA dinamicamente
var provider = builder.Configuration["AiProvider"] ?? "OpenAI";

if (provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.Configure<MentalHealthSupport.Services.GoogleOptions>(
        builder.Configuration.GetSection("Google"));
    builder.Services.AddScoped<MentalHealthSupport.Services.IAiSupportService, MentalHealthSupport.Services.GoogleGeminiSupportService>();
}
else
{
    builder.Services.Configure<MentalHealthSupport.Services.OpenAiOptions>(
        builder.Configuration.GetSection("OpenAI"));
    builder.Services.AddScoped<MentalHealthSupport.Services.IAiSupportService, MentalHealthSupport.Services.OpenAiSupportService>();
}

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
