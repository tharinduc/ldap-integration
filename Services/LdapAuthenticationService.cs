using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;

namespace ldap
{
    public interface IAuthenticationService
    {
        AppUser Login(string username, string password);
        List<AppUser> Users();
        List<AppGroup> Groups();
    }

    class LdapAuthenticationService : IAuthenticationService
    {
        private const string CnAttribute = "cn";
        private const string MemberOfAttribute = "memberOf";
        private const string EmailAttribute = "mail";
        private const string MemberAttribute = "member";
        private readonly LdapConfig _config;
        private readonly LdapConnection _connection;

        public LdapAuthenticationService(IOptions<LdapConfig> config)
        {
            _config = config.Value;
            _connection = new LdapConnection
            {
                SecureSocketLayer = false
            };
        }

        public AppUser Login(string username, string password)
        {
            _connection.Connect(_config.Url, int.Parse(_config.Port));
            _connection.Bind(_config.BindDn, _config.BindCredentials);

            var searchFilter = string.Format(_config.SearchFilter, username);
            var result = _connection.Search(
                _config.SearchBase,
                LdapConnection.ScopeSub,
                searchFilter,
                new string[] { CnAttribute, MemberOfAttribute, EmailAttribute },
                false
            );

            try
            {
                var user = result.Next();
                if (user != null)
                {
                    _connection.Bind(user.Dn, password);
                    if (_connection.Bound)
                    {
                        return new AppUser
                        {
                            Username = user.GetAttribute(CnAttribute).StringValue,
                            Email = user.GetAttribute(EmailAttribute).StringValue,
                            IsAdmin = user.GetAttribute(MemberOfAttribute).StringValue.Contains((_config.AdminGroupDn))
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            _connection.Disconnect();
            return null;
        }

        public List<AppUser> Users()
        {
            _connection.Connect(_config.Url, int.Parse(_config.Port));
            _connection.Bind(_config.BindDn, _config.BindCredentials);

            var result = _connection.Search(
                _config.UsersDn,
                LdapConnection.ScopeSub,
                "(cn=*)",
                new string[] { CnAttribute, MemberOfAttribute, EmailAttribute },
                false
            );

            try
            {
                LdapEntry user;
                List<AppUser> users = new List<AppUser>();
                while (result.HasMore())
                {
                    user = result.Next();
                    var isMemberOf = user.GetAttributeSet().ContainsKey(MemberOfAttribute);
                    users.Add(
                        new AppUser
                        {
                            Username = user.GetAttribute(CnAttribute).StringValue,
                            Email = user.GetAttribute(EmailAttribute).StringValue,
                            IsAdmin = isMemberOf ? user.GetAttribute(MemberOfAttribute).StringValue.Contains((_config.AdminGroupDn)) : false
                        }
                    );
                }
                return users;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _connection.Disconnect();
            }
        }

        public List<AppGroup> Groups()
        {
            _connection.Connect(_config.Url, int.Parse(_config.Port));
            _connection.Bind(_config.BindDn, _config.BindCredentials);

            var result = _connection.Search(
                _config.GroupsDn,
                LdapConnection.ScopeSub,
                "(cn=*)",
                new string[] { CnAttribute, MemberAttribute },
                false
            );

            try
            {
                LdapEntry group;
                List<AppGroup> groups = new List<AppGroup>();
                while (result.HasMore())
                {
                    group = result.Next();
                    var members = new List<string>();
                    var isMember = group.GetAttributeSet().ContainsKey(MemberAttribute);
                    if (isMember)
                    {
                        var groupEnumerator = group.GetAttribute(MemberAttribute).StringValues;
                        while (groupEnumerator.MoveNext())
                        {
                            members.Add(groupEnumerator.Current);
                        }
                    }
                    groups.Add(
                        new AppGroup
                        {
                            Name = group.GetAttribute(CnAttribute).StringValue,
                            Users = members
                        }
                    );
                }
                return groups;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _connection.Disconnect();
            }
        }
    }
}