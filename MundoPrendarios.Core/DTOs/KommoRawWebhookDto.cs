namespace MundoPrendarios.Core.DTOs
{
    public class KommoRawWebhookDto
    {
        public List<KommoLeadDto> Leads { get; set; }
    }

    public class KommoLeadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<KommoCustomFieldDto> CustomFieldsValues { get; set; }
        public KommoEmbeddedDto _embedded { get; set; }
    }

    public class KommoCustomFieldDto
    {
        public int FieldId { get; set; }
        public List<KommoFieldValueDto> Values { get; set; }
    }

    public class KommoFieldValueDto
    {
        public string Value { get; set; }
    }

    public class KommoEmbeddedDto
    {
        public List<KommoTagDto> Tags { get; set; }
    }

    public class KommoTagDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
