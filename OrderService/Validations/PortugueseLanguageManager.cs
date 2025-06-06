using FluentValidation.Resources;

public class PortugueseLanguageManager : LanguageManager
{
    public PortugueseLanguageManager()
    {
        Culture = new System.Globalization.CultureInfo("pt-BR");

        AddTranslation("pt-BR", "NotEmptyValidator", "'{PropertyName}' é obrigatório.");
        AddTranslation("pt-BR", "MinimumLengthValidator", "'{PropertyName}' deve conter pelo menos {MinLength} caracteres.");
        AddTranslation("pt-BR", "MaximumLengthValidator", "'{PropertyName}' deve conter no máximo {MaxLength} caracteres.");
        AddTranslation("pt-BR", "EmailValidator", "'{PropertyName}' não é um endereço de e-mail válido.");
        AddTranslation("pt-BR", "GreaterThanValidator", "'{PropertyName}' deve ser maior que {ComparisonValue}.");
        AddTranslation("pt-BR", "LessThanValidator", "'{PropertyName}' deve ser menor que {ComparisonValue}.");
        AddTranslation("pt-BR", "EqualValidator", "'{PropertyName}' deve ser igual a {ComparisonValue}.");
        AddTranslation("pt-BR", "NotEqualValidator", "'{PropertyName}' não deve ser igual a {ComparisonValue}.");
        AddTranslation("pt-BR", "CreditCardValidator", "'{PropertyName}' não é um número de cartão de crédito válido.");
        AddTranslation("pt-BR", "UrlValidator", "'{PropertyName}' não é uma URL válida.");
        AddTranslation("pt-BR", "RegularExpressionValidator", "'{PropertyName}' não corresponde ao padrão exigido.");
        AddTranslation("pt-BR", "InclusiveBetweenValidator", "'{PropertyName}' deve estar entre {From} e {To}.");
        AddTranslation("pt-BR", "ExclusiveBetweenValidator", "'{PropertyName}' deve estar entre {From} e {To}, mas não incluir os limites.");
        AddTranslation("pt-BR", "LengthValidator", "'{PropertyName}' deve ter entre {MinLength} e {MaxLength} caracteres.");
        AddTranslation("pt-BR", "ScalePrecisionValidator", "'{PropertyName}' deve ter no máximo {Precision} dígitos, com {Scale} dígitos após o ponto decimal.");
        AddTranslation("pt-BR", "MustAsyncValidator", "'{PropertyName}' deve atender à condição assíncrona especificada.");
        AddTranslation("pt-BR", "MustValidator", "'{PropertyName}' deve atender à condição especificada.");
        AddTranslation("pt-BR", "NotNullValidator", "'{PropertyName}' não pode ser nulo.");
        AddTranslation("pt-BR", "NullValidator", "'{PropertyName}' deve ser nulo.");
        AddTranslation("pt-BR", "EnumValidator", "'{PropertyName}' deve ser um valor válido do tipo enum.");
        AddTranslation("pt-BR", "CreditCardValidator", "'{PropertyName}' não é um número de cartão de crédito válido.");
        AddTranslation("pt-BR", "DateValidator", "'{PropertyName}' deve ser uma data válida.");
        AddTranslation("pt-BR", "DateTimeValidator", "'{PropertyName}' deve ser uma data e hora válidas.");
        AddTranslation("pt-BR", "TimeSpanValidator", "'{PropertyName}' deve ser um intervalo de tempo válido.");
        AddTranslation("pt-BR", "AsyncValidator", "'{PropertyName}' deve atender à condição assíncrona especificada.");
        AddTranslation("pt-BR", "RegularExpressionValidator", "'{PropertyName}' não corresponde ao padrão exigido.");
        AddTranslation("pt-BR", "CreditCardValidator", "'{PropertyName}' não é um número de cartão de crédito válido.");
        AddTranslation("pt-BR", "EmailValidator", "'{PropertyName}' não é um endereço de e-mail válido.");
        AddTranslation("pt-BR", "UrlValidator", "'{PropertyName}' não é uma URL válida.");
        AddTranslation("pt-BR", "GreaterThanOrEqualValidator", "'{PropertyName}' deve ser maior ou igual a {ComparisonValue}.");
        AddTranslation("pt-BR", "LessThanOrEqualValidator", "'{PropertyName}' deve ser menor ou igual a {ComparisonValue}.");
        AddTranslation("pt-BR", "NotEmptyValidator", "'{PropertyName}' não pode estar vazio.");
        
        // ... você pode adicionar outras traduções se quiser
    }
}
