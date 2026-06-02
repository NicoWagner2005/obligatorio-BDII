namespace TicketingMundialUCU.Services;

public static class UserRoles
{
    public const string General = "general";
    public const string Funcionario = "funcionario";
    public const string Administrador = "administrador";

    public static readonly string[] All =
    [
        General,
        Funcionario,
        Administrador
    ];

    public static bool IsValid(string role)
    {
        return All.Contains(role, StringComparer.Ordinal);
    }
}
