using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    [Serializable]
    public class VkNews
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
    }
}
