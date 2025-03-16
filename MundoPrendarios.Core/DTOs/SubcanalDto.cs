namespace MundoPrendarios.Core.DTOs
{
    public class SubcanalDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Provincia { get; set; }
        public string Localidad { get; set; }
        public int CanalId { get; set; }
        public string CanalNombre { get; set; }
        public int AdminCanalId { get; set; }
        public string AdminCanalNombre { get; set; }
        public bool Activo { get; set; }
        public List<GastoDto> Gastos { get; set; } = new List<GastoDto>();
        public List<UsuarioDto> Vendors { get; set; } = new List<UsuarioDto>();
    }

    public class SubcanalCrearDto
    {
        public string Nombre { get; set; }
        public string Provincia { get; set; }
        public string Localidad { get; set; }
        public int CanalId { get; set; }
        public int AdminCanalId { get; set; }
    }

    public class GastoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class GastoCrearDto
    {
        public string Nombre { get; set; }
        public decimal Porcentaje { get; set; }
        public int SubcanalId { get; set; }
    }
}
