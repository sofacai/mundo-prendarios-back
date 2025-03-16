using MundoPrendarios.Core.Entities;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(Usuario usuario, string rolNombre);
    }
}