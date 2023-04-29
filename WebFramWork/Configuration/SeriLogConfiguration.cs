using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WebFramWork.Configuration
{
    public static class SeriLogConfiguration
    {
        public static void GetSerilogConfig()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            Log.Logger = new LoggerConfiguration()
              .ReadFrom.Configuration(configuration)
              .CreateLogger();
        }
        public static void UseSeriLogAsLogger(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog();
        }
    }
}
