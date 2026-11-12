using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Services
{
    public interface IPayPalAuthService
    {
        Task<string> GetAccessTokenAsync(
            CancellationToken cancellationToken = default);
    }
}
