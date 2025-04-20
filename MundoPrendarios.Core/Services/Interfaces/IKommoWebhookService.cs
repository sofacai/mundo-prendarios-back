using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MundoPrendarios.Core.Services.Interfaces
{
    public interface IKommoWebhookService
    {
        Task<object> ProcesarDesdeFormAsync(IFormCollection form);
    }
}
