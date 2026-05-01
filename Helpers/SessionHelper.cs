namespace HotelMS.Helpers
{
    public static class SessionHelper
    {
        public static void SetUserID(ISession session, int id) =>
            session.SetString("UserID", id.ToString());

        public static int? GetUserID(ISession session)
        {
            var val = session.GetString("UserID");
            return val != null ? int.Parse(val) : null;
        }

        public static void SetUsername(ISession session, string name) =>
            session.SetString("Username", name);

        public static string? GetUsername(ISession session) =>
            session.GetString("Username");

        public static void SetUserRole(ISession session, string role) =>
            session.SetString("UserRole", role);

        public static string? GetUserRole(ISession session) =>
            session.GetString("UserRole");

        public static void ClearSession(ISession session) =>
            session.Clear();
    }
}
