using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Ninject;
using Ninject.Activation;
using Ninject.Infrastructure.Disposal;
using ucubot.Infrastructure;
using ucubot.DBCode;

namespace ucubot
{
    public class Startup
    {
        
        private readonly AsyncLocal<Scope> scopeProvider = new AsyncLocal<Scope>();
        private IReadOnlyKernel Kernel { get; set; }

        private object Resolve(Type type) => Kernel.Get(type);
        private object RequestScope(IContext context) => scopeProvider.Value;
        
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile($"appsettings.Db.json", optional: false)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            
            StudentRepository = new StudentEndpointDBCode(Configuration);
            LessonSignalRepository = new LessonSignalEndpointDBCode(Configuration);
            StudentSignalsRepository = new StudentSignalsEndpointDBCode(Configuration);
        }

        private IConfiguration Configuration { get; }
        private IStudentRepository StudentRepository { get; }
        private ILessonSignalRepository LessonSignalRepository { get; }
        private IStudentSignalsRepository StudentSignalsRepository { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc()
                // Make api to convert snake_case into CamelCase
                .AddJsonOptions(x =>
                {
                    x.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    };
                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddRequestScopingMiddleware(() => scopeProvider.Value = new Scope());
//            services.AddCustomControllerActivation(Resolve);
//            services.AddCustomViewComponentActivation(Resolve);
            services.AddSingleton<IConfiguration>(f => Configuration);
            services.AddSingleton<IStudentRepository>(f => StudentRepository);
            services.AddSingleton<ILessonSignalRepository>(f => LessonSignalRepository);
            services.AddSingleton<IStudentSignalsRepository>(f => StudentSignalsRepository);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Kernel = RegisterApplicationComponents(app, loggerFactory);

            // Add custom middleware
//            app.Use(async (context, next) =>
//            {
//                await Kernel.Get<CustomMiddleware>().Invoke(context, next);
//            });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        
        private IReadOnlyKernel RegisterApplicationComponents(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            IKernelConfiguration config = new KernelConfiguration();

            // Register application services
            config.Bind(app.GetControllerTypes()).ToSelf().InScope(RequestScope);

            // Cross-wire required framework services
            config.BindToMethod(app.GetRequestService<IViewBufferScope>);
            config.Bind<ILoggerFactory>().ToConstant(loggerFactory);

            config.Bind<IConfiguration>().ToConstant(Configuration);
            config.Bind<IStudentRepository>().ToConstant(StudentRepository);
            config.Bind<ILessonSignalRepository>().ToConstant(LessonSignalRepository);
            config.Bind<IStudentSignalsRepository>().ToConstant(StudentSignalsRepository);

            return config.BuildReadonlyKernel();
        }
            
        private sealed class Scope : DisposableObject { }
    }
    
    public static class BindingHelpers
    {
        public static void BindToMethod<T>(this IKernelConfiguration config, Func<T> method) => config.Bind<T>().ToMethod(c => method());
    }
}
