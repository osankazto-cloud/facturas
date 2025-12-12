using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using Dapper;
using Npgsql;

namespace facturas.Data
{
    public class Invoice
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public string Nif { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Xml { get; set; } = string.Empty;
    }

    public class InvoiceRepository
    {
        private readonly string _connectionString;

        public InvoiceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private NpgsqlConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public IEnumerable<Invoice> GetAllInvoices()
        {
            using var conn = CreateConnection();
            return conn.Query<Invoice>("SELECT id AS Id, codigo AS Codigo, proveedor AS Proveedor, nif AS Nif, fecha AS Fecha, total AS Total, xml AS Xml FROM invoices ORDER BY fecha DESC");
        }

        public Invoice? GetInvoiceById(int id)
        {
            using var conn = CreateConnection();
            return conn.QueryFirstOrDefault<Invoice>("SELECT * FROM invoices WHERE id = @Id", new { id });
        }

        public int InsertInvoice(Invoice invoice)
        {
            using var conn = CreateConnection();
            var sql = @"INSERT INTO invoices (codigo, proveedor, nif, fecha, total, xml) VALUES (@Codigo, @Proveedor, @Nif, @Fecha, @Total, @Xml)";
            return conn.Execute(sql, invoice);
        }

        public int DeleteInvoice(int id)
        {
            using var conn = CreateConnection();
            return conn.Execute("DELETE FROM invoices WHERE id = @Id", new { Id = id });
        }
    }
}
