#if SERVER
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JSSoft.Crema.Services.Test.Extensions
{
    static class UserContextExtensions
    {
        public static UserContextSerializationInfo GenerateUserInfos(int userCount, int categoryCount)
        {
            var signatureDate = new SignatureDate(Authentication.SystemID, DateTime.UtcNow);
            var administrator = new UserSerializationInfo()
            {
                ID = Authentication.AdminID,
                Name = Authentication.AdminName,
                CategoryName = string.Empty,
                Authority = Authority.Admin,
                Password = Authentication.AdminID.Encrypt(),
                CreationInfo = signatureDate,
                ModificationInfo = signatureDate,
                BanInfo = (BanSerializationInfo)BanInfo.Empty,
            };
            var categoryPaths = RandomUtility.NextCategoryPaths(3, categoryCount);
            var usersByID = new Dictionary<string, UserSerializationInfo>() { { administrator.ID, administrator } };
            var authorities = new Authority[] { Authority.Guest, Authority.Member, Authority.Admin };
            var userNameByAuthority = new Dictionary<Authority, string>
            {
                { Authority.Guest, "손님" },
                { Authority.Member, "구성원" },
                { Authority.Admin, "관리자" },
            };
            for (var i = 0; i < userCount; i++)
            {
                var authority = authorities.Random();
                var name = authority.ToString().ToLower();
                var userID = NameUtility.GenerateNewName(name, usersByID.Keys);
                var password = name.Encrypt();
                var userName = Regex.Replace(userID, $"{name}(\\d+)", $"{userNameByAuthority[authority]}$1");
                var categoryPath = categoryPaths.Skip((int)(categoryCount * 0.75)).Random();
                var isBanned = authority != Authority.Admin && RandomUtility.Within(10);
                var banInfo = isBanned == true ? new BanInfo()
                {
                    Path = categoryPath + userID,
                    SignatureDate = signatureDate,
                    Comment = RandomUtility.NextString(),
                } : BanInfo.Empty;
                var info = new UserSerializationInfo()
                {
                    ID = userID,
                    Name = userName,
                    CategoryPath = categoryPath,
                    Authority = authority,
                    Password = password,
                    CreationInfo = signatureDate,
                    ModificationInfo = signatureDate,
                    BanInfo = (BanSerializationInfo)banInfo,
                };
                usersByID.Add(userID, info);
            }

            var serializationInfo = new UserContextSerializationInfo()
            {
                Version = CremaSchema.VersionValue,
                Categories = categoryPaths.ToArray(),
                Users = usersByID.Values.ToArray(),
            };

            return serializationInfo;
        }

        // private static UserSerializationInfo CreateUser(int index, Authority authority, SignatureDate signatureDate)
        // {
        //     switch (authority)
        //     {
        //         case Authority.Guest:
        //             return CreateGuest(index, signatureDate);
        //         case Authority.Member:
        //             return CreateMember(index, signatureDate);
        //         case Authority.Admin:
        //             return CreateAdmin(index, signatureDate);
        //     }
        //     throw new NotImplementedException();
        // }

        // private static UserSerializationInfo CreateAdmin(int index, SignatureDate signatureDate)
        // {
        //     return new UserSerializationInfo()
        //     {
        //         ID = "admin" + index,
        //         Name = "관리자" + index,
        //         CategoryName = "Administrators",
        //         Authority = Authority.Admin,
        //         Password = "admin".Encrypt(),
        //         CreationInfo = signatureDate,
        //         ModificationInfo = signatureDate,
        //         BanInfo = (BanSerializationInfo)BanInfo.Empty,
        //     };
        // }

        // private static UserSerializationInfo CreateMember(int index, SignatureDate signatureDate)
        // {
        //     return new UserSerializationInfo()
        //     {
        //         ID = "member" + index,
        //         Name = "구성원" + index,
        //         CategoryName = "Members",
        //         Authority = Authority.Member,
        //         Password = "member".Encrypt(),
        //         CreationInfo = signatureDate,
        //         ModificationInfo = signatureDate,
        //         BanInfo = (BanSerializationInfo)BanInfo.Empty,
        //     };
        // }

        // private static UserSerializationInfo CreateGuest(int index, SignatureDate signatureDate)
        // {
        //     return new UserSerializationInfo()
        //     {
        //         ID = "guest" + index,
        //         Name = "손님" + index,
        //         CategoryName = "Guests",
        //         Authority = Authority.Guest,
        //         Password = "guest".Encrypt(),
        //         CreationInfo = signatureDate,
        //         ModificationInfo = signatureDate,
        //         BanInfo = (BanSerializationInfo)BanInfo.Empty,
        //     };
        // }
    }
}
#endif
