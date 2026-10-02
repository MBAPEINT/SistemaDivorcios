using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Divorcios.Datos.Excepciones;
using Divorcios.Datos.Interfaces;
using Divorcios.Datos.Opciones;
using Divorcios.Datos.Resultados;
using Microsoft.Extensions.Options;
using UglyToad.PdfPig;

namespace Divorcios.Datos.Almacenamiento
{
    public sealed class AlmacenamientoArchivosLocal(IOptions<AlmacenamientoArchivosOpciones> opciones) : IAlmacenamientoArchivos
    {
        private static readonly Regex ClaveValida = new("^[a-f0-9]{2}/[a-f0-9]{32}\\.bin$", RegexOptions.CultureInvariant);

        private string Raiz()
        {
            var ruta = opciones.Value.DirectorioRaiz;
            if (string.IsNullOrWhiteSpace(ruta))
                ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SistemaDivorcios", "archivos");
            if (!Path.IsPathFullyQualified(ruta)) throw new ArchivoPersistenciaException("ALMACENAMIENTO_NO_DISPONIBLE");
            return Path.GetFullPath(ruta);
        }

        private string Ruta(string clave)
        {
            if (!ClaveValida.IsMatch(clave) || !clave.AsSpan(0, 2).SequenceEqual(clave.AsSpan(3, 2)))
                throw new ArchivoPersistenciaException("ALMACENAMIENTO_NO_DISPONIBLE");
            var raiz = Raiz();
            var ruta = Path.GetFullPath(Path.Combine(raiz, clave.Replace('/', Path.DirectorySeparatorChar)));
            if (!ruta.StartsWith(raiz.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                throw new ArchivoPersistenciaException("ALMACENAMIENTO_NO_DISPONIBLE");
            VerificarSinEnlaces(ruta);
            return ruta;
        }

        private static void VerificarSinEnlaces(string ruta)
        {
            var actual = Path.GetPathRoot(ruta)!;
            foreach (var segmento in ruta[actual.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                actual = Path.Combine(actual, segmento);
                try
                {
                    if ((File.GetAttributes(actual) & FileAttributes.ReparsePoint) != 0)
                        throw new ArchivoPersistenciaException("ALMACENAMIENTO_NO_DISPONIBLE");
                }
                catch (FileNotFoundException) { }
                catch (DirectoryNotFoundException) { }
            }
        }

        public async Task<ArchivoAlmacenado> GuardarNuevoAsync(Stream contenido, long maximoBytes, string mimeEsperado, CancellationToken cancellationToken)
        {
            string? temporal = null;
            var temporalPropio = false;
            try
            {
                var nonce = Guid.NewGuid().ToString("N");
                var clave = nonce[..2] + "/" + nonce + ".bin";
                var final = Ruta(clave);
                Directory.CreateDirectory(Path.GetDirectoryName(final)!);
                temporal = final + "." + Guid.NewGuid().ToString("N") + ".tmp";
                VerificarSinEnlaces(temporal);
                long tamano = 0;
                using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                await using (var salida = new FileStream(temporal, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 65536, FileOptions.Asynchronous))
                {
                    temporalPropio = true;
                    var buffer = new byte[65536];
                    int leidos;
                    while ((leidos = await contenido.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        if (tamano > maximoBytes - leidos) throw new ArchivoPersistenciaException("ARCHIVO_DEMASIADO_GRANDE");
                        tamano += leidos;
                        hash.AppendData(buffer, 0, leidos);
                        await salida.WriteAsync(buffer.AsMemory(0, leidos), cancellationToken);
                    }
                    if (tamano == 0) throw new ArchivoPersistenciaException("ARCHIVO_VACIO");
                    await salida.FlushAsync(cancellationToken);
                    var detectado = await DetectarMimeAsync(salida, cancellationToken);
                    if (detectado is null || detectado != mimeEsperado) throw new ArchivoPersistenciaException("CONTENIDO_NO_PERMITIDO");
                }
                cancellationToken.ThrowIfCancellationRequested();
                VerificarSinEnlaces(final);
                // Sin sobrescritura. Una corrección siempre obtiene una clave y bytes independientes.
                File.Move(temporal, final, overwrite: false);
                temporalPropio = false;
                return new(clave, mimeEsperado, tamano, Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
            }
            catch (Exception excepcion) when (excepcion is IOException or UnauthorizedAccessException)
            {
                throw new ArchivoPersistenciaException("ALMACENAMIENTO_NO_DISPONIBLE", excepcion);
            }
            finally
            {
                // Exclusivamente el temporal recién creado por esta operación.
                if (temporalPropio && temporal is not null)
                {
                    try { File.Delete(temporal); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                }
            }
        }

        public async Task<Stream> AbrirVerificadoAsync(string clave, string sha256, long tamanoBytes, string mimeType, CancellationToken cancellationToken)
        {
            FileStream? archivo = null;
            try
            {
                archivo = new FileStream(Ruta(clave), FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.Asynchronous);
                if (archivo.Length != tamanoBytes) throw new ArchivoPersistenciaException("INTEGRIDAD_ARCHIVO");
                var hash = await SHA256.HashDataAsync(archivo, cancellationToken);
                if (!CryptographicOperations.FixedTimeEquals(hash, Convert.FromHexString(sha256))
                    || await DetectarMimeAsync(archivo, cancellationToken) != mimeType)
                    throw new ArchivoPersistenciaException("INTEGRIDAD_ARCHIVO");
                archivo.Position = 0;
                var resultado = archivo;
                archivo = null;
                return resultado;
            }
            catch (Exception excepcion) when (excepcion is IOException or UnauthorizedAccessException or FormatException)
            {
                throw new ArchivoPersistenciaException("ALMACENAMIENTO_NO_DISPONIBLE", excepcion);
            }
            finally
            {
                if (archivo is not null) await archivo.DisposeAsync();
            }
        }

        public Task DescartarNuevoAsync(string clave, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try { File.Delete(Ruta(clave)); }
            catch (Exception excepcion) when (excepcion is IOException or UnauthorizedAccessException)
            {
                throw new ArchivoPersistenciaException("ALMACENAMIENTO_NO_DISPONIBLE", excepcion);
            }
            return Task.CompletedTask;
        }

        // El PDF se analiza sin ejecutar contenido. La estructura válida no certifica su valor documental.
        private static async Task<string?> DetectarMimeAsync(FileStream archivo, CancellationToken cancellationToken)
        {
            var inicio = new byte[Math.Min(32, (int)Math.Min(archivo.Length, 32))];
            archivo.Position = 0;
            await archivo.ReadExactlyAsync(inicio, cancellationToken);
            var fin = new byte[(int)Math.Min(1024, archivo.Length)];
            archivo.Position = archivo.Length - fin.Length;
            await archivo.ReadExactlyAsync(fin, cancellationToken);
            if (inicio.Length >= 8 && Encoding.ASCII.GetString(inicio.AsSpan(0, 5)) == "%PDF-"
                && inicio[5] is (byte)'1' or (byte)'2' && inicio[6] == (byte)'.' && inicio[7] is >= (byte)'0' and <= (byte)'9'
                && Encoding.ASCII.GetString(fin).Contains("%%EOF", StringComparison.Ordinal))
            {
                archivo.Position = 0;
                using var copia = new MemoryStream();
                await archivo.CopyToAsync(copia, cancellationToken);
                try
                {
                    using var pdf = PdfDocument.Open(copia.ToArray(), new ParsingOptions { UseLenientParsing = false });
                    if (pdf.IsEncrypted || pdf.NumberOfPages < 1) return null;
                    for (var i = 1; i <= pdf.NumberOfPages; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var pagina = pdf.GetPage(i);
                        if (pagina.Width <= 0 || pagina.Height <= 0) return null;
                    }
                    return "application/pdf";
                }
                catch (Exception e) when (e is not OperationCanceledException and not OutOfMemoryException
                    and not IOException and not UnauthorizedAccessException)
                {
                    return null;
                }
            }
            if (inicio.Length >= 3 && inicio[0] == 0xff && inicio[1] == 0xd8 && inicio[2] == 0xff
                && fin.Length >= 2 && fin[^2] == 0xff && fin[^1] == 0xd9)
                return "image/jpeg";
            byte[] png = [137, 80, 78, 71, 13, 10, 26, 10];
            byte[] iend = [0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130];
            if (inicio.Length >= 24 && inicio.AsSpan(0, 8).SequenceEqual(png)
                && inicio.AsSpan(8, 8).SequenceEqual(new byte[] { 0, 0, 0, 13, 73, 72, 68, 82 })
                && fin.Length >= 12 && fin.AsSpan(fin.Length - 12).SequenceEqual(iend))
                return "image/png";
            return null;
        }
    }
}
