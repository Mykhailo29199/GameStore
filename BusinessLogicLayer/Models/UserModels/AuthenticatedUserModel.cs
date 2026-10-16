namespace BusinessLogicLayer.Models.UserModels;

public class AuthenticatedUserModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<string> Permissions { get; set; } = [];
}
