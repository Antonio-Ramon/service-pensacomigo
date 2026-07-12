using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Domain.Repositories;
using PensaComigo.Persistence.Repositories;

namespace PensaComigo.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<PensaComigoDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("Default")));

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IComentarioRepository, ComentarioRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        return services;
    }
}
