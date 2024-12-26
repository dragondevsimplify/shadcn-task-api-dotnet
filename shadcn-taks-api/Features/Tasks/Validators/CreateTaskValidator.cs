using FluentValidation;
using shadcn_taks_api.Features.Tasks.Models;

namespace shadcn_taks_api.Features.Tasks.Validators;

public class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskValidator()
    {
        RuleFor(req => req.Name).NotEmpty();
        RuleFor(req => req.Title).NotEmpty();
        RuleFor(req => req.Status).NotEmpty();
        RuleFor(req => req.Priority).NotEmpty();
    }
}