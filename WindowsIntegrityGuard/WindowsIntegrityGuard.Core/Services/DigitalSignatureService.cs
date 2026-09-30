using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Core.Services
{
    public sealed class DigitalSignatureService : IDigitalSignatureService
    {
        private static readonly Guid VerifyAction = new("00AAC56B-CD44-11D0-8CC2-00C04FC295EE");

        private const uint ERROR_SUCCESS = 0x00000000;
        private const uint TRUST_E_NOSIGNATURE = 0x800B0100;
        private const uint TRUST_E_SUBJECT_NOT_TRUSTED = 0x800B0004;
        private const uint TRUST_E_BAD_DIGEST = 0x80096010;
        private const uint CERT_E_UNTRUSTEDROOT = 0x800B0109;
        private const uint CERT_E_REVOKED = 0x800B010C;

        public Task<DigitalSignatureResult> VerifyAsync(string filePath, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
            cancellationToken.ThrowIfCancellationRequested();

            DateTimeOffset verifiedAt = DateTimeOffset.UtcNow;

            if (!File.Exists(filePath))
            {
                return Task.FromResult(CreateResult(filePath, DigitalSignatureStatus.Failed, "Arquivo não encontrado.", null, verifiedAt));
            }

            IntPtr fileInfoPointer = IntPtr.Zero;

            try
            {
                var fileInfo = new WINTRUST_FILE_INFO
                {
                    cbStruct = (uint)Marshal.SizeOf<WINTRUST_FILE_INFO>(),
                    pcwszFilePath = Path.GetFullPath(filePath)
                };

                fileInfoPointer = Marshal.AllocHGlobal(Marshal.SizeOf<WINTRUST_FILE_INFO>());
                Marshal.StructureToPtr(fileInfo, fileInfoPointer, false);

                var trustData = new WINTRUST_DATA
                {
                    cbStruct = (uint)Marshal.SizeOf<WINTRUST_DATA>(),
                    dwUIChoice = 2,
                    dwRevocationChecks = 0,
                    dwUnionChoice = 1,
                    pFile = fileInfoPointer,
                    dwStateAction = 0,
                    dwProvFlags = 0,
                    dwUIContext = 0
                };

                Guid action = VerifyAction;
                uint errorCode = WinVerifyTrust(IntPtr.Zero, ref action, ref trustData);

                cancellationToken.ThrowIfCancellationRequested();

                DigitalSignatureStatus status = GetStatus(errorCode);

                return Task.FromResult(CreateResult(filePath, status, GetMessage(status, errorCode), errorCode, verifiedAt));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or ExternalException or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException)
            {
                return Task.FromResult(CreateResult(filePath, DigitalSignatureStatus.Failed, ex.Message, null, verifiedAt));
            }
            finally
            {
                if (fileInfoPointer != IntPtr.Zero)
                {
                    Marshal.DestroyStructure<WINTRUST_FILE_INFO>(fileInfoPointer);
                    Marshal.FreeHGlobal(fileInfoPointer);
                }
            }
        }

        private static DigitalSignatureStatus GetStatus(uint errorCode)
        {
            return errorCode switch
            {
                ERROR_SUCCESS => DigitalSignatureStatus.Trusted,
                TRUST_E_NOSIGNATURE => DigitalSignatureStatus.Inconclusive,
                TRUST_E_SUBJECT_NOT_TRUSTED => DigitalSignatureStatus.Untrusted,
                TRUST_E_BAD_DIGEST => DigitalSignatureStatus.Untrusted,
                CERT_E_UNTRUSTEDROOT => DigitalSignatureStatus.Untrusted,
                CERT_E_REVOKED => DigitalSignatureStatus.Untrusted,
                _ => DigitalSignatureStatus.Inconclusive
            };
        }

        private static string GetMessage(DigitalSignatureStatus status, uint errorCode)
        {
            if (errorCode == TRUST_E_NOSIGNATURE)
            {
                return "Nenhuma assinatura reconhecida nesta verificação. Pode ser necessária validação por catálogo.";
            }

            return status switch
            {
                DigitalSignatureStatus.Trusted => "A assinatura digital foi validada pelo Windows.",
                DigitalSignatureStatus.Untrusted => "A validação da assinatura retornou uma falha de confiança.",
                _ => $"A verificação não foi conclusiva. Código: 0x{errorCode:X8}."
            };
        }

        private static DigitalSignatureResult CreateResult(string filePath, DigitalSignatureStatus status, string message, uint? errorCode, DateTimeOffset verifiedAt)
        {
            return new DigitalSignatureResult
            {
                FilePath = filePath,
                Status = status,
                Message = message,
                ErrorCode = errorCode,
                VerifiedAt = verifiedAt
            };
        }

        [DllImport("wintrust.dll", ExactSpelling = true, PreserveSig = true)]
        private static extern uint WinVerifyTrust(IntPtr hwnd, ref Guid pgActionID, ref WINTRUST_DATA pWVTData);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WINTRUST_FILE_INFO
        {
            public uint cbStruct;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pcwszFilePath;

            public IntPtr hFile;
            public IntPtr pgKnownSubject;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WINTRUST_DATA
        {
            public uint cbStruct;
            public IntPtr pPolicyCallbackData;
            public IntPtr pSIPClientData;
            public uint dwUIChoice;
            public uint dwRevocationChecks;
            public uint dwUnionChoice;
            public IntPtr pFile;
            public uint dwStateAction;
            public IntPtr hWVTStateData;
            public IntPtr pwszURLReference;
            public uint dwProvFlags;
            public uint dwUIContext;
            public IntPtr pSignatureSettings;
        }
    }
}
