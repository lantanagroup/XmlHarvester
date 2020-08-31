namespace LantanaGroup.XmlHarvester
{
    public partial class MappingColumn
    {
        public bool ErrorShown { get; set; }
        public string GetHeading()
        {
            if (!string.IsNullOrEmpty(Heading))
                return Heading;

            return Name;
        }
    }
}
