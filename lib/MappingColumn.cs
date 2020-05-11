using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LantanaGroup.XmlDocumentConverter
{
    public partial class MappingColumn
    {
        public bool ErrorShown { get; set; }
        public string GetHeading()
        {
            if (!string.IsNullOrEmpty(this.Heading))
                return this.Heading;

            return this.Name;
        }
    }
}
