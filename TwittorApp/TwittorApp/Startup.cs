using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using TwittorApp.GraphQL;
using TwittorApp.Models;

namespace TwittorApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        private readonly IWebHostEnvironment _env;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (_env.IsProduction())
            {
                Console.WriteLine("==>Using Azure SQL Server DB");
                services.AddDbContext<Kasus2DbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("ProdConnection")));
            }

            else
            {
                services.AddDbContext<Kasus2DbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("LocalConn")));
            }

            services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddAuthorization();

            services.AddControllers();

            // DI Dependency Injection
            services.Configure<TokenSettings>(Configuration.GetSection("TokenSettings"));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration.GetSection("TokenSettings").GetValue<string>("Issuer"),
                        ValidateIssuer = true,
                        ValidAudience = Configuration.GetSection("TokenSettings").GetValue<string>("Audience"),
                        ValidateAudience = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("TokenSettings").GetValue<string>("Key"))),
                        ValidateIssuerSigningKey = true
                    };
                });
            services.Configure<KafkaSettings>(Configuration.GetSection("KafkaSettings"));
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
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
