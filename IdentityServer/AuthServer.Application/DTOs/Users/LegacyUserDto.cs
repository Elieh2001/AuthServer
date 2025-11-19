namespace AuthServer.Application.DTOs.Users;


public class LegacyUserDto
{
    public string LegacyUserId { get; set; } = string.Empty;
    public string Email { get; set; }= string.Empty;
    public string FirstName { get; set; }= string.Empty;
    public string LastName { get; set; }= string.Empty;
    public Dictionary<string, object> AdditionalData { get; set; }

    public LegacyUserDto()
    {
        AdditionalData = new Dictionary<string, object>();
    }

}
