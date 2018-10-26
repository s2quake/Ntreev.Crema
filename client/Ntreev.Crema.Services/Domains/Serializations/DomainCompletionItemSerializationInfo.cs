using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Domains.Serializations
{
    [DataContract(Namespace = SchemaUtility.Namespace)]
    struct DomainCompletionItemSerializationInfo
    {
        public DomainCompletionItemSerializationInfo(long id, string userID, DateTime dateTime, Type type)
        {
            this.ID = id;
            this.UserID = userID;
            this.DateTime = dateTime;
            this.Type = type.AssemblyQualifiedName;
        }

        public override string ToString()
        {
            return $"{this.ID}\t{this.UserID}\t{this.DateTime:o}\t{this.Type}";
        }

        public static DomainCompletionItemSerializationInfo Parse(string text)
        {
            var items = StringUtility.Split(text, '\t');
            return new DomainCompletionItemSerializationInfo()
            {
                ID = long.Parse(items[0]),
                UserID = items[1],
                DateTime = DateTime.Parse(items[1]),
                Type = items[2],
            };
        }

        [DataMember]
        public long ID { get; set; }

        [DataMember]
        public string UserID { get; set; }

        [DataMember]
        public DateTime DateTime { get; set; }

        [DataMember]
        public string Type { get; set; }
    }
}
