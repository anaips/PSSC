
using ProiectPSSC.Data;
using ProiectPSSC.Data.Repositories;
using ProiectPSSC.Domain.Repositories;
using ProiectPSSC.Domain.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;


namespace ProiectPSSC {
    public class Program {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddDbContext<ComandaContext>
                (options => { options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                    
                });
            
            builder.Services.AddTransient<IComandaRepository, ComandaRepositor>();
            builder.Services.AddTransient<IUtilizatoRepository, UtilizatoRepository>();
            builder.Services.AddTransient<IProdusRepository, ProdusRepository>();
            builder.Services.AddTransient<PublishComandaWorkflow>();
            builder.Services.AddTransient<LivrareWorkflow>();
            builder.Services.AddTransient<FacturareWorkflowv2>();

            builder.Services.AddHttpClient();

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Example.Api", Version = "v1" });
            });


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