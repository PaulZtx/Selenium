using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace WpfApp2
{
    [DataContract]
    public class VkNewsTwo
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string ImagePath { get; set; }

    }
}
