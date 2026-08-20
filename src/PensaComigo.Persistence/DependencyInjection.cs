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

        // O DbContext É a unidade de trabalho; expõe a mesma instância como IUnitOfWork.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PensaComigoDbContext>());

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IComentarioRepository, ComentarioRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<IEtapaRepository, EtapaRepository>();

        return services;
    }
}
