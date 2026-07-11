using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Domain.Repositories;
using PensaComigo.Persistence.Repositories;

namespace PensaComigo.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        // ponytail: registro do PensaComigoDbContext (AddDbContext + connection string
        // Supabase) entra na Fatia 4. Aqui só os repositórios.
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IComentarioRepository, ComentarioRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        return services;
    }
}
