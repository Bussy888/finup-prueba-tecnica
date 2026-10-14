using System.Net.Mail;
using System.Text.RegularExpressions;
using Claims.Application.Dtos;
using Claims.Domain;
using Claims.Domain.Exceptions;

namespace Claims.Application.Validation;

public static partial class ClaimValidator
{
    [GeneratedRegex(@"^\d{7,15}$")]
    private static partial Regex PhoneRegex();

    public static void Validate(CreateClaimRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        var name = request.FullName?.Trim() ?? "";
        if (name.Length < 3 || name.Length > 100)
            errors["fullName"] = ["El nombre debe tener entre 3 y 100 caracteres."];

        var document = request.DocumentId?.Trim() ?? "";
        if (document.Length == 0)
            errors["documentId"] = ["El documento de identidad (CI) es obligatorio."];
        else if (document.Length > 30)
            errors["documentId"] = ["El documento de identidad no puede superar los 30 caracteres."];

        if (!IsValidEmail(request.Email?.Trim()))
            errors["email"] = ["El correo electrónico no es válido."];

        var phone = request.Phone?.Trim();
        if (!string.IsNullOrEmpty(phone) && !PhoneRegex().IsMatch(phone))
            errors["phone"] = ["El teléfono debe tener entre 7 y 15 dígitos."];

        var description = request.Description?.Trim() ?? "";
        if (description.Length < 20 || description.Length > 2000)
            errors["description"] = ["La descripción debe tener entre 20 y 2000 caracteres."];

        if (errors.Count > 0)
            throw new ValidationFailedException(errors);
    }

    public static void Validate(ChangeStatusRequest request)
    {
        var comment = request.Comment?.Trim();

        if (ClaimStatusRules.RequiresComment(request.Status) && string.IsNullOrEmpty(comment))
            throw Fail("comment", "Para resolver o rechazar hay que escribir un comentario.");

        if (comment?.Length > 1000)
            throw Fail("comment", "El comentario no puede superar los 1000 caracteres.");
    }

    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 254) return false;
        if (!MailAddress.TryCreate(email, out var parsed) || parsed.Address != email) return false;
        return parsed.Host.Contains('.') && !parsed.Host.EndsWith('.');
    }

    private static ValidationFailedException Fail(string field, string message) =>
        new(new Dictionary<string, string[]> { [field] = [message] });
}
