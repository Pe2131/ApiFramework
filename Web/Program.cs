using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Common;
using Microsoft.Extensions.Configuration;
using Serilog;
using WebFramework.Configuration;
using WebFramework.CustomMapping;
using WebFramework.Middlewares;
using WebFramWork.Configuration;

SeriLogConfiguration.GetSerilogConfig();
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
// Register services directly with Autofac here. Don't
// call builder.Populate(), that happens in AutofacServiceProviderFactory.
builder.Host.ConfigureContainer<ContainerBuilder>(builder => builder.AddServices());
builder.UseSeriLogAsLogger();

// Add services to the container.
var _siteSetting = builder.Configuration.GetSection(nameof(SiteSettings)).Get<SiteSettings>();
builder.Services.AddControllers();
builder.Services.InitializeAutoMapper();
builder.Services.AddDbContext(builder.Configuration);

builder.Services.AddCustomIdentity(_siteSetting.IdentitySettings);
builder.Services.AddMinimalMvc();
builder.Services.AddJwtAuthentication(_siteSetting.JwtSettings);

builder.Services.AddCustomApiVersioning();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//if (args.Length == 1 && args[0] == "init")
//{
    app.IntializeDatabase();
//}
app.UseCustomExceptionHandler();

app.UseHsts(app.Environment);
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
