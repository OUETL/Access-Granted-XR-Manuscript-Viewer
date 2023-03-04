namespace OU.OVAL.Core
{
    public class UserInfo
    {
        public string name = "Unnamed";
        public bool audible = false;
    }

    public class UserInfoMap : SerializableDictionary<System.Guid, UserInfo>
    {
    }
}