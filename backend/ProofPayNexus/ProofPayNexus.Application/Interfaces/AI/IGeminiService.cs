using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.AI
{
    public interface IGeminiService
    {
        Task<string> AnalyzeDocumentAsync(
        Stream documentStream,
        string mimeType,
        CancellationToken cancellationToken = default);
        Task<string> GenerateTextAsync(
    string prompt,
    CancellationToken cancellationToken = default);
    }
}
