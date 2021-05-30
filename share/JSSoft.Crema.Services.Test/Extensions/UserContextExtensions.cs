using JSSoft.Crema.Data;
using JSSoft.Crema.Data.Xml.Schema;
using JSSoft.Crema.ServiceModel;
using JSSoft.Crema.Services.Extensions;
using JSSoft.Crema.Services.Random;
using JSSoft.Crema.Services.Users.Serializations;
using JSSoft.Library;
using JSSoft.Library.Linq;
using JSSoft.Library.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Extensions
{
    static class UserContextExtensions
    {
#if SERVER
        public static void GenerateUserInfos(string repositoryPath, IObjectSerializer serializer)
        {
            var designedInfo = new SignatureDate(Authentication.SystemID, DateTime.UtcNow);
            var administrator = new UserSerializationInfo()
            {
                ID = Authentication.AdminID,
                Name = Authentication.AdminName,
                CategoryName = string.Empty,
                Authority = Authority.Admin,
                Password = Authentication.AdminID.Encrypt(),
                CreationInfo = designedInfo,
                ModificationInfo = designedInfo,
                BanInfo = (BanSerializationInfo)BanInfo.Empty,
            };

            var users = new List<UserSerializationInfo>
            {
                administrator
            };
            for (var i = 0; i < 10; i++)
            {
                var admin = new UserSerializationInfo()
                {
                    ID = "admin" + i,
                    Name = "관리자" + i,
                    CategoryName = "Administrators",
                    Authority = Authority.Admin,
                    Password = "admin".Encrypt(),
                    CreationInfo = designedInfo,
                    ModificationInfo = designedInfo,
                    BanInfo = (BanSerializationInfo)BanInfo.Empty,
                };

                var member = new UserSerializationInfo()
                {
                    ID = "member" + i,
                    Name = "구성원" + i,
                    CategoryName = "Members",
                    Authority = Authority.Member,
                    Password = "member".Encrypt(),
                    CreationInfo = designedInfo,
                    ModificationInfo = designedInfo,
                    BanInfo = (BanSerializationInfo)BanInfo.Empty,
                };

                var guest = new UserSerializationInfo()
                {
                    ID = "guest" + i,
                    Name = "손님" + i,
                    CategoryName = "Guests",
                    Authority = Authority.Guest,
                    Password = "guest".Encrypt(),
                    CreationInfo = designedInfo,
                    ModificationInfo = designedInfo,
                    BanInfo = (BanSerializationInfo)BanInfo.Empty,
                };

                users.Add(admin);
                users.Add(member);
                users.Add(guest);
            }

            var categoryList = new List<string>
            {
                "/Administrators/",
                "/Members/",
                "/Guests/",
                "/etc/"
            };
            for (var i = 0; i < 10; i++)
            {
                categoryList.Add($"/etc{RandomUtility.NextCategoryPath(1)}");
            }

            var serializationInfo = new UserContextSerializationInfo()
            {
                Version = CremaSchema.VersionValue,
                Categories = categoryList.ToArray(),
                Users = users.ToArray(),
            };

            serializationInfo.WriteToDirectory(repositoryPath, serializer);
        }
#endif
    }
}
