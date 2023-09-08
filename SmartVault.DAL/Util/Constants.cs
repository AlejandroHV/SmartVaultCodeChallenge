namespace SmartVault.DataGeneration.Util
{
    public static class Constants
    {
        public static string INSERT_USER_QUERY = "INSERT INTO User (Id, FirstName, LastName, DateOfBirth, AccountId, Username, Password {0}) VALUES (@Id, @FirstName, @LastName, @DateOfBirth, @AccountId, @Username, @Password {1})";
        public static string INSERT_ACCOUNT_QUERY = "INSERT INTO Account (Id, Name {0}) VALUES (@Id, @Name {1})";
        public static string INSERT_DOCUMENT_QUERY = "INSERT INTO Document ( Name, FilePath, Length, AccountId {0}) VALUES ( @Name, @FilePath, @Length, @AccountId {1})";


        public static string TABLE_NAME_USER = "User";
        public static string TABLE_NAME_ACCOUNT = "Account";
        public static string TABLE_NAME_DOCUMENT = "Document";
        public static string TABLE_NAME_OAUTH = "OAuthUser";
    }
}
