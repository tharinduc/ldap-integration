using System.Collections.Generic;

namespace ldap
{
    public class AppGroup
    {
        public string Name { get; set; }
        public List<string> Users { get; set; }
    }
}