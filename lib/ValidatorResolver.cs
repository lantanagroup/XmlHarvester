using Saxon.Api;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LantanaGroup.XmlDocumentConverter
{
    public class ValidatorResolver : XmlResolver
    {
        private readonly string rootPath;

        public ValidatorResolver(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            Uri baseUri = new Uri("file://" + this.rootPath);

            if (absoluteUri.ToString().EndsWith("skeleton1-5.xsl"))
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("LantanaGroup.XmlDocumentConverter.iso-sch-skeleton1-5.xsl");
            }
            else if (absoluteUri.ToString().StartsWith("file:///"))
            {
                string filePath = absoluteUri.ToString().Substring(8);
                string actualPath = System.IO.Path.Combine(this.rootPath, filePath);

                if (new FileInfo(actualPath).Exists)
                    return new System.IO.FileStream(actualPath, FileMode.Open);
            }

            throw new NotImplementedException();
        }
    }
}
