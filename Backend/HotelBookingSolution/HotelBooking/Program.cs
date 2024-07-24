using HotelBooking.Contexts;
using HotelBooking.Interfaces;
using HotelBooking.Models;
using HotelBooking.Repositories;
using HotelBooking.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #region contexts
            builder.Services.AddDbContext<HotelBookingDbContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection"))
                );
            #endregion

            #region repositories
            builder.Services.AddScoped<IRepository<int, User>, UserRepository>();
            builder.Services.AddScoped<IRepository<int, UserDetails>, UserDetailRepository>();
            #endregion

            #region services
            builder.Services.AddScoped<IUserLoginAndRegisterServices, UserLoginAndRegisterServices>();
            builder.Services.AddScoped<ITokenServices, TokenServices>();
            builder.Services.AddScoped<IUserServices, UserServices>();
            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
