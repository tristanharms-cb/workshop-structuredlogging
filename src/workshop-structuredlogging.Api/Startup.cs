using System;
using System.Linq;
using System.Net;
using Coolblue.Utilities.ApplicationHealth;
using Coolblue.Utilities.ApplicationHealth.AspNetCore;
using Coolblue.Utilities.MonitoringEvents;
using Coolblue.Utilities.MonitoringEvents.Datadog;
using Coolblue.Utilities.MonitoringEvents.SimpleInjector;
using Coolblue.Utilities.RequestResponseLogging.AspNetCore;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

using SimpleInjector;
using SimpleInjector.Integration.AspNetCore.Mvc;
using SimpleInjector.Lifestyles;

using StatsdClient;

using Swashbuckle.AspNetCore.Swagger;
using workshop_structuredlogging.Api.Infrastructure;

using PhilosophicalMonkey;

using Coolblue.Sinks.Splunk;
using ChimpLab.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Coolblue.CorrelationId.AspNetCore;
using Coolblue.CorrelationId;
using Serilog.Events;
using workshop_structuredlogging.Api.Materials;

namespace workshop_structuredlogging.Api
{
    public class Startup
    {
        public static string AppName => "Logging workshop";

        private ILogger _logger;
        private MonitoringEvents _monitoringEvents;
        private WebServiceSettings _settings;
        private readonly Container _container = new Container();
        private readonly IHostingEnvironment _hostingEnvironment;

        public Action<Container> RegisterOverrides { get; set; } = c => { return; };

        public Startup(IHostingEnvironment env)
        {
            this._hostingEnvironment = env;
            ConfigureSettings();
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelationId();
            services.AddMvc()
                    .AddMvcOptions(opt => opt.OutputFormatters.RemoveType<StringOutputFormatter>());

            if (!_hostingEnvironment.IsProduction())
            {
                services.AddSwaggerGen(c => c.SwaggerDoc("v1", new Info { Title = $"{Startup.AppName} API", Version = "v1" }));
            }
            services.AddSingleton<IControllerActivator>(new SimpleInjectorControllerActivator(_container));

            services.UseSimpleInjectorAspNetRequestScoping(_container);
        }

        public void Configure(IApplicationBuilder app)
        {
            ConfigureLoggingAndMetrics();

            ConfigureSimpleInjector(app);
            ConfigureMiddleware(app);
        }

        private void ConfigureSettings()
        {
            var configuration = GetConfiguration();

            _settings = new WebServiceSettings(configuration);
        }

        private IConfigurationRoot GetConfiguration()
        {
            var personalGlobalConfigKey = $"{Startup.AppName}_global_config_file";
            var personalConfigKey = $"{Startup.AppName}_config_file";
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddJsonFileFromEnvironmentVariable(personalGlobalConfigKey, optional: true)
                .AddJsonFileFromEnvironmentVariable(personalConfigKey, optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            return config;
        }

        private void ConfigureLoggingAndMetrics()
        {
            ConfigureSerilog();
            ConfigureDatadog();

            _monitoringEvents = new MonitoringEvents(_logger, new DatadogMetrics());

            _monitoringEvents.ApplicationStart();
        }

        private void ConfigureSerilog()
        {
            var splunkConnectionInfo =
                new SplunkUdpSinkConnectionInfo(hostname: _settings.SplunkHost, port: _settings.SplunkUdpPort);

            var configuration = new LoggerConfiguration()
                .MinimumLevel.Is(_settings.GlobalMinimumLogLevel)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message,-30:lj} {Properties:j}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Literate)
                .WriteTo.Trace()
                //.WriteTo.SplunkViaUdp(splunkConnectionInfo, restrictedToMinimumLevel: _settings.SplunkMinimumLogLevel)
                .WriteTo.SplunkViaUdp(IPAddress.Loopback, 1515, LogEventLevel.Information)
                .Enrich.FromLogContext();

            if (!string.IsNullOrEmpty(_settings.SeqUrl))
            {
                configuration.WriteTo.Seq(_settings.SeqUrl);
            }

            _logger = configuration.CreateLogger();
        }

        private void ConfigureDatadog()
        {
            DogStatsd.Configure(new StatsdConfig
            {
                Prefix = Startup.AppName,
                StatsdServerName = "localhost"
            });
        }

        private void ConfigureSimpleInjector(IApplicationBuilder app)
        {
            _container.Options.PropertySelectionBehavior = new MonitoringEventsPropertySelectionBehavior();
            _container.Options.DefaultLifestyle = new AsyncScopedLifestyle();

            _container.RegisterSingleton(_settings);
            _container.RegisterSingleton(_monitoringEvents);
            _container.Register<ILongRunningProcess, LongRunningProcess>();

            _container.RegisterMvcControllers(app);

            _container.Register<ExceptionHandlingMiddleware>(() => new ExceptionHandlingMiddleware(_monitoringEvents, !_hostingEnvironment.IsProduction()));

            RegisterOverrides(_container);
        }

        private void ConfigureMiddleware(IApplicationBuilder app)
        {
            app.UseCorrelationId();

            app.Use((context, next) => _container.GetInstance<ExceptionHandlingMiddleware>().Invoke(context, next));

//            app.UseApplicationHealthEndpoints(_container.GetAllInstances<IHealthTest>().ToList(),
//                                              Reflect.OnTypes.GetAssembly(typeof(Startup)), applicationStartTime: DateTimeOffset.Now);

            app.UseMvc();

            if (!_hostingEnvironment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{Startup.AppName} API V1"));
            }
            app.UseRequestResponseLogging(AppName, _monitoringEvents, !_hostingEnvironment.IsProduction());
        }
    }
}