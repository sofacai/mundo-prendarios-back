namespace MundoPrendarios.Core.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cuil { get; set; }
        public string Dni { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Provincia { get; set; }
        public int CanalId { get; set; }

        // Relaciones
        public Canal Canal { get; set; }
        public List<Operacion> Operaciones { get; set; } = new List<Operacion>();
    }
}