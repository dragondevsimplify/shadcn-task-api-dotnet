using shadcn_taks_api.Filters;

namespace shadcn_taks_api.Extensions;

public static class ValidationExtension
{
    public static void WithRequestValidation<TRequest>(this RouteHandlerBuilder builder)
    {
        builder.AddEndpointFilter<ValidationFilter<TRequest>>().ProducesValidationProblem();
    }
}