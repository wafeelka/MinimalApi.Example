public class UserRepository : IUserRepository
{
    private List<UserDto> _users =>  new() 
    {
        new UserDto("Sara", "husband"),
        new UserDto("John", "waifu"),
        new UserDto("Gertruda", "naruto"),
    };
    public UserDto GetUser(UserModel userModel) =>
        _users.FirstOrDefault(u => string.Equals(u.Password, userModel.Password)
        && string.Equals(u.UserName, userModel.UserName)) ?? throw new Exception("invalid credentials");
}
