using System.Numerics;
namespace MundoPrendarios.Core.Entities
{
    public class Canal
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
        public string TipoCanal { get; set; } // Concesionario, Multimarca, Agencia, Habitualista, Freelance, Consumidor Final
        public bool Activo { get; set; } = true;

        // Relaciones
        public List<Subcanal> Subcanales { get; set; } = new List<Subcanal>();

        // Cambiar esta relación
        // public List<Plan> Planes { get; set; } = new List<Plan>();

        // Nueva relación con PlanCanal
        public List<PlanCanal> PlanesCanales { get; set; } = new List<PlanCanal>();
    }
}