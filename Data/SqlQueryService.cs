using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace facturas.Data
{
    public class SqlQueryService
    {
        private readonly string _queriesFilePath;

        public SqlQueryService(string queriesFilePath = "Data/queries.json")
        {
            if (string.IsNullOrWhiteSpace(queriesFilePath)) throw new ArgumentException("queriesFilePath must be provided", nameof(queriesFilePath));

            // If an absolute path was provided use it directly
            if (Path.IsPathRooted(queriesFilePath))
            {
                _queriesFilePath = Path.GetFullPath(queriesFilePath);
            }
            else
            {
                // Candidate locations (in order): current working directory, executable directory
                var candidates = new[]
                {
                    Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), queriesFilePath)),
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, queriesFilePath))
                };

                // Use the first existing file if present, otherwise default to the exe directory location
                var existing = Array.Find(candidates, File.Exists);
                _queriesFilePath = existing ?? candidates[1];
            }

            var dir = Path.GetDirectoryName(_queriesFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            Debug.WriteLine($"[SqlQueryService] Queries file: {_queriesFilePath}");
        }

        public record QueryRecord(string Sql, Dictionary<string, object?> Parameters, DateTime Timestamp);

        public IEnumerable<QueryRecord> LoadAll()
        {
            if (!File.Exists(_queriesFilePath)) return Array.Empty<QueryRecord>();
            var json = File.ReadAllText(_queriesFilePath);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            try
            {
                var list = JsonSerializer.Deserialize<List<QueryRecord>>(json, opts);
                return list ?? new List<QueryRecord>();
            }
            catch (System.Text.Json.JsonException jex)
            {
                Debug.WriteLine($"[SqlQueryService] JSON parse error reading '{_queriesFilePath}': {jex.Message}");
                try
                {
                    // Move corrupted file aside so EXE can continue with a fresh file
                    var corruptPath = _queriesFilePath + ".corrupt." + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".json";
                    File.Move(_queriesFilePath, corruptPath);
                    Debug.WriteLine($"[SqlQueryService] Moved corrupted queries file to: {corruptPath}");
                }
                catch (Exception moveEx)
                {
                    Debug.WriteLine($"[SqlQueryService] Failed to move corrupted file: {moveEx}");
                }

                return new List<QueryRecord>();
            }
        }

        public void Append(QueryRecord rec)
        {
            var list = new List<QueryRecord>(LoadAll());
            list.Add(rec);
            var opts = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            try
            {
                Debug.WriteLine($"[SqlQueryService] Writing {list.Count} records to {_queriesFilePath}");
                File.WriteAllText(_queriesFilePath, JsonSerializer.Serialize(list, opts));
                Debug.WriteLine($"[SqlQueryService] Successfully wrote queries file.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SqlQueryService] Error writing queries file: {ex}");
                // Surface IO issues during development; consider logging in production
                throw new IOException($"Error writing queries file '{_queriesFilePath}'", ex);
            }
        }

        // Generate an INSERT statement from invoice XML content and persist it to the queries file.
        public QueryRecord GenerateInsertFromXml(string xmlContent)
        {
            var doc = XDocument.Parse(xmlContent);
            var root = doc.Element("Factura");
            if (root == null) throw new InvalidOperationException("XML no contiene elemento raiz 'Factura'.");

            var codigo = (string?)root.Element("Codigo") ?? string.Empty;
            var cab = root.Element("Cabecera");
            var proveedor = cab != null ? (string?)cab.Element("Proveedor") ?? string.Empty : string.Empty;
            var nif = cab != null ? (string?)cab.Element("NIF") ?? string.Empty : string.Empty;
            var fechaStr = cab != null ? (string?)cab.Element("Fecha") ?? string.Empty : string.Empty;
            DateTime fecha = DateTime.MinValue;
            if (!DateTime.TryParse(fechaStr, out fecha)) fecha = DateTime.Now;

            decimal total = 0m;
            var pie = root.Element("Pie");
            if (pie != null)
            {
                var totalStr = (string?)pie.Element("Total") ?? "0";
                decimal.TryParse(totalStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out total);
            }

            var sql = "INSERT INTO invoices (codigo, proveedor, nif, fecha, total, xml) VALUES (@codigo, @proveedor, @nif, @fecha, @total, @xml);";
            var parameters = new Dictionary<string, object?>
            {
                { "codigo", codigo },
                { "proveedor", proveedor },
                { "nif", nif },
                { "fecha", fecha },
                { "total", total },
                { "xml", xmlContent }
            };

            var rec = new QueryRecord(sql, parameters, DateTime.UtcNow);
            Append(rec);
            return rec;
        }
    }
}
