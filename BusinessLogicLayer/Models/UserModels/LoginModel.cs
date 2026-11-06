namespace BusinessLogicLayer.Models.UserModels;

public class LoginModel
{
    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool InternalAuth { get; set; }
}
