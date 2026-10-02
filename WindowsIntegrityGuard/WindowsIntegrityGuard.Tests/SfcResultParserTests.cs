using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Service.Scanners;

namespace WindowsIntegrityGuard.Tests;

public class SfcResultParserTests
{
    [Theory]
    [InlineData("Windows Resource Protection did not find any integrity violations.", 0, IntegrityStatus.Healthy)]
    [InlineData("A Proteção de Recursos do Windows não encontrou nenhuma violação de integridade.", 0, IntegrityStatus.Healthy)]
    [InlineData("A Proteção de Recursos do Windows não encontrou nenhuma violação de integridade.", 1, IntegrityStatus.Inconclusive)]
    [InlineData("Windows Resource Protection found integrity violations.", 1, IntegrityStatus.Corrupted)]
    [InlineData("A Proteção de Recursos do Windows encontrou violações de integridade.", 1, IntegrityStatus.Corrupted)]
    [InlineData("Windows Resource Protection could not perform the requested operation.", 1, IntegrityStatus.Inconclusive)]
    [InlineData("A Proteção de Recursos do Windows não pôde executar a operação solicitada.", 1, IntegrityStatus.Inconclusive)]
    [InlineData("", 0, IntegrityStatus.Inconclusive)]
    [InlineData("Mensagem desconhecida", 0, IntegrityStatus.Inconclusive)]
    public void Classify_ShouldBeConservative(string output, int exitCode, IntegrityStatus expected)
    {
        Assert.Equal(expected, SfcResultParser.Classify(output, exitCode));
    }
}
