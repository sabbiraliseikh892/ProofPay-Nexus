using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Storage
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(
            Stream fileStream,
            string fileName,
            CancellationToken cancellationToken = default);

        Task DeleteFileAsync(
            string storedFileName,
            CancellationToken cancellationToken = default);
        Task<Stream> OpenReadAsync(
    string storedFileName,
    CancellationToken cancellationToken = default);
    }
}
