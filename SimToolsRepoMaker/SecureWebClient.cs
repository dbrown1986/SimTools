using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimTools
{
    public static class SecureWebClient
    {
        /// <summary>
        /// Sends a secure HTTP POST request with a JSON payload using BouncyCastle TLS.
        /// Bypasses native Windows Schannel entirely (fixes Win 7 SSL issues).
        /// </summary>
        public static async Task<string> PostJsonAsync(string url, string jsonData)
        {
            var uri = new Uri(url);
            string host = uri.Host;
            int port = uri.Port == -1 ? 443 : uri.Port;

            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(host, port);
                Stream rawStream = tcpClient.GetStream();

                var tlsClientProtocol = new TlsClientProtocol(rawStream);
                tlsClientProtocol.Connect(new LegacyTlsClient());

                using (Stream secureStream = tlsClientProtocol.Stream)
                {
                    byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonData);

                    // Manually construct the raw HTTP/1.1 POST Request headers
                    string httpRequest = $"POST {uri.PathAndQuery} HTTP/1.1\r\n" +
                                         $"Host: {host}\r\n" +
                                         "Content-Type: application/json; charset=utf-8\r\n" +
                                         $"Content-Length: {bodyBytes.Length}\r\n" +
                                         "Connection: close\r\n" +
                                         "User-Agent: SimToolsUpdater\r\n\r\n";

                    byte[] headerBytes = Encoding.ASCII.GetBytes(httpRequest);

                    // Write the headers, then immediately stream the JSON body payload
                    await secureStream.WriteAsync(headerBytes, 0, headerBytes.Length);
                    await secureStream.WriteAsync(bodyBytes, 0, bodyBytes.Length);
                    await secureStream.FlushAsync();

                    // Parse and process the raw TCP response stream
                    using (var ms = new MemoryStream())
                    {
                        await secureStream.CopyToAsync(ms);
                        byte[] responseBytes = ms.ToArray();

                        int headerEndIndex = FindHeaderEnd(responseBytes);
                        if (headerEndIndex == -1)
                            throw new Exception("Invalid HTTP response format.");

                        // Split headers from body
                        string headersText = Encoding.ASCII.GetString(responseBytes, 0, headerEndIndex);
                        string bodyText = Encoding.UTF8.GetString(responseBytes, headerEndIndex, responseBytes.Length - headerEndIndex);

                        // Check the headers for standard HTTP error statuses
                        if (headersText.Contains("HTTP/1.1 409") || headersText.Contains("409 Conflict"))
                        {
                            throw new HttpRequestException("409 Conflict", null, System.Net.HttpStatusCode.Conflict);
                        }
                        if (headersText.Contains("HTTP/1.1 400") || headersText.Contains("HTTP/1.1 404") || headersText.Contains("HTTP/1.1 500"))
                        {
                            throw new HttpRequestException("Server rejected activation.", null, System.Net.HttpStatusCode.BadRequest);
                        }

                        return bodyText;
                    }
                }
            }
        }

        /// <summary>
        /// Downloads a file from a secure URL using modern BouncyCastle v2.x cryptographic TLS engine,
        /// completely bypassing Windows Schannel. Perfect for Windows 7.
        /// </summary>
        public static async Task DownloadFileAsync(string url, string destinationPath)
        {
            var uri = new Uri(url);
            string host = uri.Host;
            int port = uri.Port == -1 ? 443 : uri.Port;

            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(host, port);
                Stream rawStream = tcpClient.GetStream();

                var tlsClientProtocol = new TlsClientProtocol(rawStream);
                tlsClientProtocol.Connect(new LegacyTlsClient());

                using (Stream secureStream = tlsClientProtocol.Stream)
                {
                    string httpRequest = $"GET {uri.PathAndQuery} HTTP/1.1\r\n" +
                                         $"Host: {host}\r\n" +
                                         "Connection: close\r\n" +
                                         "User-Agent: SimToolsUpdater\r\n\r\n";

                    byte[] requestBytes = Encoding.ASCII.GetBytes(httpRequest);
                    await secureStream.WriteAsync(requestBytes, 0, requestBytes.Length);
                    await secureStream.FlushAsync();

                    using (var ms = new MemoryStream())
                    {
                        await secureStream.CopyToAsync(ms);
                        byte[] responseBytes = ms.ToArray();

                        int headerEndIndex = FindHeaderEnd(responseBytes);
                        if (headerEndIndex == -1)
                            throw new Exception("Invalid HTTP response format.");

                        using (var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                        {
                            await fs.WriteAsync(responseBytes, headerEndIndex, responseBytes.Length - headerEndIndex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Downloads a file with real-time progress reporting using BouncyCastle TLS.
        /// </summary>
        public static async Task DownloadFileWithProgressAsync(
            string url,
            string destinationPath,
            Action<int> onProgress,
            Action onIndeterminate,
            Action<DateTime?> onHeadersParsed)
        {
            var uri = new Uri(url);
            string host = uri.Host;
            int port = uri.Port == -1 ? 443 : uri.Port;

            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(host, port);
                Stream rawStream = tcpClient.GetStream();

                var tlsClientProtocol = new TlsClientProtocol(rawStream);
                tlsClientProtocol.Connect(new LegacyTlsClient());

                using (Stream secureStream = tlsClientProtocol.Stream)
                {
                    string httpRequest = $"GET {uri.PathAndQuery} HTTP/1.1\r\n" +
                                         $"Host: {host}\r\n" +
                                         "Connection: close\r\n" +
                                         "User-Agent: SimToolsUpdater\r\n\r\n";

                    byte[] requestBytes = Encoding.ASCII.GetBytes(httpRequest);
                    await secureStream.WriteAsync(requestBytes, 0, requestBytes.Length);
                    await secureStream.FlushAsync();

                    using (var ms = new MemoryStream())
                    {
                        byte[] tempBuffer = new byte[4096];
                        int bytesRead;
                        int headerEndIndex = -1;

                        while ((bytesRead = await secureStream.ReadAsync(tempBuffer, 0, tempBuffer.Length)) > 0)
                        {
                            await ms.WriteAsync(tempBuffer, 0, bytesRead);
                            byte[] currentData = ms.ToArray();
                            headerEndIndex = FindHeaderEnd(currentData);
                            if (headerEndIndex != -1) break;
                        }

                        if (headerEndIndex == -1)
                            throw new Exception("Invalid HTTP response format.");

                        byte[] fullBuffered = ms.ToArray();
                        string headerText = Encoding.ASCII.GetString(fullBuffered, 0, headerEndIndex);

                        long? totalBytes = null;
                        DateTime? remoteLastModified = null;

                        string[] headers = headerText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var header in headers)
                        {
                            if (header.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                            {
                                if (long.TryParse(header.Substring(15).Trim(), out long length))
                                    totalBytes = length;
                            }
                            else if (header.StartsWith("Last-Modified:", StringComparison.OrdinalIgnoreCase))
                            {
                                if (DateTime.TryParse(header.Substring(14).Trim(), out DateTime modified))
                                    remoteLastModified = modified;
                            }
                        }

                        onHeadersParsed?.Invoke(remoteLastModified);

                        using (var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            int initialBodySize = fullBuffered.Length - headerEndIndex;
                            if (initialBodySize > 0)
                            {
                                await fs.WriteAsync(fullBuffered, headerEndIndex, initialBodySize);
                            }

                            byte[] buffer = new byte[8192];
                            long totalBytesRead = initialBodySize;
                            int lastPercent = 0;

                            if (totalBytes.HasValue)
                            {
                                int pct = (int)(totalBytesRead * 100 / totalBytes.Value);
                                onProgress?.Invoke(pct);
                            }
                            else
                            {
                                onIndeterminate?.Invoke();
                            }

                            while ((bytesRead = await secureStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fs.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;

                                if (totalBytes.HasValue)
                                {
                                    int pct = (int)(totalBytesRead * 100 / totalBytes.Value);
                                    if (pct != lastPercent)
                                    {
                                        lastPercent = pct;
                                        onProgress?.Invoke(pct);
                                    }
                                }
                                else
                                {
                                    onIndeterminate?.Invoke();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static int FindHeaderEnd(byte[] data)
        {
            for (int i = 0; i < data.Length - 3; i++)
            {
                if (data[i] == 13 && data[i + 1] == 10 && data[i + 2] == 13 && data[i + 3] == 10)
                {
                    return i + 4; // Skip past \r\n\r\n
                }
            }
            return -1;
        }

        public static async Task<string> GetStringAsync(string url)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"simtools_temp_{Guid.NewGuid():N}.txt");

            try
            {
                await DownloadFileAsync(url, tempFile);

                if (File.Exists(tempFile))
                {
                    return await File.ReadAllTextAsync(tempFile);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch
                {
                    // Ignore clean-up failures
                }
            }

            return string.Empty;
        }
    }

    public class LegacyTlsClient : DefaultTlsClient
    {
        public LegacyTlsClient() : base(new BcTlsCrypto(new SecureRandom()))
        {
        }

        public override TlsAuthentication GetAuthentication()
        {
            return new LegacyTlsAuthentication();
        }
    }

    public class LegacyTlsAuthentication : TlsAuthentication
    {
        public void NotifyServerCertificate(TlsServerCertificate serverCertificate)
        {
            // Accept any server certificate without OS verification
        }

        public TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
        {
            // Using null! suppresses the warning while correctly returning null to BouncyCastle
            return null!;
        }
    }
}