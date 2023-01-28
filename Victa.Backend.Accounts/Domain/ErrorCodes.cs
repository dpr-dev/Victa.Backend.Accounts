namespace Victa.Backend.Accounts.Domain;


public static class ErrorCodes
{
    public const string Unhandled = "accounts-00001";

    public const string UserWasNotFoundById = "accounts-00010";
    public const string UserWasNotFoundByEmail = "accounts-00011";
    public const string UserWasNotFoundByLogin = "accounts-00012";

    public const string PasswordVerificationFailed = "accounts-00020";
}
