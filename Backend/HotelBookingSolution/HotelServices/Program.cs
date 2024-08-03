using HotelBooking.Interfaces;
using HotelServices.Contexts;
using HotelServices.Interfaces;
using HotelServices.Models;
using HotelServices.Repositories;
using HotelServices.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace HotelServices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.


            // Register IHttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSwaggerGen(option =>
            {
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {  }
                }
            });
            });

            #region Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["TokenKey:JWT"]))
                };

            });
            #endregion

            #region contexts
            builder.Services.AddDbContext<HotelServicesContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection"))
                );
            #endregion

            //builder.Services.AddHttpClient("BookingService", client =>
            //{
            //    client.BaseAddress = new Uri("https://localhost:7263/");
            //});

            builder.Services.AddHttpClient("BookingService", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7263/");
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });


            #region repositories
            builder.Services.AddScoped<IRepository<int, Room>, RoomRepository>();
            builder.Services.AddScoped<IRepository<int, Hotel>, HotelRepository>();
            builder.Services.AddScoped<IRepository<int,Amenity>,AmenitiesRepository>();
            #endregion

            #region services
            builder.Services.AddScoped<IHotelServices, HotelsServices>();
            builder.Services.AddScoped<IAmenityService, AmenitiesServices>();
            builder.Services.AddScoped<IRoomService, RoomService>();
            //builder.Services.AddScoped<IAzureBlobService, AzureBlobService>();
            builder.Services.AddScoped<IAzureBlobService>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("AzureBlobStorage");
                var containerName = configuration.GetValue<string>("BlobContainerName");
                return new AzureBlobService(connectionString, containerName);
            });
            #endregion

            #region CORS
            builder.Services.AddCors(opts => {
                opts.AddPolicy("MyCors", options =>
                {
                    options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                });
            });
            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("MyCors");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpLogging();
            app.UseWebSockets();
            app.MapControllers();


            app.Run();
        }
    }
}
