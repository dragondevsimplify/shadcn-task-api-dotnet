using FluentValidation;
using shadcn_taks_api.Features.Tags.Models;

namespace shadcn_taks_api.Features.Tags.Validators;

public class UpdateTagValidator : AbstractValidator<UpdateTagRequest>
{
    public UpdateTagValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.Name).NotEmpty();
    }
}