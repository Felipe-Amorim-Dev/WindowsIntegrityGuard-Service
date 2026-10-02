using System.Globalization;
using System.Text;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Service.Scanners;

public static class SfcResultParser
{
    public static IntegrityStatus Classify(string output, int exitCode)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return IntegrityStatus.Inconclusive;
        }

        string normalized = RemoveDiacritics(output).ToLowerInvariant();
        if (Contains(normalized, "could not perform the requested operation", "nao pode executar a operacao solicitada", "nao foi possivel executar a operacao solicitada", "another servicing or repair operation is currently running", "ha um reparo de sistema pendente"))
        {
            return IntegrityStatus.Inconclusive;
        }

        // A negative statement MUST be examined before a positive substring.
        bool healthy = Contains(normalized, "did not find any integrity violations", "nao encontrou nenhuma violacao de integridade", "nao encontrou violacoes de integridade", "nao encontrou nenhuma violacao", "nao encontrou arquivos corrompidos");
        if (healthy)
        {
            return exitCode == 0 ? IntegrityStatus.Healthy : IntegrityStatus.Inconclusive;
        }

        bool corrupted = Contains(normalized, "found integrity violations", "encontrou violacoes de integridade", "encontrou arquivos corrompidos");
        if (corrupted)
        {
            return IntegrityStatus.Corrupted;
        }

        return IntegrityStatus.Inconclusive;
    }

    private static bool Contains(string text, params string[] expressions) => expressions.Any(text.Contains);

    private static string RemoveDiacritics(string text)
    {
        string decomposed = text.Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(decomposed.Length);
        foreach (char character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                result.Append(character);
            }
        }
        return result.ToString().Normalize(NormalizationForm.FormC);
    }
}
