namespace Victa.Backend.Accounts.Core.Errors;

public class ExecutionError
{
    public string? Code { get; set; }
    public string? Details { get; set; }


    public static ExecutionError PrincipalNotFound { get; } = new ExecutionError { };
}
