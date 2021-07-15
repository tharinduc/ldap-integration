namespace ldap
{
    public class LdapConfig
    {
        public string Url { get; set; }
        public string Port { get; set; }
        public string BindDn { get; set; }
        public string BindCredentials { get; set; }
        public string SearchBase { get; set; }
        public string SearchFilter { get; set; }
        public string AdminGroupDn { get; set; }
        public string UsersDn { get; set; }
        public string GroupsDn { get; set; }
    }
}