public class KommoRawWebhookDto
{
    public List<KommoLeadRawDto> LeadsUpdate { get; set; } = new();
}

public class KommoLeadRawDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<KommoCustomFieldDto> CustomFields { get; set; } = new();
    public List<KommoTagDto> Tags { get; set; } = new();
}

public class KommoCustomFieldDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<KommoFieldValueDto> Values { get; set; } = new();
}

public class KommoFieldValueDto
{
    public string Value { get; set; }
    public int? Enum { get; set; } // por si llega
}

public class KommoTagDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}
