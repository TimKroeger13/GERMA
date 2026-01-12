public interface IUserIdvalidation
{
    bool IsValidUserId(string userid);
}

public class UserIdvalidation() : IUserIdvalidation
{
    public bool IsValidUserId(string userid)
    {
        if (string.IsNullOrWhiteSpace(userid))
            return false;

        if (userid.Length > 100)
            return false;

        // Allow only letters, numbers, underscores, and dashes
        return System.Text.RegularExpressions.Regex.IsMatch(userid, @"^[a-zA-Z0-9_-]+$");
    }
}