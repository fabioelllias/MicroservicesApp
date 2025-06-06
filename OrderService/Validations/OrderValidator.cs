using FluentValidation;
using Contracts;

public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty().MinimumLength(3).WithName("Nome do Produto");;
    }
}
