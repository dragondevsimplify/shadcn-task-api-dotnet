using FluentValidation;
using shadcn_taks_api.Features.Tags.Models;

namespace shadcn_taks_api.Features.Tags.Validators;

public class CreateTagValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagValidator()
    {
        RuleFor(req => req.Name).NotEmpty();
    }
}