namespace MundoPrendarios.Core.DTOs
{
    public class CanalDto
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
        public string TipoCanal { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; } // Nuevo campo
        public string OpcionesCobro { get; set; } // Nuevo campo
        public string Foto { get; set; } // Nuevo campo

        // Datos del titular
        public string TitularNombreCompleto { get; set; } // Nuevo campo
        public string TitularTelefono { get; set; } // Nuevo campo
        public string TitularEmail { get; set; } // Nuevo campo

        public List<OficialComercialResumenDto> OficialesComerciales { get; set; } = new List<OficialComercialResumenDto>();
        public List<SubcanalSimpleDto> Subcanales { get; set; } = new List<SubcanalSimpleDto>();
        public List<PlanCanalDto> PlanesCanal { get; set; } = new List<PlanCanalDto>();

    }

    public class CanalCrearDto
    {
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
        public string TipoCanal { get; set; }
        public string OpcionesCobro { get; set; } // Nuevo campo
        public string Foto { get; set; } // Nuevo campo

        // Datos del titular
        public string TitularNombreCompleto { get; set; } // Nuevo campo
        public string TitularTelefono { get; set; } // Nuevo campo
        public string TitularEmail { get; set; } // Nuevo campo

        public List<int> OficialesComerciales { get; set; } = new List<int>();

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