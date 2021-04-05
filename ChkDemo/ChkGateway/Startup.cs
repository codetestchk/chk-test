using ChkDatabase;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ChkSDK.BankProxy;
using ChkSDK.Services;
using ChkSDK.SettingsModel;
using ChkGateway.Middleware;

namespace ChkGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<ChkDbContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("ChkConnection")));

            services.Configure<JwtSettings>(options => Configuration.GetSection("JwtSettings").Bind(options));

            // This is where I inject the fake bank proxy, this line would be altered in real system
            services.AddSingleton<ICardValidatorService, CardValidatorService>();
            services.AddScoped<IBankServiceProxy, FakeBankServiceProxy>();
            services.AddScoped<IMerchantService, MerchantService>();
            services.AddScoped<ITransactionService, TransactionService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // Custom JWT middleware that validates access to restricted methods
            app.UseMiddleware<JwtMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
