using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Application.Behaviors;
using PensaComigo.Application.Comentarios;
using PensaComigo.Application.Messaging;

namespace PensaComigo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(DespachoDeEventosBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        // Rate limit de comentários (Fatia 20): cache em memória do processo.
        // Singleton porque o balde precisa sobreviver entre requisições — Scoped
        // nasceria vazio a cada chamada e o limite nunca bateria.
        services.AddMemoryCache();
        services.AddSingleton<LimitadorDeComentarios>();

        // Buffer de eventos da requisição (Fatia 23). Scoped: uma fila por requisição.
        services.AddScoped<FilaDeEventos>();

        return services;
    }
}
