namespace TwittorApp.Dtos
{
    public record InputUpdatePasswordUser
    (
        int? Id,
        string oldPassword,
        string newPassword
    );
}
