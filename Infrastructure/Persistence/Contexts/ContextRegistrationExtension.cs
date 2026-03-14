using Backend.Infrastructure.Persistence.EFC.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Backend.Infrastructure.Persistence.Contexts;

public static class ContextRegistrationExtension
{
    public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            services.AddSingleton(_ =>
            {
                var conn = new SqliteConnection("DataSource=:memory:;Cache=Shared");
                conn.Open();
                return conn;
            });

            services.AddDbContext<CoursesOnlineDbContext>((sp, options) =>
            {
                var connection = sp.GetRequiredService<SqliteConnection>();
                options.UseSqlite(connection);
            });
        }
        else
        {
            services.AddDbContext<CoursesOnlineDbContext>(options =>
            {
                var dbConfig = configuration.GetConnectionString("CoursesOnlineDatabase")
                    ?? throw new InvalidOperationException("Connection string 'CoursesOnlineDatabase' not found.");

                options.UseSqlServer(dbConfig);
            });
        }

        return services;
    }
}
