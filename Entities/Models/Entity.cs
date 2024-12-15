using Entities.LinkModels;
using System.Xml;

namespace Entities.Models
{
    public class Entity
    {
        public readonly IDictionary<string, object> Entitys;

        //public Dictionary<string, object> Entitys { get; set; }

        //public Entity()
        //{
        //    Entitys = new Dictionary<string, object>();
        //}

        //public void Add(string key, object value)
        //{
        //    Entitys[key] = value;
        //}

        private void WriteLinksToXml(string key, object value, XmlWriter writer)
        {
            writer.WriteStartElement(key);
            if (value.GetType() == typeof(List<Link>))
            {
                foreach (var val in value as List<Link>)
                {
                    writer.WriteStartElement(nameof(Link));
                    WriteLinksToXml(nameof(val.Href), val.Href, writer);
                    WriteLinksToXml(nameof(val.Method), val.Method, writer);
                    WriteLinksToXml(nameof(val.Rel), val.Rel, writer);
                    writer.WriteEndElement();
                }
            }
            else
            {
                writer.WriteString(value.ToString());
            }
            writer.WriteEndElement();
        }
    }
}
