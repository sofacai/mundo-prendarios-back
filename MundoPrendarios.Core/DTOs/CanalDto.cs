namespace MundoPrendarios.Core.DTOs
{
    public class CanalDto
    {
        public int Id { get; set; }
        public string NombreFantasia { get; set; }
        public string RazonSocial { get; set; }
        public string Provincia { get; set; }
        public string Localidad { get; set; }
        public string Cuit { get; set; }
        public string CBU { get; set; }
        public string Alias { get; set; }
        public string Banco { get; set; }
        public string NumCuenta { get; set; }
        public string TipoCanal { get; set; }
        public bool Activo { get; set; }
        public List<SubcanalSimpleDto> Subcanales { get; set; } = new List<SubcanalSimpleDto>();
        public List<PlanCanalDto> PlanesCanal { get; set; } = new List<PlanCanalDto>();
    }

    public class CanalCrearDto
    {
        public string NombreFantasia { get; set; }
        public string RazonSocial { get; set; }
        public string Provincia { get; set; }
        public string Localidad { get; set; }
        public string Cuit { get; set; }
        public string CBU { get; set; }
        public string Alias { get; set; }
        public string Banco { get; set; }
        public string NumCuenta { get; set; }
        public string TipoCanal { get; set; }
    }

    public class SubcanalSimpleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Provincia { get; set; }
        public string Localidad { get; set; }
        public bool Activo { get; set; }
        public List<GastoDto> Gastos { get; set; } = new List<GastoDto>();
    }
}