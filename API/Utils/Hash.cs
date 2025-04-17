using Microsoft.AspNetCore.Identity;

public class Hash
{
    public static string HashPassword(string password)
    {
        var passwordHasher = new PasswordHasher<object>();
        return passwordHasher.HashPassword(null, password);
    }
}