using Microsoft.Extensions.Configuration;
using ProofPayNexus.Application.Interfaces.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Infrastructure.Storage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _storagePath;

        public LocalFileStorageService(IConfiguration configuration)
        {
            var configuredPath = configuration["FileStorage:RootPath"];

            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                configuredPath = "App_Data/Evidence";
            }

            _storagePath = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, configuredPath));
        }

        public async Task<string> SaveFileAsync(
            Stream fileStream,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(fileStream);

            if (!fileStream.CanRead)
                throw new ArgumentException("File stream must be readable.");

            var extension = Path.GetExtension(fileName);

            var storedFileName =
                $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

            Directory.CreateDirectory(_storagePath);

            var fullPath = Path.Combine(_storagePath, storedFileName);

            await using var outputStream = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await fileStream.CopyToAsync(
                outputStream,
                cancellationToken);

            return storedFileName;
        }

        public Task DeleteFileAsync(
            string storedFileName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(storedFileName) ||
                Path.GetFileName(storedFileName) != storedFileName)
            {
                throw new ArgumentException("Invalid stored filename.");
            }

            var fullPath = Path.Combine(_storagePath, storedFileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }


        public Task<Stream> OpenReadAsync(
            string storedFileName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storedFileName) ||
                Path.GetFileName(storedFileName) != storedFileName)
            {
                throw new ArgumentException(
                    "Invalid stored file name.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var filePath = Path.Combine(_storagePath, storedFileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Evidence file was not found.",
                    filePath);
            }

            Stream stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            return Task.FromResult(stream);
        }

    }
}
