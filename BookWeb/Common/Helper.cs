using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace BookWeb.Common
{
    public static class Helper
    {
        public static string CnnVal(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
    }
}
