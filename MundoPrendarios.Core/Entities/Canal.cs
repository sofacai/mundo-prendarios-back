namespace MundoPrendarios.Core.Entities
{
    public class Canal
    {
        public int Id { get; set; }
        public string NombreFantasia { get; set; }
        public string RazonSocial { get; set; }
        public string Provincia { get; set; }
        public string Localidad { get; set; }
        public string Direccion { get; set; } // Nuevo campo
        public string Cuit { get; set; }
        public string CBU { get; set; }
        public string Alias { get; set; }
        public string Banco { get; set; }
        public string NumCuenta { get; set; }
        public string TipoCanal { get; set; } // Concesionario, Multimarca, Agencia, Habitualista, Freelance, Consumidor Final
        public bool Activo { get; set; } = true;
        public DateTime FechaAlta { get; set; } = DateTime.Now; // Nuevo campo
        public string OpcionesCobro { get; set; } // Nuevo campo (cheque, transferencia, efectivo)
        public string Foto { get; set; } // Nuevo campo

        // Datos del titular
        public string TitularNombreCompleto { get; set; } // Nuevo campo
        public string TitularTelefono { get; set; } // Nuevo campo
        public string TitularEmail { get; set; } // Nuevo campo

        // Relaciones
        public List<Subcanal> Subcanales { get; set; } = new List<Subcanal>();
        public List<PlanCanal> PlanesCanales { get; set; } = new List<PlanCanal>();
    }
}