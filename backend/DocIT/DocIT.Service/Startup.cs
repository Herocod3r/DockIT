﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDbGenericRepository;
using Swashbuckle.AspNetCore.Swagger;

namespace DocIT.Service
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
            //services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var settings = new Models.Settings(Configuration);
            services.AddSingleton(settings);
            var context = new MongoDbContext(settings.MongoConnectionString,settings.DbName);
            services.AddTransient<MongoDB.Driver.IMongoDatabase>((x) =>new MongoDbContext(settings.MongoConnectionString, settings.DbName).Database);
            services.AddIdentity<Models.ApplicationUser, Models.ApplicationRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;


                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                // ApplicationUser settings
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@.-_";

            }).AddMongoDbStores<IMongoDbContext>(context).AddDefaultTokenProviders();
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
          .AddJwtBearer(x =>
          {
              x.RequireHttpsMetadata = false;
              x.SaveToken = true;
              x.TokenValidationParameters = new TokenValidationParameters
              {
                  ValidateIssuerSigningKey = true,
                  IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.JwtSecurityKey)),
                  ValidateIssuer = false,
                  ValidateAudience = false
              };
          });
            services.ConfigureCoreApp();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "DocIT API",
                    Version = "v0.1.0",
                    Description = "DocIT REST API",
                    Contact = new Contact
                    {
                        Name = "DocIT API Developers",
                        Email = "jethromain@gmail.com",
                        Url = "https://localhost:5000"
                    }
                });
                options.AddSecurityDefinition("Bearer",
                new ApiKeyScheme
                {
                    In = "header",
                    Description = "Please enter into field the word 'Bearer' following by space and JWT",
                    Name = "Authorization",
                    Type = "apiKey"
                });
                options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> {
                { "Bearer", Enumerable.Empty<string>() },
            });


                var basePath = AppContext.BaseDirectory;
                var assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                var fileName = Path.GetFileName(assemblyName + ".xml");
                options.IncludeXmlComments(System.IO.Path.Combine(basePath, fileName));

               // options.SchemaFilter<SchemaFilter>();
                //options.OperationFilter<SecurityRequirementsOperationFilter>();
            });
            services.AddCors(options =>
            {
                options.AddPolicy("AllowMyOrigin",
                builder1 => builder1
                .WithOrigins("*")
                .WithHeaders("*")
                .WithMethods("*")
                .WithExposedHeaders("X-Pagination")
                .SetPreflightMaxAge(TimeSpan.FromSeconds(2520))
                );
            });

            DocIT.Core.AutoMapperConfig.RegisterMappings();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCors("AllowMyOrigin");
            app.UseAuthentication();
            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DocIT API");
                c.RoutePrefix = string.Empty;


            });

            app.UseMvc();
        }
    }
}
