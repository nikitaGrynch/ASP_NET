namespace ASP_NET.Models.User;

public class ProfileModel
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public string RealName { get; set; }
    public string Email { get; set; }
    public string? Avatar { get; set; }
    public DateTime RegisterDt { get; set; }
    public DateTime? LastEnterDt { get; set; }

    public Boolean IsEmailPublic { get; set; }
    public Boolean IsRealNamePublic { get; set; }
    public Boolean IsDatesPublic { get; set; }
    /// <summary>
    /// Does the profile belongs to authorized user?
    /// </summary>
    public Boolean IsPersonal { get; set; } = false;

    public ProfileModel(Data.Entity.User user)
    {
        // object mapping - отображение одного объекта на другом
        var userProps = user.GetType().GetProperties(); // свойства, описанные в типе объекта User
        var thisProps = this.GetType().GetProperties();
        foreach (var thisProp in thisProps)
        {
            var prop = userProps.FirstOrDefault(userProp =>
                userProp.Name == thisProp.Name
                && userProp.PropertyType.IsAssignableTo(thisProp.PropertyType));
            if (prop is not null)
            {
                thisProp.SetValue(this, prop.GetValue(user));
            }
        }
    }

    public ProfileModel()
    {
        Login = RealName = Email = null!;
    }
}