namespace Calibrator.WpfApplication.Infrastructure.Authentication;

public class UserIdentificationContext
{
    private User? _currentUser;

    public void SetUser(User user)
    {
        _currentUser = user;
    }

    public User? GetUser()
    {
        // For now, return a mock user
        return _currentUser ?? new User { FullName = "Test User", Email = "test@example.com" };
    }
}

public class User
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
