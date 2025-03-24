using System;

namespace MundoPrendarios.Core.DTOs
{
    // DTO para mostrar la relación Canal-OficialComercial
    public class CanalOficialComercialDto
    {
        public int Id { get; set; }
        public int CanalId { get; set; }
        public string CanalNombre { get; set; }
        public int OficialComercialId { get; set; }
        public string OficialComercialNombre { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public bool Activo { get; set; }
    }

    // DTO para crear una relación Canal-OficialComercial
    public class CanalOficialComercialCrearDto
    {
        public int CanalId { get; set; }
        public int OficialComercialId { get; set; }
    }

    // DTO simplificado para lista de oficiales comerciales
    public class OficialComercialResumenDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido}";
        public DateTime FechaAsignacion { get; set; }
    }
}