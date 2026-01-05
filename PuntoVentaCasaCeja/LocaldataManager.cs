using System.Collections.Generic;
using System.Data.SQLite;
using Windows.Storage;
using System.IO;
using System.Data;
using Newtonsoft.Json;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using PuntoVentaCasaCeja.Properties;
using System.Windows.Markup;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Diagnostics;

namespace PuntoVentaCasaCeja
{
    public class LocaldataManager
    {
        public string impresora = "";
        string dbpath;
        public SQLiteConnection connection;
        private const string PreloadedDbName = "PreLoadedCatalog.db"; // Nuevo: Nombre del archivo precargado
        public bool IsCatalogPreloaded { get; private set; } // Nuevo: Bandera para saber si usó el catálogo precargado
                                                             // Diccionario completo de comandos CREATE TABLE
        private readonly Dictionary<string, string> tableCreationCommands = new Dictionary<string, string>
        {
            // 1. Tablas de PreLoadedCatalog.db correspondiente a productos, categorias y medidas
            { "productos", "CREATE TABLE 'productos' ( 'id' INTEGER NOT NULL, 'codigo' TEXT, 'nombre' TEXT, 'presentacion' TEXT, 'iva' REAL, 'menudeo' REAL, 'mayoreo' REAL, 'cantidad_mayoreo' INTEGER, 'especial' REAL, 'vendedor' REAL, 'imagen' TEXT, 'activo' INTEGER, 'created_at' TEXT, 'updated_at' TEXT, 'medida_id' INTEGER, 'categoria_id' INTEGER, FOREIGN KEY('categoria_id') REFERENCES 'categorias'('id'), PRIMARY KEY('id'), FOREIGN KEY('medida_id') REFERENCES 'medidas'('id'))" },
            { "categorias", "CREATE TABLE 'categorias' ( 'id' INTEGER NOT NULL, 'nombre' TEXT, 'activo' INTEGER, 'isdescuento' INTEGER, 'descuento' REAL, 'created_at' TEXT, 'updated_at' TEXT, PRIMARY KEY('id'))" },
            { "medidas", "CREATE TABLE 'medidas' ( 'id' INTEGER NOT NULL, 'nombre' TEXT, 'activo' INTEGER, 'created_at' TEXT, 'updated_at' TEXT, PRIMARY KEY('id'))" },
            
            // 2. Tablas básicas sin dependencias
            {"sucursales", "CREATE TABLE 'sucursales' ('id' INTEGER NOT NULL, 'puerta_enlace1' TEXT, 'puerta_enlace2' TEXT, 'puerta_enlace3' TEXT, 'puerta_enlace4' TEXT, 'razon_social' TEXT, 'direccion' TEXT, 'correo' TEXT, 'activo' INTEGER, 'created_at' TEXT, 'updated_at' TEXT, PRIMARY KEY('id' AUTOINCREMENT))"},
            {"proveedores", "CREATE TABLE 'proveedores' ('id' INTEGER NOT NULL, 'nombre' TEXT, 'direccion' TEXT, 'correo' TEXT, 'telefono' TEXT, 'descripcion' TEXT, 'activo' INTEGER, 'created_at' TEXT, 'updated_at' TEXT, PRIMARY KEY('id'))"},
            {"usuarios", "CREATE TABLE 'usuarios' ('id' INTEGER NOT NULL, 'nombre' TEXT, 'correo' TEXT, 'confirmacion' INTEGER, 'telefono' TEXT, 'imagen' TEXT, 'usuario' TEXT, 'clave' TEXT, 'is_root' INTEGER, 'activo' INTEGER, 'created_at' TEXT, 'updated_at' TEXT, PRIMARY KEY('id'))"},

            // 3. Tablas que dependen de las básicas
            {"clientes", "CREATE TABLE 'clientes' ('id' INTEGER, 'nombre' TEXT, 'rfc' TEXT, 'calle' TEXT, 'no_exterior' TEXT, 'no_interior' TEXT, 'cp' TEXT, 'colonia' TEXT, 'ciudad' TEXT, 'telefono' TEXT, 'correo' TEXT, 'activo' INTEGER, 'created_at' TEXT, 'updated_at' TEXT, PRIMARY KEY('id'))"},
            {"clientes_temporal", "CREATE TABLE 'clientes_temporal' ('id' INTEGER, 'nombre' TEXT, 'rfc' TEXT, 'calle' TEXT, 'no_exterior' TEXT, 'no_interior' TEXT, 'cp' TEXT, 'colonia' TEXT, 'ciudad' TEXT, 'telefono' TEXT, 'correo' TEXT, PRIMARY KEY('id' AUTOINCREMENT))"},
            {"entradas", "CREATE TABLE 'entradas' ('id' INTEGER NOT NULL, 'fecha_factura' TEXT, 'total_factura' REAL, 'folio_factura' TEXT, 'usuario_id' INTEGER, 'sucursal_id' INTEGER, 'proveedor_id' INTEGER, 'cancelacion' INTEGER, 'estado' INTEGER, 'detalles' TEXT, 'created_at' TEXT, 'updated_at' TEXT, 'comentarios' TEXT DEFAULT NULL, PRIMARY KEY('id' AUTOINCREMENT))"},
            
            // 4. Tablas de transacciones principales
            {"ventas", "CREATE TABLE 'ventas' ('id' INTEGER NOT NULL, 'total' REAL, 'descuento' REAL, 'folio' TEXT, 'folio_corte' TEXT, 'fecha_venta' TEXT, 'metodo_pago' TEXT, 'tipo' INTEGER, 'sucursal_id' INTEGER, 'usuario_id' INTEGER, 'cancelacion' TEXT, 'estado' INTEGER, 'detalles' TEXT, FOREIGN KEY('usuario_id') REFERENCES 'usuarios'('id'), FOREIGN KEY('sucursal_id') REFERENCES 'sucursales'('id'), PRIMARY KEY('id' AUTOINCREMENT))"},
            {"apartados", "CREATE TABLE 'apartados' ('id' INTEGER, 'productos' TEXT, 'total' REAL, 'total_pagado' REAL, 'fecha_apartado' TEXT, 'folio_corte' TEXT, 'fecha_entrega' TEXT, 'estado' INTEGER, 'cliente_creditos_id' INTEGER, 'id_cajero_registro' INTEGER, 'id_cejero_entrega' INTEGER, 'sucursal_id' INTEGER, 'observaciones' TEXT, 'created_at' TEXT, 'updated_at' TEXT, FOREIGN KEY('id_cajero_registro') REFERENCES 'usuarios'('id'), FOREIGN KEY('id_cejero_entrega') REFERENCES 'usuarios'('id'), PRIMARY KEY('id' AUTOINCREMENT))"},
            {"creditos", "CREATE TABLE 'creditos' ('id' INTEGER, 'productos' TEXT, 'total' REAL, 'total_pagado' REAL, 'fecha_de_credito' TEXT, 'folio' TEXT, 'estado' INTEGER, 'cliente_creditos_id' INTEGER, 'id_cajero_registro' INTEGER, 'sucursal_id' INTEGER, 'observaciones' TEXT, 'created_at' TEXT, 'updated_at' TEXT, FOREIGN KEY('sucursal_id') REFERENCES 'sucursales'('id'), FOREIGN KEY('id_cajero_registro') REFERENCES 'usuarios'('id'), PRIMARY KEY('id'))"},

            // 5. Tablas de soporte para transacciones
            {"producto_venta", "CREATE TABLE 'producto_venta' ('id' INTEGER NOT NULL, 'venta_id' INTEGER, 'producto_id' INTEGER, 'codigo' TEXT, 'cantidad' INTEGER, 'precio_venta' REAL, 'estado' INTEGER, 'detalles' TEXT, PRIMARY KEY('id' AUTOINCREMENT), FOREIGN KEY('producto_id') REFERENCES 'productos'('id'), FOREIGN KEY('venta_id') REFERENCES 'ventas'('id'))"},
            {"producto_entrada", "CREATE TABLE 'producto_entrada' ('id' INTEGER NOT NULL, 'entrada_id' INTEGER, 'producto_id' INTEGER, 'codigo' INTEGER, 'cantidad' INTEGER, 'costo' REAL, 'estado' INTEGER, 'detalles' TEXT, 'created_at' TEXT, 'updated_at' TEXT, PRIMARY KEY('id' AUTOINCREMENT))"},
            {"salidas", "CREATE TABLE 'salidas' ('id' INTEGER NOT NULL, 'id_sucursal_origen' INTEGER NOT NULL, 'id_sucursal_destino' INTEGER NOT NULL, 'productos' TEXT NOT NULL, 'folio' TEXT NOT NULL, 'fecha_salida' TEXT NOT NULL, 'usuario_id' INTEGER NOT NULL, 'total_importe' REAL NOT NULL, 'cancelado' INTEGER NOT NULL DEFAULT 0, 'created_at' TEXT NOT NULL, 'updated_at' TEXT NOT NULL, PRIMARY KEY('id' AUTOINCREMENT))"},

            // 6. Tablas temporales
            {"apartados_temporal", "CREATE TABLE 'apartados_temporal' ('id' INTEGER, 'productos' TEXT, 'total' REAL, 'total_pagado' REAL, 'fecha_apartado' TEXT, 'folio_corte' TEXT, 'fecha_entrega' TEXT, 'estado' INTEGER, 'cliente_creditos_id' INTEGER, 'id_cajero_registro' INTEGER, 'id_cajero_entrega' INTEGER, 'sucursal_id' INTEGER, 'temporal' INTEGER, 'observaciones' TEXT, PRIMARY KEY('id' AUTOINCREMENT), FOREIGN KEY('id_cajero_entrega') REFERENCES 'usuarios'('id'), FOREIGN KEY('sucursal_id') REFERENCES 'sucursales'('id'), FOREIGN KEY('id_cajero_registro') REFERENCES 'usuarios'('id'))"},
            {"creditos_temporal", "CREATE TABLE 'creditos_temporal' ('id' INTEGER, 'productos' TEXT, 'total' REAL, 'total_pagado' REAL, 'fecha_de_credito' TEXT, 'folio' TEXT, 'estado' INTEGER, 'cliente_creditos_id' INTEGER, 'id_cajero_registro' INTEGER, 'sucursal_id' INTEGER, 'temporal' INTEGER, 'observaciones' TEXT, FOREIGN KEY('id_cajero_registro') REFERENCES 'usuarios'('id'), FOREIGN KEY('sucursal_id') REFERENCES 'sucursales'('id'), PRIMARY KEY('id' AUTOINCREMENT))"},
            {"salidas_temporal", "CREATE TABLE 'salidas_temporal' ('id' INTEGER NOT NULL, 'id_sucursal_origen' INTEGER, 'id_sucursal_destino' INTEGER, 'productos' TEXT, 'folio' TEXT, 'fecha_salida' TEXT, 'usuario_id' INTEGER, 'total_importe' REAL, 'estado' INTEGER, 'created_at' TEXT, 'updated_at' TEXT, PRIMARY KEY('id' AUTOINCREMENT))"},

            // 7. Tablas de abonos
            {"abonos_apartado", "CREATE TABLE 'abonos_apartado' ('id' INTEGER, 'folio' TEXT, 'metodo_pago' TEXT, 'total_abonado' REAL, 'fecha' TEXT, 'id_apartado' INTEGER, 'folio_corte' TEXT, 'id_cajero' INTEGER, 'created_at' TEXT, 'updated_at' TEXT, FOREIGN KEY('id_apartado') REFERENCES 'apartados'('id'), FOREIGN KEY('id_cajero') REFERENCES 'usuarios'('id'))"},
            {"abonos_credito", "CREATE TABLE 'abonos_credito' ('id' INTEGER, 'folio' TEXT, 'metodo_pago' TEXT, 'total_abonado' REAL, 'fecha' TEXT, 'id_credito' INTEGER, 'folio_corte' TEXT, 'id_cajero' INTEGER, 'created_at' TEXT, 'updated_at' TEXT, FOREIGN KEY('id_credito') REFERENCES 'creditos'('id'), FOREIGN KEY('id_cajero') REFERENCES 'usuarios'('id'))"},
            {"abonos_apartado_temporal", "CREATE TABLE 'abonos_apartado_temporal' ('id' INTEGER NOT NULL, 'folio' TEXT, 'metodo_pago' TEXT, 'total_abonado' REAL, 'fecha' TEXT, 'folio_apartado' TEXT, 'id_apartado' INTEGER, 'folio_corte' TEXT, 'id_cajero' INTEGER, PRIMARY KEY('id' AUTOINCREMENT))"},
            {"abonos_credito_temporal", "CREATE TABLE 'abonos_credito_temporal' ('id' INTEGER NOT NULL, 'folio' TEXT, 'metodo_pago' TEXT, 'total_abonado' REAL, 'fecha' TEXT, 'folio_credito' TEXT, 'id_credito' INTEGER, 'folio_corte' TEXT, 'id_cajero' INTEGER, PRIMARY KEY('id' AUTOINCREMENT))"},

            // 8. Otras tablas
            {"cortes", "CREATE TABLE 'cortes' ('id' INTEGER NOT NULL UNIQUE, 'folio_corte' TEXT, 'fondo_apertura' REAL, 'total_efectivo' REAL, 'total_tarjetas_debito' REAL, 'total_tarjetas_credito' REAL, 'total_cheques' REAL, 'total_transferencias' REAL, 'efectivo_apartados' REAL, 'efectivo_creditos' REAL, 'gastos' TEXT, 'ingresos' TEXT, 'sobrante' REAL, 'fecha_apertura_caja' TEXT, 'fecha_corte_caja' TEXT, 'sucursal_id' INTEGER, 'usuario_id' INTEGER, 'estado' INTEGER, 'detalles' TEXT, 'created_at' TEXT, 'updated_at' TEXT, 'total_apartados' REAL DEFAULT 0, 'total_creditos' REAL DEFAULT 0, FOREIGN KEY('sucursal_id') REFERENCES 'sucursales'('id'), FOREIGN KEY('usuario_id') REFERENCES 'usuarios'('id'), PRIMARY KEY('id' AUTOINCREMENT))"},
            {"operaciones", "CREATE TABLE 'operaciones' ('id' INTEGER NOT NULL, 'accion' TEXT, 'confirmar' INTEGER, 'created_at' TEXT, 'updated_at' TEXT, 'producto_id' INTEGER, 'usuario_id' INTEGER, PRIMARY KEY('id'))"},
            {"alta_temporal", "CREATE TABLE 'alta_temporal' ('id' INTEGER NOT NULL, 'codigo' TEXT, 'nombre' TEXT, 'presentacion' TEXT, 'menudeo' REAL, 'mayoreo' REAL, 'cantidad_mayoreo' INTEGER, 'especial' REAL, 'vendedor' REAL, 'medida_id' INTEGER, 'categoria_id' INTEGER, 'estado' INTEGER, 'detalles' TEXT, FOREIGN KEY('medida_id') REFERENCES 'medidas'('id'), PRIMARY KEY('id' AUTOINCREMENT), FOREIGN KEY('categoria_id') REFERENCES 'categorias'('id'))"},           
        };

        public LocaldataManager()
        {
            dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"CasaCeja\DataBase\CasaCeja.db");

            if (!File.Exists(dbpath))
            {
                string preloadedPath = Path.Combine(Application.StartupPath, "Data", PreloadedDbName);

                // Intentar usar la base precargada si existe
                if (File.Exists(preloadedPath))
                {
                    try
                    {
                        string subPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"CasaCeja\DataBase");
                        Directory.CreateDirectory(subPath);
                        File.Copy(preloadedPath, dbpath);
                        IsCatalogPreloaded = true;

                        connection = new SQLiteConnection(@"data source=" + dbpath);
                        connection.Open();
                        EnsureAdditionalTablesExist();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error usando DB precargada: {ex.Message}");
                    }
                }

                CreateNewDatabase();
            }
            else
            {
                connection = new SQLiteConnection(@"data source=" + dbpath);
                connection.Open();
                EnsureAdditionalTablesExist();
                IsCatalogPreloaded = CheckIfCatalogPreloaded();

            }
        }

        private void CreateNewDatabase()
        {
            string subPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"CasaCeja\DataBase");
            Directory.CreateDirectory(subPath);
            SQLiteConnection.CreateFile(dbpath);
            connection = new SQLiteConnection(@"data source=" + dbpath);
            connection.Open();
            CreateAllTables();
            IsCatalogPreloaded = false;
        }

        private void CreateAllTables()
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var cmdText in tableCreationCommands.Values)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = cmdText;
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        // Nuevo método: Verificar tablas adicionales
        private void EnsureAdditionalTablesExist()
        {
            var requiredTables = new List<string>
            {             
                "sucursales", "proveedores", "usuarios",                
                "clientes", "clientes_temporal", "entradas",
                "ventas", "apartados", "creditos",
                "producto_venta", "producto_entrada", "salidas",
                "apartados_temporal", "creditos_temporal", "salidas_temporal",   
                "abonos_apartado", "abonos_credito", "abonos_apartado_temporal", "abonos_credito_temporal",
                "cortes", "operaciones", "alta_temporal"
            };

            foreach (var table in requiredTables)
            {
                if (!TableExists(table))
                {                  
                    CreateSingleTable(table);
                }
                Console.WriteLine("Existe la tabla: " + table);
            }
        }

        private bool TableExists(string tableName)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name=@name";
                cmd.Parameters.AddWithValue("@name", tableName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }           
        }

        private void CreateSingleTable(string tableName)
        {            
            if (tableCreationCommands.TryGetValue(tableName, out string createCommand))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = createCommand;
                    command.ExecuteNonQuery();
                }
                Console.WriteLine("Se creo la tabla: " + tableName);
            }
            else
            {
                throw new ArgumentException($"No se encontró el comando CREATE TABLE para: {tableName}");
            }
        }


        // Modificado: Método para verificar si la tabla productos tiene datos
        public bool IsTableEmpty(string table)
        {
            // Tablas precargadas nunca se consideran vacías
            if (IsCatalogPreloaded && (table == "productos" || table == "categorias" || table == "medidas"))
                return false;

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(1) FROM " + table;
                using (var result = command.ExecuteReader())
                {
                    return !(result.Read() && result.GetInt32(0) > 0);
                }
            }
        }

        public void UpdateExistingProducts(List<Producto> productosActualizados)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var producto in productosActualizados)
                    {
                        var productoLocal = GetProductById(producto.id);

                        // Convertir fechas a DateTime para comparación
                        DateTime fechaActualizado;
                        DateTime fechaLocal;

                        bool fechaValidaActualizado = DateTime.TryParse(producto.updated_at, out fechaActualizado);
                        bool fechaValidaLocal = DateTime.TryParse(productoLocal?.updated_at, out fechaLocal);

                        if (productoLocal == null ||
                           (fechaValidaActualizado && fechaValidaLocal && fechaActualizado > fechaLocal))
                        {
                            if (productoLocal == null)
                            {
                                InsertProduct(producto);
                            }
                            else
                            {
                                UpdateProduct(producto);
                            }
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Debug.WriteLine($"Errorrrrr en actualización: {ex.Message}");
                    throw;
                }
            }
        }
        private Producto GetProductById(int id)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM productos WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Producto
                        {
                            id = reader.GetInt32(0),
                            codigo = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            nombre = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            presentacion = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            iva = reader.IsDBNull(4) ? 0 : reader.GetDouble(4),
                            menudeo = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                            mayoreo = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
                            cantidad_mayoreo = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                            especial = reader.IsDBNull(8) ? 0 : reader.GetDouble(8),
                            vendedor = reader.IsDBNull(9) ? 0 : reader.GetDouble(9),
                            imagen = reader.IsDBNull(10) ? "" : reader.GetString(10),
                            activo = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                            created_at = reader.IsDBNull(12)
                                ? DateTime.MinValue.ToString("yyyy-MM-dd HH:mm:ss")
                                : reader.GetString(12),
                            updated_at = reader.IsDBNull(13)
                                ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                : reader.GetString(13),
                            medida_id = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                            categoria_id = reader.IsDBNull(15) ? 0 : reader.GetInt32(15)
                        };
                    }
                }
            }
            return null;
        }

        private bool CheckIfCatalogPreloaded()
        {
            try
            {
                // Verificar si productos tiene datos
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM productos";
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine("Productos en la base de datos: " + count);
                    return count > 0;
                }
                
            }
            catch { return false; }
        }

        private void UpdateProduct(Producto producto)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
            UPDATE productos SET
                codigo = @codigo,
                nombre = @nombre,
                presentacion = @presentacion,
                iva = @iva,
                menudeo = @menudeo,
                mayoreo = @mayoreo,
                cantidad_mayoreo = @cantidad_mayoreo,
                especial = @especial,
                vendedor = @vendedor,
                imagen = @imagen,
                activo = @activo,
                updated_at = @updated_at,
                medida_id = @medida_id,
                categoria_id = @categoria_id
            WHERE id = @id";

                command.Parameters.AddWithValue("@id", producto.id);
                command.Parameters.AddWithValue("@codigo", producto.codigo ?? "");
                command.Parameters.AddWithValue("@nombre", producto.nombre ?? "");
                command.Parameters.AddWithValue("@presentacion", producto.presentacion ?? "");
                command.Parameters.AddWithValue("@iva", producto.iva);
                command.Parameters.AddWithValue("@menudeo", producto.menudeo);
                command.Parameters.AddWithValue("@mayoreo", producto.mayoreo);
                command.Parameters.AddWithValue("@cantidad_mayoreo", producto.cantidad_mayoreo);
                command.Parameters.AddWithValue("@especial", producto.especial);
                command.Parameters.AddWithValue("@vendedor", producto.vendedor);
                command.Parameters.AddWithValue("@imagen", producto.imagen ?? "");
                command.Parameters.AddWithValue("@activo", producto.activo);
                command.Parameters.AddWithValue("@created_at", producto.created_at);
                command.Parameters.AddWithValue("@updated_at", producto.updated_at);
                command.Parameters.AddWithValue("@medida_id", producto.medida_id);
                command.Parameters.AddWithValue("@categoria_id", producto.categoria_id);
                
                command.ExecuteNonQuery();
            }
        }

        private void InsertProduct(Producto producto)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
            INSERT INTO productos 
            (id, codigo, nombre, presentacion, iva, menudeo, mayoreo, cantidad_mayoreo, especial, 
             vendedor, imagen, activo, created_at, updated_at, medida_id, categoria_id) 
            VALUES 
            (@id, @codigo, @nombre, @presentacion, @iva, @menudeo, @mayoreo, @cantidad_mayoreo, @especial, 
             @vendedor, @imagen, @activo, @created_at, @updated_at, @medida_id, @categoria_id)";

                command.Parameters.AddWithValue("@id", producto.id);
                command.Parameters.AddWithValue("@codigo", producto.codigo ?? "");
                command.Parameters.AddWithValue("@nombre", producto.nombre ?? "");
                command.Parameters.AddWithValue("@presentacion", producto.presentacion ?? "");
                command.Parameters.AddWithValue("@iva", producto.iva);
                command.Parameters.AddWithValue("@menudeo", producto.menudeo);
                command.Parameters.AddWithValue("@mayoreo", producto.mayoreo);
                command.Parameters.AddWithValue("@cantidad_mayoreo", producto.cantidad_mayoreo);
                command.Parameters.AddWithValue("@especial", producto.especial);
                command.Parameters.AddWithValue("@vendedor", producto.vendedor);
                command.Parameters.AddWithValue("@imagen", producto.imagen ?? "");
                command.Parameters.AddWithValue("@activo", producto.activo);
                command.Parameters.AddWithValue("@created_at", producto.created_at);
                command.Parameters.AddWithValue("@updated_at", producto.updated_at);
                command.Parameters.AddWithValue("@medida_id", producto.medida_id);
                command.Parameters.AddWithValue("@categoria_id", producto.categoria_id);       

                command.ExecuteNonQuery();
            }
        }
       

        public void AnalisisTemporalTotal()
        {
            AnalizarAbonosApartadoTemporales();
            AnalizarAbonosCreditoTemporales();
            AnalizarApartadosTemporales();
            AnalizarClientesTemporales();
            AnalizarCrerditosTemporales();
            AnalizarProductosTemporales();
        }
        public void setImpresora(string impresora)
        {
            this.impresora = impresora;
        }

        public int GetTableRowCount(string table)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(1) FROM " + table + " WHERE activo=1";
                using (var result = command.ExecuteReader())
                {
                    return result.Read() ? result.GetInt32(0) : 0;
                }
            }
        }
        public string getSucursalname(int id)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT razon_social FROM sucursales WHERE id = @setId LIMIT 1";
            command.Parameters.AddWithValue("setId", id);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                string r = result.GetString(0);
                return r;
            }
            return "";
        }
        public string getSucursalAddr(int id)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT direccion FROM sucursales WHERE id = @setId LIMIT 1";
            command.Parameters.AddWithValue("setId", id);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                string r = result.IsDBNull(0)?"":result.GetString(0);
                return r;
            }
            return "";
        }
        public string getTableLastUpdate(string table)
        {
            string last_update = "2000-01-01T00:00:00Z";
            if (!IsTableEmpty(table))
            {
                string query = "SELECT updated_at FROM " + table + " ORDER BY updated_at DESC LIMIT 1";
                SQLiteCommand command = new SQLiteCommand(query, connection);
                SQLiteDataReader result = command.ExecuteReader();
                if (result.Read())
                {
                    last_update = result.GetString(0);
                }
            }
            return last_update;
        }
        public Dictionary<string, int> getIndicesAdministradores()
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id, nombre FROM usuarios WHERE is_root = 1 AND activo = 1";
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                map.Add(result.GetString(1), result.GetInt32(0));
            }
            return map;
        }
        public Dictionary<string, int> getIndicesMedidas()
        {
            Dictionary<string, int> map = new Dictionary<string, int>();

            string query = "SELECT * FROM medidas WHERE activo=1";
            SQLiteCommand command = new SQLiteCommand(query, connection);
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                map.Add(result.GetString(1), result.GetInt32(0));
            }
            return map;
        }
        public Dictionary<string, int> getIndicesCategorias()
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            string query = "SELECT * FROM categorias WHERE activo=1";
            SQLiteCommand command = new SQLiteCommand(query, connection);
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                string key = result.GetString(1);
                if (!map.ContainsKey(key))
                    map.Add(key, result.GetInt32(0));
            }
            return map;
        }
        public Dictionary<string, int> getIndicesSucursales()
        {
            Dictionary<string, int> map = new Dictionary<string, int>();

            string query = "SELECT * FROM sucursales WHERE activo=1";
            SQLiteCommand command = new SQLiteCommand(query, connection);
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                map.Add(result.GetString(5), result.GetInt32(0));
            }
            return map;
        }
        public Dictionary<string, int> getIndicesProveedores()
        {
            Dictionary<string, int> map = new Dictionary<string, int>();

            string query = "SELECT * FROM proveedores";
            SQLiteCommand command = new SQLiteCommand(query, connection);
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                map.Add(result.GetString(1), result.GetInt32(0));
            }
            return map;
        }
        public DataTable getProductos(string offset)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT productos.id AS ID, productos.codigo AS CODIGO, productos.nombre AS NOMBRE, categorias.nombre AS CATEGORIA, medidas.nombre AS MEDIDA, productos.presentacion AS PRESENTACION, productos.iva AS IVA," +
                " productos.menudeo AS MENUDEO, productos.mayoreo AS MAYOREO, productos.cantidad_mayoreo AS 'CANTIDAD DE MAYOREO', productos.especial AS ESPECIAL, productos.vendedor AS VENDEDOR" +
                " FROM productos INNER JOIN categorias ON productos.categoria_id = categorias.id INNER JOIN medidas ON productos.medida_id = medidas.id WHERE productos.activo=1 LIMIT 19 OFFSET @setOffset";
            command.Parameters.AddWithValue("setOffset", offset);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
        public DataTable getProductos(string offset, string arg)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT productos.id AS ID, productos.codigo AS CODIGO, productos.nombre AS NOMBRE, categorias.nombre AS CATEGORIA, medidas.nombre AS MEDIDA, productos.presentacion AS PRESENTACION, productos.iva AS IVA," +
                " productos.menudeo AS MENUDEO, productos.mayoreo AS MAYOREO, productos.cantidad_mayoreo AS 'CANTIDAD DE MAYOREO', productos.especial AS ESPECIAL, productos.vendedor AS VENDEDOR" +
                " FROM productos INNER JOIN categorias ON productos.categoria_id = categorias.id INNER JOIN medidas ON productos.medida_id = medidas.id WHERE productos.activo=1 " + arg + " LIMIT 19 OFFSET @setOffset";
            command.Parameters.AddWithValue("setOffset", offset);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
        public int getProductosRowCount(string arg)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) AS RowCnt FROM productos WHERE activo=1 " + arg;
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                int count = result.GetInt32(0);
                return count;
            }
            return 0;
        }
        public int getProductosRowCount(string arg, string arg2)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) AS RowCnt FROM productos WHERE activo=1 AND (UPPER(productos.codigo) LIKE @setCodigo OR UPPER(productos.nombre) LIKE @setNombre) " + arg;
            command.Parameters.AddWithValue("setCodigo", "%" + arg2 + "%");
            command.Parameters.AddWithValue("setNombre", "%" + arg2 + "%");
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                int count = result.GetInt32(0);
                return count;
            }
            return 0;
        }
        public DataTable getProductos(string offset, string arg, string arg2)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT productos.id AS ID, productos.codigo AS CODIGO, productos.nombre AS NOMBRE, categorias.nombre AS CATEGORIA, medidas.nombre AS MEDIDA, productos.presentacion AS PRESENTACION, productos.iva AS IVA," +
                " productos.menudeo AS MENUDEO, productos.mayoreo AS MAYOREO, productos.cantidad_mayoreo AS 'CANTIDAD DE MAYOREO', productos.especial AS ESPECIAL, productos.vendedor AS VENDEDOR, productos.imagen AS IMAGEN" +
                " FROM productos INNER JOIN categorias ON productos.categoria_id = categorias.id INNER JOIN medidas ON productos.medida_id = medidas.id WHERE productos.activo=1 AND (UPPER(productos.codigo) LIKE @setCodigo OR UPPER(productos.nombre) LIKE @setNombre) " + arg + " LIMIT 19 OFFSET @setOffset";
            command.Parameters.AddWithValue("setOffset", offset);
            command.Parameters.AddWithValue("setCodigo", "%" + arg2 + "%");
            command.Parameters.AddWithValue("setNombre", "%" + arg2 + "%");
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }

        public DataTable getOperaciones()
        {
            DataTable dt = new DataTable();
            string query = "SELECT * FROM operaciones";
            SQLiteCommand command = new SQLiteCommand(query, connection);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
        public DataTable getSucursales()
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id AS ID, razon_social AS RAZON_SOCIAL, correo AS CORREO, puerta_enlace1 AS PUERTA_DE_ENLACE_1, puerta_enlace2 AS PUERTA_DE_ENLACE_2," +
                " puerta_enlace3 AS PUERTA_DE_ENLACE_3, puerta_enlace4 AS PUERTA_DE_ENLACE_4 FROM sucursales WHERE activo = 1";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
        public DataTable getSucursales(string arg)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id AS ID, razon_social AS RAZON_SOCIAL, correo AS CORREO, puerta_enlace1 AS PUERTA_DE_ENLACE_1, puerta_enlace2 AS PUERTA_DE_ENLACE_2," +
                " puerta_enlace3 AS PUERTA_DE_ENLACE_3, puerta_enlace4 AS PUERTA_DE_ENLACE_4 FROM sucursales WHERE activo = 1 AND razon_social LIKE @setRazon";
            command.Parameters.AddWithValue("setRazon", "%" + arg + "%");
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
        public Usuario getLoginUser(string usuario, string contraseña)
        {
            Usuario usr = null;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM usuarios WHERE usuario = @setUsuario AND clave = @setClave AND activo = 1";
            command.Parameters.AddWithValue("setUsuario", usuario);
            command.Parameters.AddWithValue("setClave", contraseña);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                usr = new Usuario();
                usr.id = result.GetInt32(0);
                usr.nombre = result.GetString(1);
                usr.correo = result.GetString(2);
                usr.telefono = result.GetString(4);
                usr.imagen = result.IsDBNull(5) ? "" : result.GetString(5);
                usr.usuario = result.GetString(6);
                usr.clave = result.GetString(7);
                usr.es_raiz = result.GetInt32(8);
            }
            return usr;
        }
        public string getNombreUsuario(int id)
        {
            string nombre = "";
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT nombre FROM usuarios WHERE id = @setId";
            command.Parameters.AddWithValue("setId", id);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                nombre = result.GetString(0);
            }
            return nombre;
        }
        public void limpiarVentas()
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "DELETE FROM producto_venta";
            command.ExecuteNonQuery();
            command.CommandText = "DELETE FROM ventas";
            command.ExecuteNonQuery();
        }
        public void ClearDatabase()
        {

            SQLiteCommand command = connection.CreateCommand();

            
            command.CommandText = "DELETE FROM operaciones";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM producto_venta";
            command.ExecuteScalar();      
            command.CommandText = "DELETE FROM producto_entrada";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM alta_temporal";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM proveedores";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM cortes";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM apartados_temporal";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM creditos_temporal";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM clientes_temporal";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM abonos_apartado_temporal";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM abonos_apartado";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM abonos_credito_temporal";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM abonos_credito";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM apartados";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM creditos";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM clientes";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM categorias";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM medidas";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM productos";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM usuarios";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM sucursales";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM ventas";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM entradas";
            command.ExecuteScalar();
            command.CommandText = "DELETE FROM salidas_temporal";
            command.ExecuteScalar();
        }
        public DataTable getCategorias()
        {
            DataTable dt = new DataTable();
            string query = "SELECT id AS ID, nombre AS CATEGORIA FROM categorias WHERE activo=1";
            SQLiteCommand command = new SQLiteCommand(query, connection);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
        public DataTable getCategorias(string arg)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id AS ID, nombre AS CATEGORIA FROM categorias WHERE activo=1 AND nombre LIKE @setNombre";
            command.Parameters.AddWithValue("setNombre", "%" + arg + "%");
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }

        // Agregar este método en LocaldataManager.cs si no lo tienes ya
        public (bool tieneDescuento, decimal porcentajeDescuento) GetDescuentoCategoria(int categoriaId)
        {
            try
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT isdescuento, descuento FROM categorias WHERE id = @categoriaId AND activo = 1";
                command.Parameters.AddWithValue("@categoriaId", categoriaId);

                SQLiteDataReader result = command.ExecuteReader();
                if (result.Read())
                {
                    bool tiene = result.GetInt32(0) == 1;
                    decimal descuentoDecimal = result.IsDBNull(1) ? 0 : result.GetDecimal(1);

                    Console.WriteLine($"POS - Descuento categoría {categoriaId}: tiene={tiene}, porcentaje={descuentoDecimal}");

                    return (tiene, descuentoDecimal);
                }
                return (false, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"POS - Error al obtener descuento de categoría {categoriaId}: {ex.Message}");
                return (false, 0);
            }
        }

        public DataTable getVentasPendientes()
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT ventas.total AS total, ventas.descuento AS descuento, ventas.folio AS folio, ventas.folio_corte AS folio_corte, ventas.fecha_venta AS 'fecha_venta', ventas.metodo_pago AS metodo_pago, ventas.tipo AS tipo, ventas.sucursal_id AS sucursal_id, ventas.usuario_id AS usuario_id, ventas.id AS id FROM ventas";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }

        public DataTable getVentasFecha(string fecha)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT ventas.id AS ID, ventas.total AS TOTAL, ventas.descuento AS DESC, ventas.folio AS FOLIO, ventas.fecha_venta AS 'FECHA DE VENTA', usuarios.nombre AS CAJERO, ventas.metodo_pago AS PAGOS " +
                                  "FROM ventas INNER JOIN usuarios ON ventas.usuario_id = usuarios.id " +
                                  "WHERE SUBSTR(ventas.fecha_venta, 1, 10) = @fecha " +
                                  "ORDER BY ventas.fecha_venta DESC";
            command.Parameters.AddWithValue("@fecha", fecha);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }

        public int GetCliente(string nombre)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id FROM clientes WHERE nombre = @nombre";
            command.Parameters.AddWithValue("@nombre", nombre);

            int clienteId = -1;  // Valor por defecto en caso de no encontrar el cliente
            using (SQLiteDataReader result = command.ExecuteReader())
            {
                if (result.Read())
                {
                    clienteId = result.GetInt32(0);
                }
            }
            return clienteId;
        }

        /*public DataTable GetCreditosDataTable()
        {
            DataTable creditosTable = new DataTable();

            creditosTable.Columns.Add("Folio", typeof(string));
            creditosTable.Columns.Add("Cliente", typeof(string));
            creditosTable.Columns.Add("Total", typeof(double));
            creditosTable.Columns.Add("Pagado", typeof(double));
            creditosTable.Columns.Add("Fecha", typeof(string));
            creditosTable.Columns.Add("Estado", typeof(string));
            creditosTable.Columns.Add("Orden", typeof(int));  
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT folio, cliente_creditos_id, total, total_pagado, fecha_de_credito, estado FROM creditos";

            using (SQLiteDataReader result = command.ExecuteReader())
            {
                while (result.Read())
                {
                    string folio = result.GetString(0);
                    int clienteId = result.GetInt32(1);
                    double total = result.GetDouble(2);
                    double totalPagado = result.GetDouble(3);
                    string fechaCredito = result.GetString(4);
                    int estado = result.GetInt32(5);
                    string estadoString = GetEstadoString(estado);
                    int orden = GetEstadoOrden(estado);

                    string clienteNombre = GetClienteNombre(clienteId);

                    creditosTable.Rows.Add(folio, clienteNombre, total, totalPagado, fechaCredito, estadoString, orden);
                }
            }

            DataView dv = creditosTable.DefaultView;
            dv.Sort = "Orden ASC";
            DataTable sortedTable = dv.ToTable();

            sortedTable.Columns.Remove("Orden");

            return sortedTable;
        }*/
        public DataTable GetCreditosDataTable(int idSucursal)
        {
            DataTable creditosTable = new DataTable();

            creditosTable.Columns.Add("Folio", typeof(string));
            creditosTable.Columns.Add("Cliente", typeof(string));
            creditosTable.Columns.Add("Total", typeof(double));
            creditosTable.Columns.Add("Pagado", typeof(double));
            creditosTable.Columns.Add("Fecha", typeof(string));
            creditosTable.Columns.Add("Estado", typeof(string));
            creditosTable.Columns.Add("Orden", typeof(int));
            creditosTable.Columns.Add("Sucursal", typeof(string));

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT folio, cliente_creditos_id, total, total_pagado, fecha_de_credito, estado FROM creditos";

            using (SQLiteDataReader result = command.ExecuteReader())
            {
                while (result.Read())
                {
                    string folio = result.GetString(0);
                    int clienteId = result.GetInt32(1);
                    double total = result.GetDouble(2);
                    double totalPagado = result.GetDouble(3);
                    string fechaCredito = result.GetString(4);
                    int estado = result.GetInt32(5);

                    string estadoString = GetEstadoString(estado);
                    int orden = GetEstadoOrden(estado);

                    string clienteNombre = GetClienteNombre(clienteId);
                    int sucursalId = ExtractSucursalIdFromFolio(folio);

                    if (sucursalId == idSucursal)
                    {
                        string razonSocial = GetRazonSocial(sucursalId);
                        creditosTable.Rows.Add(folio, clienteNombre, total, totalPagado, fechaCredito, estadoString, orden, razonSocial);
                    }
                }
            }

            DataView dv = creditosTable.DefaultView;
            dv.Sort = "Orden ASC";
            DataTable sortedTable = dv.ToTable();

            sortedTable.Columns.Remove("Orden");

            return sortedTable;
        }
        public bool CreditoExiste(string folio)
        {
            // Implementa la lógica para verificar en la base de datos si ya existe un crédito con el mismo folio
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM creditos WHERE folio = @folio";
            command.Parameters.AddWithValue("@folio", folio);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
        }
        public bool ApartadoExiste(string folio)
        {
            // Crear comando SQL para verificar si ya existe un apartado con el mismo folio
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM apartados WHERE folio_corte = @folio";
            command.Parameters.AddWithValue("@folio", folio);

            // Ejecutar el comando y obtener el conteo
            int count = Convert.ToInt32(command.ExecuteScalar());

            // Retornar true si el apartado existe, de lo contrario false
            return count > 0;
        }

        //Metodo anterior, filtra deacuerdo al folio de la sucursal y no al de la sucrusal id, ESTA MAL
        /*
        public DataTable GetApartadosDataTable(int idSucursal)
        {
            DataTable apartadosTable = new DataTable();

            apartadosTable.Columns.Add("Folio", typeof(string));
            apartadosTable.Columns.Add("Cliente", typeof(string));
            apartadosTable.Columns.Add("Total", typeof(double));
            apartadosTable.Columns.Add("Pagado", typeof(double));
            apartadosTable.Columns.Add("Fecha", typeof(string));
            apartadosTable.Columns.Add("Estado", typeof(string));
            apartadosTable.Columns.Add("Orden", typeof(int));
            apartadosTable.Columns.Add("Sucursal", typeof(string));

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT folio_corte, cliente_creditos_id, total, total_pagado, fecha_apartado, estado FROM apartados";

            using (SQLiteDataReader result = command.ExecuteReader())
            {
                while (result.Read())
                {
                    string folio = result.GetString(0);
                    int clienteId = result.GetInt32(1);
                    double total = result.GetDouble(2);
                    double totalPagado = result.GetDouble(3);
                    string fechaApartado = result.GetString(4);
                    int estado = result.GetInt32(5);

                    string estadoString = GetEstadoString(estado);
                    int orden = GetEstadoOrden(estado);

                    string clienteNombre = GetClienteNombre(clienteId);
                    int sucursalId = ExtractSucursalIdFromFolio(folio);
                    if (sucursalId == idSucursal)
                    {
                        string razonSocial = GetRazonSocial(sucursalId);
                        apartadosTable.Rows.Add(folio, clienteNombre, total, totalPagado, fechaApartado, estadoString, orden, razonSocial);
                    }
                }
            }

            DataView dv = apartadosTable.DefaultView;
            dv.Sort = "Orden ASC";
            DataTable sortedTable = dv.ToTable();

            sortedTable.Columns.Remove("Orden");

            // Imprimir en consola cada fila
            foreach (DataRow row in sortedTable.Rows)
            {
                Console.WriteLine($"Folio: {row["Folio"]}, Cliente: {row["Cliente"]}, Total: {row["Total"]}, Pagado: {row["Pagado"]}, Fecha: {row["Fecha"]}, Estado: {row["Estado"]}, Sucursal: {row["Sucursal"]}");
            }

            return sortedTable;
        }
        */

        // metodo original, se usaba en generar excel, no muestra productos en excel pero funciona bien.
        public DataTable GetApartadosDataTable(int idSucursal)
        {
            DataTable apartadosTable = new DataTable();

            apartadosTable.Columns.Add("Folio", typeof(string));
            apartadosTable.Columns.Add("Cliente", typeof(string));
            apartadosTable.Columns.Add("Total", typeof(double));
            apartadosTable.Columns.Add("Pagado", typeof(double));
            apartadosTable.Columns.Add("Fecha", typeof(string));
            apartadosTable.Columns.Add("Estado", typeof(string));
            apartadosTable.Columns.Add("Orden", typeof(int));
            apartadosTable.Columns.Add("Sucursal", typeof(string));

            SQLiteCommand command = connection.CreateCommand();

            // Ajustar la consulta para filtrar directamente por el idSucursal.
            command.CommandText = "SELECT folio_corte, cliente_creditos_id, total, total_pagado, fecha_apartado, estado " +
                                  "FROM apartados WHERE sucursal_id = @idSucursal";
            command.Parameters.AddWithValue("@idSucursal", idSucursal);

            using (SQLiteDataReader result = command.ExecuteReader())
            {
                while (result.Read())
                {
                    string folio = result.GetString(0);
                    int clienteId = result.GetInt32(1);
                    double total = result.GetDouble(2);
                    double totalPagado = result.GetDouble(3);
                    string fechaApartado = result.GetString(4);
                    int estado = result.GetInt32(5);

                    string estadoString = GetEstadoString(estado);
                    int orden = GetEstadoOrden(estado);

                    string clienteNombre = GetClienteNombre(clienteId);
                    string razonSocial = GetRazonSocial(idSucursal);

                    // Añadir fila solo si el idSucursal coincide (aunque ya lo estamos filtrando en SQL)
                    apartadosTable.Rows.Add(folio, clienteNombre, total, totalPagado, fechaApartado, estadoString, orden, razonSocial);
                }
            }

            DataView dv = apartadosTable.DefaultView;
            dv.Sort = "Orden ASC";
            DataTable sortedTable = dv.ToTable();

            sortedTable.Columns.Remove("Orden");

            Console.WriteLine(idSucursal);
            // Imprimir en consola para depuración
            foreach (DataRow row in sortedTable.Rows)
            {
                Console.WriteLine($"Folio: {row["Folio"]}, Cliente: {row["Cliente"]}, Total: {row["Total"]}, Pagado: {row["Pagado"]}, Fecha: {row["Fecha"]}, Estado: {row["Estado"]}, Sucursal: {row["Sucursal"]}");
            }

            return sortedTable;
        }

        /* Método para obtener los apartados en formato DataTable para exportar a Excel, lo exporta en json, no es el metodo original.
        public DataTable GetApartadosExcelDataTable2(int idSucursal)
        {
            DataTable apartadosTable = new DataTable();

            apartadosTable.Columns.Add("Folio", typeof(string));
            apartadosTable.Columns.Add("Cliente", typeof(string));
            apartadosTable.Columns.Add("Total", typeof(double));
            apartadosTable.Columns.Add("Pagado", typeof(double));
            apartadosTable.Columns.Add("Fecha", typeof(string));
            apartadosTable.Columns.Add("Estado", typeof(string));
            apartadosTable.Columns.Add("Orden", typeof(int));
            apartadosTable.Columns.Add("Sucursal", typeof(string));
            apartadosTable.Columns.Add("Productos", typeof(string));
            SQLiteCommand command = connection.CreateCommand();

            // Ajustar la consulta para filtrar directamente por el idSucursal.
            command.CommandText = "SELECT folio_corte, cliente_creditos_id, total, total_pagado, fecha_apartado, estado, productos " +
                                  "FROM apartados WHERE sucursal_id = @idSucursal";
            command.Parameters.AddWithValue("@idSucursal", idSucursal);

            using (SQLiteDataReader result = command.ExecuteReader())
            {
                while (result.Read())
                {
                    string folio = result.GetString(0);
                    int clienteId = result.GetInt32(1);
                    double total = result.GetDouble(2);
                    double totalPagado = result.GetDouble(3);
                    string fechaApartado = result.GetString(4);
                    int estado = result.GetInt32(5);
                    string productos = result.GetString(6);

                    string estadoString = GetEstadoString(estado);
                    int orden = GetEstadoOrden(estado);

                    string clienteNombre = GetClienteNombre(clienteId);
                    string razonSocial = GetRazonSocial(idSucursal);

                    // Añadir fila solo si el idSucursal coincide (aunque ya lo estamos filtrando en SQL)
                    apartadosTable.Rows.Add(folio, clienteNombre, total, totalPagado, fechaApartado, estadoString, orden, razonSocial, productos);
                }
            }

            DataView dv = apartadosTable.DefaultView;
            dv.Sort = "Orden ASC";
            DataTable sortedTable = dv.ToTable();

            sortedTable.Columns.Remove("Orden");

            Console.WriteLine(idSucursal);
            // Imprimir en consola para depuración
            foreach (DataRow row in sortedTable.Rows)
            {
                Console.WriteLine($"Folio: {row["Folio"]}, Cliente: {row["Cliente"]}, Total: {row["Total"]}," +
                    $" Pagado: {row["Pagado"]}, Fecha: {row["Fecha"]}, Estado: {row["Estado"]}," +
                    $" Sucursal: {row["Sucursal"]}, Productos: {row["Productos"]}");
            }

            return sortedTable;
        }
        */


        // si hay problemas despues, quitar desde este punto y dejar el codigo (GetApartadosDataTable) anterior en
        // el metodo para generar excel.
        public DataTable GetApartadosExcelDataTable(int idSucursal)
        {
            DataTable apartadosTable = new DataTable();

            apartadosTable.Columns.Add("Folio", typeof(string));
            apartadosTable.Columns.Add("Cliente", typeof(string));
            apartadosTable.Columns.Add("Total", typeof(double));
            apartadosTable.Columns.Add("Pagado", typeof(double));
            apartadosTable.Columns.Add("Fecha", typeof(string));
            apartadosTable.Columns.Add("Estado", typeof(string));
            apartadosTable.Columns.Add("Sucursal", typeof(string));
            apartadosTable.Columns.Add("Productos", typeof(string));

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT folio_corte, cliente_creditos_id, total, total_pagado, fecha_apartado, estado, productos " +
                                  "FROM apartados WHERE sucursal_id = @idSucursal";
            command.Parameters.AddWithValue("@idSucursal", idSucursal);

            using (SQLiteDataReader result = command.ExecuteReader())
            {
                while (result.Read())
                {
                    string folio = result.GetString(0);
                    int clienteId = result.GetInt32(1);
                    double total = result.GetDouble(2);
                    double totalPagado = result.GetDouble(3);
                    string fechaApartado = result.GetString(4);
                    int estado = result.GetInt32(5);
                    string productosJson = result.GetString(6);

                    string estadoString = GetEstadoString(estado);
                    string clienteNombre = GetClienteNombre(clienteId);
                    string razonSocial = GetRazonSocial(idSucursal);

                    // Convertir JSON a formato de texto
                    string productosTexto = ConvertirProductos(productosJson);

                    // Añadir fila a la tabla
                    apartadosTable.Rows.Add(folio, clienteNombre, total, totalPagado, fechaApartado, estadoString, razonSocial, productosTexto);
                }
            }

            DataView dv = apartadosTable.DefaultView;
            dv.Sort = "Folio ASC"; // Ordenar si es necesario
            DataTable sortedTable = dv.ToTable();

            Console.WriteLine(idSucursal);
            foreach (DataRow row in sortedTable.Rows)
            {
                Console.WriteLine($"Folio: {row["Folio"]}, Cliente: {row["Cliente"]}, Total: {row["Total"]}, Pagado: {row["Pagado"]}, Fecha: {row["Fecha"]}, Estado: {row["Estado"]}, Sucursal: {row["Sucursal"]}, Productos: {row["Productos"]}");
            }

            return sortedTable;
        }

        // Método para convertir JSON de productos a texto plano
        private string ConvertirProductos(string productosJson)
        {
            try
            {
                var productos = System.Text.Json.JsonSerializer.Deserialize<List<productosJSON>>(productosJson);
                if (productos != null)
                {
                    // Concatenamos solo el nombre del producto y la cantidad
                    return string.Join("; ", productos.Select(p => $"{p.Nombre} (Cantidad: {p.Cantidad})"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al convertir productos: {ex.Message}");
            }
            return "Error al procesar productos";
        }


        // Clase productosJSON para deserializar el JSON
        public class productosJSON
        {
            [JsonPropertyName("nombre")]
            public string Nombre { get; set; }

            [JsonPropertyName("cantidad")]
            public int Cantidad { get; set; }
            // eliminar hasta aqui.
        }

        private int ExtractSucursalIdFromFolio(string folio)
        {
            return int.Parse(folio.Substring(0, 2));
        }

        private string GetRazonSocial(int sucursalId)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT razon_social FROM sucursales WHERE id = @Id";
            command.Parameters.AddWithValue("@Id", sucursalId);

            string razonSocial = string.Empty;
            using (SQLiteDataReader result = command.ExecuteReader())
            {
                if (result.Read())
                {
                    razonSocial = result.GetString(0);
                }
            }

            return razonSocial;
        }

        private string GetClienteNombre(int clienteId)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT nombre FROM Clientes WHERE id = @clienteId";
            command.Parameters.AddWithValue("@clienteId", clienteId);

            string nombre = "";
            using (SQLiteDataReader result = command.ExecuteReader())
            {
                if (result.Read())
                {
                    nombre = result.GetString(0);
                }
            }
            return nombre;
        }

        private string GetEstadoString(int estado)
        {
            string[] estados = { "PENDIENTE", "EXPIRO", "CANCELADO", "PAGADO", "ENTREGADO" };
            if (estado >= 0 && estado < estados.Length)
            {
                return estados[estado];
            }
            return "DESCONOCIDO";
        }

        private int GetEstadoOrden(int estado)
        {
            switch (estado)
            {
                case 1: return 0;  // Expiro
                case 0: return 1;  // Pendiente
                case 3: return 2;  // Pagado
                case 4: return 3;  // Entregado
                case 2: return 4;  // Cancelado
                default: return 5; // Desconocido
            }
        }

        public Dictionary<string, List<Apartado>> getApartadosCliente(int idCliente, int idSucursal)
        {
            Dictionary<string, List<Apartado>> apartados = new Dictionary<string, List<Apartado>>();
            string[] range = { "PENDIENTE", "EXPIRO", "CANCELADO", "PAGADO", "ENTREGADO" };
            apartados[range[0]] = new List<Apartado>();
            apartados[range[1]] = new List<Apartado>();
            apartados[range[2]] = new List<Apartado>();
            apartados[range[3]] = new List<Apartado>();
            apartados[range[4]] = new List<Apartado>();

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM apartados WHERE cliente_creditos_id = @setId AND sucursal_id = @setSucursalId";
            command.Parameters.AddWithValue("setId", idCliente);
            command.Parameters.AddWithValue("setSucursalId", idSucursal);
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                Apartado apartado = new Apartado
                {
                    id = result.GetInt32(0),
                    productos = result.GetString(1),
                    total = result.GetDouble(2),
                    total_pagado = result.GetDouble(3),
                    fecha_de_apartado = result.GetString(4),
                    folio_corte = result.GetString(5),
                    fecha_entrega = result.IsDBNull(6) ? "" : result.GetString(6),
                    estado = result.GetInt32(7),
                    cliente_creditos_id = result.GetInt32(8),
                    id_cajero_registro = result.GetInt32(9),
                    id_cajero_entrega = result.IsDBNull(10) ? "" : result.GetInt32(10).ToString(),
                    sucursal_id = result.GetInt32(11),
                    observaciones = result.GetString(12),
                };
                apartado.abonos = new List<AbonoApartado>();
                SQLiteCommand subcommand = connection.CreateCommand();
                subcommand.CommandText = "SELECT * FROM abonos_apartado WHERE id_apartado = @setIdc";
                subcommand.Parameters.AddWithValue("setIdc", apartado.id);
                SQLiteDataReader subresult = subcommand.ExecuteReader();
                while (subresult.Read())
                {
                    AbonoApartado abono = new AbonoApartado
                    {
                        id = subresult.GetInt32(0),
                        folio = subresult.GetString(1),
                        metodo_pago = subresult.GetString(2),
                        total_abonado = subresult.GetDouble(3),
                        fecha = subresult.GetString(4),
                        apartado_id = subresult.GetInt32(5),
                        folio_corte = subresult.GetString(6),
                        usuario_id = subresult.GetInt32(7)
                    };
                    apartado.abonos.Add(abono);
                }
                subcommand.Reset();
                subcommand.CommandText = "SELECT * FROM abonos_apartado_temporal WHERE id_apartado = @setIdc";
                subcommand.Parameters.AddWithValue("setIdc", apartado.id);
                subresult = subcommand.ExecuteReader();
                while (subresult.Read())
                {
                    AbonoApartado abono = new AbonoApartado
                    {
                        id = 0,
                        folio = subresult.GetString(1),
                        metodo_pago = subresult.GetString(2),
                        total_abonado = subresult.GetDouble(3),
                        fecha = subresult.GetString(4),
                        folio_apartado = subresult.GetString(5),
                        apartado_id = subresult.GetInt32(6),
                        folio_corte = subresult.GetString(7),
                        usuario_id = subresult.GetInt32(8)
                    };
                    apartado.abonos.Add(abono);
                    apartado.total_pagado += abono.total_abonado;
                    if (apartado.total_pagado >= apartado.total)
                    {
                        apartado.estado = 3;
                    }
                }
                apartados[range[apartado.estado]].Add(apartado);
            }
            command.Reset();
            command.CommandText = "SELECT * FROM apartados_temporal WHERE cliente_creditos_id = @setId AND sucursal_id = @setSucursalId";
            command.Parameters.AddWithValue("setId", idCliente);
            command.Parameters.AddWithValue("setSucursalId", idSucursal);
            result = command.ExecuteReader();
            while (result.Read())
            {
                Apartado apartado = new Apartado
                {
                    id = 0,
                    productos = result.GetString(1),
                    total = result.GetDouble(2),
                    total_pagado = result.GetDouble(3),
                    fecha_de_apartado = result.GetString(4),
                    folio_corte = result.GetString(5),
                    fecha_entrega = result.IsDBNull(6) ? "" : result.GetString(6),
                    estado = result.GetInt32(7),
                    cliente_creditos_id = result.GetInt32(8),
                    id_cajero_registro = result.GetInt32(9),
                    id_cajero_entrega = result.IsDBNull(10) ? "" : result.GetInt32(10).ToString(),
                    sucursal_id = result.GetInt32(11),
                    temporal = result.GetInt32(12),
                    observaciones = result.GetString(13),
                };
                apartado.abonos = new List<AbonoApartado>();
                SQLiteCommand subcommand = connection.CreateCommand();
                subcommand.CommandText = "SELECT * FROM abonos_apartado_temporal WHERE folio_apartado = @setFolioc";
                subcommand.Parameters.AddWithValue("setFolioc", apartado.folio_corte);
                SQLiteDataReader subresult = subcommand.ExecuteReader();
                while (subresult.Read())
                {
                    AbonoApartado abono = new AbonoApartado
                    {
                        id = 0,
                        folio = subresult.GetString(1),
                        metodo_pago = subresult.GetString(2),
                        total_abonado = subresult.GetDouble(3),
                        fecha = subresult.GetString(4),
                        folio_apartado = subresult.GetString(5),
                        apartado_id = subresult.GetInt32(6),
                        folio_corte = subresult.GetString(7),
                        usuario_id = subresult.GetInt32(8)
                    };
                    apartado.abonos.Add(abono);
                    apartado.total_pagado += abono.total_abonado;
                    if (apartado.total_pagado >= apartado.total)
                    {
                        apartado.estado = 3;
                    }
                }
                apartados[range[apartado.estado]].Add(apartado);
            }
            return apartados;
        }

        public Dictionary<string, List<Credito>> getCreditosCliente(int idCliente, int idSucursal)
        {
            Dictionary<string, List<Credito>> creditos = new Dictionary<string, List<Credito>>();
            string[] range = { "PENDIENTE", "EXPIRO", "CANCELADO", "PAGADO" };
            creditos[range[0]] = new List<Credito>();
            creditos[range[1]] = new List<Credito>();
            creditos[range[2]] = new List<Credito>();
            creditos[range[3]] = new List<Credito>();

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM creditos WHERE cliente_creditos_id = @setId AND sucursal_id = @setSucursalId";
            command.Parameters.AddWithValue("setId", idCliente);
            command.Parameters.AddWithValue("setSucursalId", idSucursal);
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                Credito credito = new Credito
                {
                    id = result.GetInt32(0),
                    productos = result.GetString(1),
                    total = result.GetDouble(2),
                    total_pagado = result.GetDouble(3),
                    fecha_de_credito = result.GetString(4),
                    folio = result.GetString(5),
                    estado = result.GetInt32(6),
                    cliente_creditos_id = result.GetInt32(7),
                    id_cajero_registro = result.GetInt32(8),
                    sucursal_id = result.GetInt32(9),
                    observaciones = result.GetString(10),
                };
                credito.abonos = new List<AbonoCredito>();
                SQLiteCommand subcommand = connection.CreateCommand();
                subcommand.CommandText = "SELECT * FROM abonos_credito WHERE id_credito = @setIdc";
                subcommand.Parameters.AddWithValue("setIdc", credito.id);
                SQLiteDataReader subresult = subcommand.ExecuteReader();
                while (subresult.Read())
                {
                    AbonoCredito abono = new AbonoCredito
                    {
                        id = subresult.GetInt32(0),
                        folio = subresult.GetString(1),
                        metodo_pago = subresult.GetString(2),
                        total_abonado = subresult.GetDouble(3),
                        fecha = subresult.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss"),
                        credito_id = subresult.GetInt32(5),
                        folio_corte = subresult.GetString(6),
                        usuario_id = subresult.GetInt32(7)
                    };
                    credito.abonos.Add(abono);
                }
                subcommand.Reset();
                subcommand.CommandText = "SELECT * FROM abonos_credito_temporal WHERE id_credito = @setIdc";
                subcommand.Parameters.AddWithValue("setIdc", credito.id);
                subresult = subcommand.ExecuteReader();
                while (subresult.Read())
                {
                    AbonoCredito abono = new AbonoCredito();
                    try
                    {
                        abono.id = 0;
                        abono.folio = subresult["folio"] != DBNull.Value ? subresult["folio"].ToString() : string.Empty;
                        abono.metodo_pago = subresult["metodo_pago"] != DBNull.Value ? subresult["metodo_pago"].ToString() : string.Empty;
                        abono.total_abonado = subresult["total_abonado"] != DBNull.Value ? Convert.ToDouble(subresult["total_abonado"]) : 0.0;
                        if (subresult["fecha"] != DBNull.Value)
                        {
                            DateTime fechaValue;
                            if (DateTime.TryParse(subresult["fecha"].ToString(), out fechaValue))
                            {
                                abono.fecha = fechaValue.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else
                            {
                                abono.fecha = string.Empty;
                            }
                        }
                        else
                        {
                            abono.fecha = string.Empty;
                        }
                        abono.folio_credito = subresult["folio_credito"] != DBNull.Value ? subresult["folio_credito"].ToString() : string.Empty;
                        abono.credito_id = subresult["credito_id"] != DBNull.Value ? Convert.ToInt32(subresult["credito_id"]) : 0;
                        abono.folio_corte = subresult["folio_corte"] != DBNull.Value ? subresult["folio_corte"].ToString() : string.Empty;
                        abono.usuario_id = subresult["usuario_id"] != DBNull.Value ? Convert.ToInt32(subresult["usuario_id"]) : 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al procesar el registro: {ex.Message}");
                    }
                    credito.abonos.Add(abono);
                    credito.total_pagado += abono.total_abonado;
                    if (credito.total_pagado >= credito.total)
                    {
                        credito.estado = 3;
                    }
                }
                creditos[range[credito.estado]].Add(credito);
            }
            command.Reset();
            command.CommandText = "SELECT * FROM creditos_temporal WHERE cliente_creditos_id = @setId AND sucursal_id = @setSucursalId";
            command.Parameters.AddWithValue("setId", idCliente);
            command.Parameters.AddWithValue("setSucursalId", idSucursal);
            result = command.ExecuteReader();
            while (result.Read())
            {
                Credito credito = new Credito
                {
                    id = 0,
                    productos = result.GetString(1),
                    total = result.GetDouble(2),
                    total_pagado = result.GetDouble(3),
                    fecha_de_credito = result.GetString(4),
                    folio = result.GetString(5),
                    estado = result.GetInt32(6),
                    cliente_creditos_id = result.GetInt32(7),
                    id_cajero_registro = result.GetInt32(8),
                    sucursal_id = result.GetInt32(9),
                    temporal = result.GetInt32(10),
                    observaciones = result.GetString(11),
                };
                credito.abonos = new List<AbonoCredito>();
                SQLiteCommand subcommand = connection.CreateCommand();
                subcommand.CommandText = "SELECT * FROM abonos_credito_temporal WHERE folio_credito = @setFolioc";
                subcommand.Parameters.AddWithValue("setFolioc", credito.folio);
                SQLiteDataReader subresult = subcommand.ExecuteReader();
                while (subresult.Read())
                {
                    AbonoCredito abono = new AbonoCredito
                    {
                        id = 0,
                        folio = subresult.GetString(1),
                        metodo_pago = subresult.GetString(2),
                        total_abonado = subresult.GetDouble(3),
                        fecha = subresult.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss"),
                        folio_credito = subresult.GetString(5),
                        credito_id = subresult.GetInt32(6),
                        folio_corte = subresult.GetString(7),
                        usuario_id = subresult.GetInt32(8)
                    };
                    credito.abonos.Add(abono);
                    credito.total_pagado += abono.total_abonado;
                    if (credito.total_pagado >= credito.total)
                    {
                        credito.estado = 3;
                    }
                }
                creditos[range[credito.estado]].Add(credito);
            }
            return creditos;
        }


        public DataTable getVentas(string arg)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT ventas.id AS ID, ventas.total AS TOTAL, ventas.descuento AS DESCUENTO, ventas.folio AS FOLIO, ventas.fecha_venta AS FECHA_DE_VENTA, usuarios.nombre AS CAJERO, ventas.metodo_pago AS PAGOS FROM ventas INNER JOIN usuarios ON ventas.usuario_id=usuarios.id WHERE folio LIKE @setFolio";
            command.Parameters.AddWithValue("setFolio", "%" + arg + "%");
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
        public DataTable getProveedores()
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM proveedores";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
        public DataTable getProveedores(string arg)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM proveedores WHERE nombre LIKE @setNombre";
            command.Parameters.AddWithValue("setNombre", "%" + arg + "%");
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
        public DataTable getProductoVentas(string id_venta)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT producto_venta.id AS ID, ventas.folio AS FOLIO, productos.codigo AS CODIGO, productos.nombre AS NOMBRE_PRODUCTO, producto_venta.cantidad AS CANTIDAD, producto_venta.precio_venta AS PRECIO_VENTA FROM producto_venta INNER JOIN ventas ON producto_venta.venta_id=ventas.id INNER JOIN productos ON producto_venta.producto_id=productos.id WHERE producto_venta.venta_id = @setId";
            command.Parameters.AddWithValue("setId", id_venta);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
        public List<ProductoVenta> getProductosVenta(string id_venta)
        {
            List<ProductoVenta> productos = new List<ProductoVenta>();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT producto_venta.producto_id, productos.nombre, producto_venta.codigo, producto_venta.cantidad, producto_venta.precio_venta FROM producto_venta INNER JOIN productos ON producto_venta.Producto_id = productos.id WHERE producto_venta.venta_id = @setVenta";
            command.Parameters.AddWithValue("setVenta", id_venta);
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                // NUEVO: Obtener el producto completo para comparar precios
                string codigo = result.GetString(2);
                Producto productoCompleto = GetProductByCode(codigo);
                double precioVenta = result.GetDouble(4);

                // NUEVO: Determinar si es precio especial comparando con el precio menudeo
                bool esPrecioEspecial = false;
                double precioOriginal = precioVenta;
                double descuentoUnitario = 0;

                if (productoCompleto != null)
                {
                    precioOriginal = productoCompleto.menudeo;
                    if (precioVenta < productoCompleto.menudeo)
                    {
                        esPrecioEspecial = true;
                        descuentoUnitario = productoCompleto.menudeo - precioVenta;
                    }
                }

                productos.Add(new ProductoVenta
                {
                    id = result.GetInt32(0),
                    nombre = result.GetString(1),
                    codigo = codigo,
                    cantidad = result.GetInt32(3),
                    precio_venta = precioVenta,
                    // NUEVO: Agregar los campos faltantes
                    precio_original = precioOriginal,
                    es_precio_especial = esPrecioEspecial,
                    descuento_unitario = descuentoUnitario
                });
            }
            return productos;
        }
        public DataTable getMedidas()
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id AS ID, nombre AS MEDIDA FROM medidas WHERE activo=1";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
        public DataTable getMedidas(string arg)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id AS ID, nombre AS MEDIDA FROM medidas WHERE activo=1 AND nombre LIKE @setNombre";
            command.Parameters.AddWithValue("setNombre", "%" + arg + "%");
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }

        public DataTable getUsuarios()
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id AS ID, nombre AS NOMBRE, correo AS CORREO, confirmacion AS CONFIRMACION, telefono AS TELEFONO, imagen AS FOTOGRAFIA, usuario as USUARIO, clave AS CLAVE," +
                "is_root AS NIVEL FROM usuarios WHERE activo=1 AND is_root > 0";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }

        public DataTable getUsuariosCajero()
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            //command.CommandText = "SELECT id AS ID, nombre AS NOMBRE, correo AS CORREO, confirmacion AS CONFIRMACION, telefono AS TELEFONO, imagen AS FOTOGRAFIA, usuario as USUARIO, clave AS CLAVE," +
              //  "is_root AS NIVEL FROM usuarios WHERE activo=1 AND is_root = 2";

            command.CommandText = "SELECT id AS ID, nombre AS NOMBRE, usuario as USUARIO, clave AS CLAVE, is_root AS NIVEL, telefono AS TELEFONO, correo AS CORREO" +
                " FROM usuarios WHERE activo=1 AND is_root = 2";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }

        // este metodo no busca, por eso se reemplazo con el metodo de abajo.
        public DataTable getUsuariosCajero2(string arg)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id AS ID, nombre AS NOMBRE, usuario as USUARIO, clave AS CLAVE, is_root AS NIVEL, telefono AS TELEFONO, correo AS CORREO" +
                " FROM usuarios WHERE activo=1 AND is_root = 2";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }
        public DataTable getUsuariosCajero(string searchTerm)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();

            // Comienza con la consulta base
            string query = "SELECT id AS ID, nombre AS NOMBRE, usuario as USUARIO, clave AS CLAVE, is_root AS NIVEL, telefono AS TELEFONO, correo AS CORREO " +
                           "FROM usuarios WHERE activo=1 AND is_root = 2";

            // Si searchTerm no está vacío, añade un filtro para buscar por usuario o nombre
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query += " AND (usuario LIKE @searchTerm OR nombre LIKE @searchTerm)";
            }

            command.CommandText = query;

            // Usamos parámetros para evitar inyecciones SQL
            command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");

            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }


        public DataTable getUsuarios(string arg)
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id AS ID, nombre AS NOMBRE, correo AS CORREO, confirmacion AS CONFIRMACION, telefono AS TELEFONO, imagen AS FOTOGRAFIA, usuario as USUARIO, clave AS CLAVE," +
                "is_root AS NIVEL FROM usuarios WHERE activo=1 AND is_root > 0 AND nombre LIKE @setNombre";
            command.Parameters.AddWithValue("setNombre", "%" + arg + "%");
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }

        public void saveMedidas(List<Medida> medidas)
        {
            foreach (Medida medida in medidas)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO medidas (id, nombre, activo, created_at, updated_at) " +
                "VALUES(@setId, @setNombre, @setActivo, @setCreated_at, @setUpdated_at) ";
                command.Parameters.AddWithValue("setId", medida.id);
                command.Parameters.AddWithValue("setNombre", medida.nombre);
                command.Parameters.AddWithValue("setActivo", medida.activo);
                command.Parameters.AddWithValue("setCreated_at", medida.created_at);
                command.Parameters.AddWithValue("setUpdated_at", medida.updated_at);
                command.ExecuteScalar();
            }
        }
        public void saveCategorias(List<Categoria> categorias)
        {
            foreach (Categoria categoria in categorias)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO categorias (id, nombre, activo, created_at, updated_at, isdescuento, descuento) " +
                "VALUES(@setId, @setNombre, @setActivo, @setCreated_at, @setUpdated_at, @setIsdescuento, @setDescuento) ";
                command.Parameters.AddWithValue("setId", categoria.id);
                command.Parameters.AddWithValue("setNombre", categoria.nombre);
                command.Parameters.AddWithValue("setActivo", categoria.activo);
                command.Parameters.AddWithValue("setCreated_at", categoria.created_at);
                command.Parameters.AddWithValue("setUpdated_at", categoria.updated_at);
                command.Parameters.AddWithValue("setIsdescuento", categoria.isdescuento);
                command.Parameters.AddWithValue("setDescuento", categoria.descuento);

                command.ExecuteScalar();
            }
        }
        //public void saveProveedores(List<Proveedor> proveedores)
        //{
        //    foreach (Proveedor proveedor in proveedores)
        //    {
        //        SQLiteCommand command = connection.CreateCommand();
        //        command.CommandText = "INSERT OR REPLACE INTO proveedores (id, nombre, direccion, correo, telefono, descripcion, activo, created_at, updated_at) " +
        //        "VALUES(@setId, @setNombre, @setDireccion, @setCorreo, @setTelefono, @setDescripcion, @setActivo, @setCreated_at, @setUpdated_at) ";
        //        command.Parameters.AddWithValue("setId", proveedor.id);
        //        command.Parameters.AddWithValue("setNombre", proveedor.nombre);
        //        command.Parameters.AddWithValue("setDireccion", proveedor.direccion);
        //        command.Parameters.AddWithValue("setCorreo", proveedor.correo);
        //        command.Parameters.AddWithValue("setTelefono", proveedor.telefono);
        //        command.Parameters.AddWithValue("setDescripcion", proveedor.descripcion);
        //        command.Parameters.AddWithValue("setActivo", proveedor.activo);
        //        command.Parameters.AddWithValue("setCreated_at", proveedor.created_at);
        //        command.Parameters.AddWithValue("setUpdated_at", proveedor.updated_at);
        //        command.ExecuteScalar();
        //    }
        //}

        public void saveProductos(List<Producto> productos)
        {
            foreach (Producto producto in productos)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO productos (id, codigo, nombre, presentacion, iva, menudeo, mayoreo, cantidad_mayoreo, especial, vendedor, imagen, activo, created_at, updated_at, medida_id, categoria_id) " +
                "VALUES(@setId, @setCodigo, @setNombre, @setPresentacion, @setIva, @setMenudeo, @setMayoreo, @setCantidad_mayoreo, @setEspecial, @setVendedor, @setImagen, @setActivo, @setCreated_at, @setUpdated_at, @setMedida_id, @setCategoria_id) ";
                command.Parameters.AddWithValue("setId", producto.id);
                command.Parameters.AddWithValue("setCodigo", producto.codigo);
                command.Parameters.AddWithValue("setNombre", producto.nombre);
                command.Parameters.AddWithValue("setPresentacion", producto.presentacion);
                command.Parameters.AddWithValue("setIva", producto.iva);
                command.Parameters.AddWithValue("setMenudeo", producto.menudeo);
                command.Parameters.AddWithValue("setMayoreo", producto.mayoreo);
                command.Parameters.AddWithValue("setCantidad_mayoreo", producto.cantidad_mayoreo);
                command.Parameters.AddWithValue("setEspecial", producto.especial);
                command.Parameters.AddWithValue("setVendedor", producto.vendedor);
                command.Parameters.AddWithValue("setImagen", producto.imagen);
                command.Parameters.AddWithValue("setActivo", producto.activo);
                command.Parameters.AddWithValue("setCreated_at", producto.created_at);
                command.Parameters.AddWithValue("setUpdated_at", producto.updated_at);
                command.Parameters.AddWithValue("setMedida_id", producto.medida_id);
                command.Parameters.AddWithValue("setCategoria_id", producto.categoria_id);
                command.ExecuteScalar();
            }
        }
        public void saveUsuarios(List<Usuario> usuarios)
        {
            foreach (Usuario usuario in usuarios)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO [usuarios] (id, nombre, correo, confirmacion, telefono, imagen, usuario, clave, is_root, activo, created_at, updated_at) " +
                "VALUES(@setId, @setNombre, @setCorreo, @setConfirmacion, @setTelefono, @setImagen, @setUsuario, @setClave, @setIs_root, @setActivo, @setCreated_at, @setUpdated_at) ";
                command.Parameters.AddWithValue("setId", usuario.id);
                command.Parameters.AddWithValue("setNombre", usuario.nombre);
                command.Parameters.AddWithValue("setCorreo", usuario.correo);
                command.Parameters.AddWithValue("setConfirmacion", usuario.confirmacion);
                command.Parameters.AddWithValue("setTelefono", usuario.telefono);
                command.Parameters.AddWithValue("setImagen", usuario.imagen);
                command.Parameters.AddWithValue("setUsuario", usuario.usuario);
                command.Parameters.AddWithValue("setClave", usuario.clave);
                command.Parameters.AddWithValue("setIs_root", usuario.es_raiz);
                command.Parameters.AddWithValue("setActivo", usuario.activo);
                command.Parameters.AddWithValue("setCreated_at", usuario.created_at);
                command.Parameters.AddWithValue("setUpdated_at", usuario.updated_at);
                command.ExecuteScalar();
            }
        }

        public void saveClientes(List<Cliente> clientes)
        {
            foreach (Cliente cliente in clientes)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO [clientes] (id, nombre, rfc, calle, no_exterior, no_interior, cp, colonia, ciudad, telefono, correo, activo, created_at, updated_at) " +
                "VALUES(@setId, @setNombre, @setRfc, @setCalle, @setNoext, @setNoint, @setCp, @setColonia, @setCiudad, @setTelefono, @setCorreo, @setActivo, @setCreated_at, @setUpdated_at) ";
                command.Parameters.AddWithValue("setId", cliente.id);
                command.Parameters.AddWithValue("setNombre", cliente.nombre);
                command.Parameters.AddWithValue("setRfc", cliente.rfc);
                command.Parameters.AddWithValue("setCalle", cliente.calle);
                command.Parameters.AddWithValue("setNoext", cliente.numero_exterior);
                command.Parameters.AddWithValue("setNoint", cliente.numero_interior);
                command.Parameters.AddWithValue("setCp", cliente.codigo_postal);
                command.Parameters.AddWithValue("setColonia", cliente.colonia);
                command.Parameters.AddWithValue("setCiudad", cliente.ciudad);
                command.Parameters.AddWithValue("setTelefono", cliente.telefono);
                command.Parameters.AddWithValue("setCorreo", cliente.correo);
                command.Parameters.AddWithValue("setActivo", cliente.activo);
                command.Parameters.AddWithValue("setCreated_at", cliente.created_at);
                command.Parameters.AddWithValue("setUpdated_at", cliente.updated_at);
                command.ExecuteNonQuery();
            }
        }
        public void saveApartados(List<Apartado> apartados)
        {
            foreach (Apartado apartado in apartados)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO [apartados] (id, productos, total, total_pagado, fecha_apartado, folio_corte, fecha_entrega, estado, " +
                    "cliente_creditos_id, id_cajero_registro, id_cejero_entrega, sucursal_id, observaciones, created_at, updated_at) VALUES (@setId, @setProductos, @setTotal, @setPagado, @setFapart," +
                    "@setFolioCorte, @setFentr, @setEstado, @setCliente, @setIdreg, @setIdentr, @setSucursal, @setObs, @setCreated, @setUpdated)";
                command.Parameters.AddWithValue("setId", apartado.id);
                command.Parameters.AddWithValue("setProductos", apartado.productos);
                command.Parameters.AddWithValue("setTotal", apartado.total);
                command.Parameters.AddWithValue("setPagado", apartado.total_pagado);
                command.Parameters.AddWithValue("setFapart", apartado.fecha_de_apartado);
                command.Parameters.AddWithValue("setFolioCorte", apartado.folio_corte);
                command.Parameters.AddWithValue("setFentr", apartado.fecha_entrega);
                command.Parameters.AddWithValue("setEstado", apartado.estado);
                command.Parameters.AddWithValue("setCliente", apartado.cliente_creditos_id);
                command.Parameters.AddWithValue("setIdreg", apartado.id_cajero_registro);
                command.Parameters.AddWithValue("setIdentr", apartado.id_cajero_entrega);
                command.Parameters.AddWithValue("setSucursal", apartado.sucursal_id);
                command.Parameters.AddWithValue("setObs", apartado.observaciones);
                command.Parameters.AddWithValue("setCreated", apartado.created_at);
                command.Parameters.AddWithValue("setUpdated", apartado.updated_at);
                command.ExecuteNonQuery();
            }
            
        }
        public void saveCortes(List<Corte> cortes)
        {
            foreach (Corte corte in cortes)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO [cortes] (id, folio_corte, fondo_apertura, total_efectivo, total_tarjetas_debito, " +
                    "total_tarjetas_credito, total_cheques, total_transferencias, efectivo_apartados, efectivo_creditos, gastos, ingresos, sobrante, " +
                    "fecha_apertura_caja, fecha_corte_caja, sucursal_id, usuario_id, estado, detalles, created_at, updated_at) VALUES (@setId, @setFolio, @setFondoApertura, @setTotalEfectivo, " +
                    "@setTotalTarjetasDebito, @setTotalTarjetasCredito, @setTotalCheques, @setTotalTransferencias, @setEfectivoApartados, " +
                    "@setEfectivoCreditos, @setGastos, @setIngresos, @setSobrante, @setFechaAperturaCaja, @setFechaCorteCaja, @setSucursalId, @setUsuarioId, @setEstado, @setDetalles, @setCreated_at, @setUpdated_at)";

                command.Parameters.AddWithValue("@setId", corte.id);
                command.Parameters.AddWithValue("@setFolio", corte.folio_corte);
                command.Parameters.AddWithValue("@setFondoApertura", corte.fondo_apertura);
                command.Parameters.AddWithValue("@setTotalEfectivo", corte.total_efectivo);
                command.Parameters.AddWithValue("@setTotalTarjetasDebito", corte.total_tarjetas_debito);
                command.Parameters.AddWithValue("@setTotalTarjetasCredito", corte.total_tarjetas_credito);
                command.Parameters.AddWithValue("@setTotalCheques", corte.total_cheques);
                command.Parameters.AddWithValue("@setTotalTransferencias", corte.total_transferencias);
                command.Parameters.AddWithValue("@setEfectivoApartados", corte.efectivo_apartados);
                command.Parameters.AddWithValue("@setEfectivoCreditos", corte.efectivo_creditos);
                command.Parameters.AddWithValue("@setGastos", corte.gastos);
                command.Parameters.AddWithValue("@setIngresos", corte.ingresos);
                command.Parameters.AddWithValue("@setSobrante", corte.sobrante);
                command.Parameters.AddWithValue("@setFechaAperturaCaja", corte.fecha_apertura_caja);
                command.Parameters.AddWithValue("@setFechaCorteCaja", corte.fecha_corte_caja);
                command.Parameters.AddWithValue("@setSucursalId", corte.sucursal_id);
                command.Parameters.AddWithValue("@setUsuarioId", corte.usuario_id);
                command.Parameters.AddWithValue("@setEstado",corte.estado);
                command.Parameters.AddWithValue("@setDetalles", corte.detalles);
                command.Parameters.AddWithValue("@setCreated_at",corte.created_at);
                command.Parameters.AddWithValue("@setUpdated_at",corte.updated_at);
                command.ExecuteNonQuery();
            }
        }


        public void saveCreditos(List<Credito> creditos)
        {
            foreach (Credito credito in creditos)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO [creditos] (id, productos, total, total_pagado, fecha_de_credito, folio, estado, " +
                    "cliente_creditos_id, id_cajero_registro, sucursal_id, observaciones, created_at, updated_at) VALUES (@setId, @setProductos, @setTotal, @setPagado, @setFapart," +
                    "@setFolio, @setEstado, @setCliente, @setIdreg, @setSucursal, @setObs, @setCreated, @setUpdated)";
                command.Parameters.AddWithValue("setId", credito.id);
                command.Parameters.AddWithValue("setProductos", credito.productos);
                command.Parameters.AddWithValue("setTotal", credito.total);
                command.Parameters.AddWithValue("setPagado", credito.total_pagado);
                command.Parameters.AddWithValue("setFapart", credito.fecha_de_credito);
                command.Parameters.AddWithValue("setFolio", credito.folio);
                command.Parameters.AddWithValue("setEstado", credito.estado);
                command.Parameters.AddWithValue("setCliente", credito.cliente_creditos_id);
                command.Parameters.AddWithValue("setIdreg", credito.id_cajero_registro);
                command.Parameters.AddWithValue("setSucursal", credito.sucursal_id);
                command.Parameters.AddWithValue("setObs", credito.observaciones);
                command.Parameters.AddWithValue("setCreated", credito.created_at);
                command.Parameters.AddWithValue("setUpdated", credito.updated_at);
                command.ExecuteNonQuery();
            }
        }
        public void saveAbonosCredito(List<AbonoCredito> abonos)
        {
            foreach (AbonoCredito abono in abonos)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO [abonos_credito] (id, folio, metodo_pago, total_abonado, fecha, id_credito, folio_corte, id_cajero, created_at, updated_at)" +
                    "VALUES (@setId, @setFolio, @setMetodo, @setTotal, @setFecha, @setCredito, @setCorte, @setCajero, @setCreated, @setUpdated)";
                command.Parameters.AddWithValue("setId", abono.id);
                command.Parameters.AddWithValue("setFolio", abono.folio);
                command.Parameters.AddWithValue("setMetodo", abono.metodo_pago);
                command.Parameters.AddWithValue("setTotal", abono.total_abonado);
                command.Parameters.AddWithValue("setFecha", abono.fecha);
                command.Parameters.AddWithValue("setCredito", abono.credito_id);
                command.Parameters.AddWithValue("setCorte", abono.folio_corte);
                command.Parameters.AddWithValue("setCajero", abono.usuario_id);
                command.Parameters.AddWithValue("setCreated", abono.created_at);
                command.Parameters.AddWithValue("setUpdated", abono.updated_at);
                command.ExecuteNonQuery();
            }
        }
        public void saveAbonosApartado(List<AbonoApartado> abonos)
        {
            foreach (AbonoApartado abono in abonos)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO [abonos_apartado] (id, folio, metodo_pago, total_abonado, fecha, id_apartado, folio_corte, id_cajero, created_at, updated_at)" +
                    "VALUES (@setId, @setFolio, @setMetodo, @setTotal, @setFecha, @setApartado, @setCorte, @setCajero, @setCreated, @setUpdated)";
                command.Parameters.AddWithValue("setId", abono.id);
                command.Parameters.AddWithValue("setFolio", abono.folio);
                command.Parameters.AddWithValue("setMetodo", abono.metodo_pago);
                command.Parameters.AddWithValue("setTotal", abono.total_abonado);
                command.Parameters.AddWithValue("setFecha", abono.fecha);
                command.Parameters.AddWithValue("setApartado", abono.apartado_id);
                command.Parameters.AddWithValue("setCorte", abono.folio_corte);
                command.Parameters.AddWithValue("setCajero", abono.usuario_id);
                command.Parameters.AddWithValue("setCreated", abono.created_at);
                command.Parameters.AddWithValue("setUpdated", abono.updated_at);
                command.ExecuteNonQuery();
            }
        }
        //public void saveOperaciones(List<Operacion> operaciones)
        //{
        //    foreach (Operacion operacion in operaciones)
        //    {
        //        SQLiteCommand command = connection.CreateCommand();
        //        command.CommandText = "INSERT OR REPLACE INTO operaciones (id, accion, confirmar, created_at, updated_at, producto_id, usuario_id) " +
        //        "VALUES(@setId, @setAccion, @setConfirmar, @setCreated_at, @setUpdated_at, @setProducto_id, @setUsuario_id) ";
        //        command.Parameters.AddWithValue("setId", operacion.id);
        //        command.Parameters.AddWithValue("setAccion", operacion.accion);
        //        command.Parameters.AddWithValue("setConfirmar", operacion.confirmar);
        //        command.Parameters.AddWithValue("setCreated_at", operacion.created_at);
        //        command.Parameters.AddWithValue("setUpdated_at", operacion.updated_at);
        //        command.Parameters.AddWithValue("setProducto_id", operacion.producto_id);
        //        command.Parameters.AddWithValue("setUsuario_id", operacion.usuario_id);
        //        command.ExecuteScalar();
        //    }
        //}
        public void saveSucursales(List<Sucursal> sucursales)
        {
            foreach (Sucursal sucursal in sucursales)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "INSERT OR REPLACE INTO sucursales (id, puerta_enlace1, puerta_enlace2, puerta_enlace3, puerta_enlace4, razon_social, direccion, correo, activo, created_at, updated_at) " +
                "VALUES(@setId, @setEnlace1, @setEnlace2, @setEnlace3, @setEnlace4, @setRazon, @setDir, @setCorreo, @setActivo, @setCreated_at, @setUpdated_at) ";
                command.Parameters.AddWithValue("setId", sucursal.id);
                command.Parameters.AddWithValue("setEnlace1", sucursal.puerta_enlace1);
                command.Parameters.AddWithValue("setEnlace2", sucursal.puerta_enlace2);
                command.Parameters.AddWithValue("setEnlace3", sucursal.puerta_enlace3);
                command.Parameters.AddWithValue("setEnlace4", sucursal.puerta_enlace4);
                command.Parameters.AddWithValue("setRazon", sucursal.razon_social);
                command.Parameters.AddWithValue("setDir", sucursal.direccion);
                command.Parameters.AddWithValue("setCorreo", sucursal.correo);
                command.Parameters.AddWithValue("setActivo", sucursal.activo);
                command.Parameters.AddWithValue("setCreated_at", sucursal.created_at);
                command.Parameters.AddWithValue("setUpdated_at", sucursal.updated_at);
                command.ExecuteScalar();
            }
        }
        public int getLastIdVentas()
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id FROM ventas ORDER BY id DESC LIMIT 1";
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                int id = result.GetInt32(0);
                return id;
            }
            return 0;
        }
        public int getLastIdApartados()
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id FROM apartados_temporal ORDER BY id DESC LIMIT 1";
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                int id = result.GetInt32(0);
                return id;
            }
            return 0;
        }
        // ===========================================
        // MODIFICACIÓN: CrearVenta - INCLUIR DESCUENTOS PRECIO ESPECIAL
        // ===========================================
        // MÉTODO CrearVenta EN LocaldataManager - NO MODIFICAR
        public int CrearVenta(Dictionary<string, string> venta, List<ProductoVenta> productos)
        {
            int id = 0;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO ventas (total, folio, folio_corte, fecha_venta, metodo_pago, tipo, sucursal_id, usuario_id, cancelacion, estado, detalles, descuento) " +
                                  "VALUES(@setTotal, @setFolio, @setCorte, @setFecha, @setMetodo, @setTipo, @setSucursal_id, @setUsuario_id, @setCancelacion, @setEstado, @setDetalles, @setDescuento)";
            command.Parameters.AddWithValue("setTotal", venta["total"]);
            command.Parameters.AddWithValue("setFolio", venta["folio"]);
            command.Parameters.AddWithValue("setCorte", venta["folio_corte"]);
            command.Parameters.AddWithValue("setFecha", venta["fecha_venta"]);
            command.Parameters.AddWithValue("setMetodo", venta["metodo_pago"]);
            command.Parameters.AddWithValue("setTipo", venta["tipo"]);
            command.Parameters.AddWithValue("setSucursal_id", venta["sucursal_id"]);
            command.Parameters.AddWithValue("setUsuario_id", venta["usuario_id"]);
            command.Parameters.AddWithValue("setCancelacion", 0);
            command.Parameters.AddWithValue("setEstado", 1);
            command.Parameters.AddWithValue("setDetalles", "Pendiente de envío");
            command.Parameters.AddWithValue("setDescuento", venta.ContainsKey("descuento") ? venta["descuento"] : "0");
            command.ExecuteScalar();

            command.CommandText = "select last_insert_rowid()";
            Int64 LastRowID64 = (Int64)command.ExecuteScalar();
            id = (int)LastRowID64;

            foreach (ProductoVenta p in productos)
            {
                command.CommandText = "INSERT INTO producto_venta (venta_id, producto_id, codigo, cantidad, precio_venta, estado, detalles)" +
                                      "VALUES (@setVenta, @setProducto, @setCodigo, @setCantidad, @setPrecio, @setEstado, @setDetalles)";
                command.Parameters.AddWithValue("setVenta", id);
                command.Parameters.AddWithValue("setProducto", p.id);
                command.Parameters.AddWithValue("setCodigo", p.codigo);
                command.Parameters.AddWithValue("setCantidad", p.cantidad);
                command.Parameters.AddWithValue("setPrecio", p.precio_venta);
                command.Parameters.AddWithValue("setEstado", 1);
                command.Parameters.AddWithValue("setDetalles", "Pendiente de envío");
                command.ExecuteScalar();
            }

            return id;
        }

        public Cliente buscarCliente(string data)
        {
            Cliente cliente = null;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM clientes WHERE nombre = @setData OR telefono = @setData OR correo = @setData LIMIT 1";
            command.Parameters.AddWithValue("setData", data);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                cliente = new Cliente()
                {
                    id = result.GetInt32(0),
                    nombre = result.GetString(1),
                    rfc = result.IsDBNull(2) ? "" : result.GetString(2),
                    calle = result.IsDBNull(3) ? "" : result.GetString(3),
                    numero_exterior = result.IsDBNull(4) ? "" : result.GetString(4),
                    numero_interior = result.IsDBNull(5) ? "" : result.GetString(5),
                    codigo_postal = result.IsDBNull(6) ? "" : result.GetString(6),
                    colonia = result.IsDBNull(7) ? "" : result.GetString(7),
                    ciudad = result.IsDBNull(8) ? "" : result.GetString(8),
                    telefono = result.GetString(9),
                    correo = result.GetString(10),
                    activo = 1
                };
            }
            else
            {
                command.Reset();
                command.CommandText = "SELECT * FROM clientes_temporal WHERE nombre = @setData OR telefono = @setData OR correo = @setData LIMIT 1";
                command.Parameters.AddWithValue("setData", data);
                result = command.ExecuteReader();
                if (result.Read())
                {
                    cliente = new Cliente()
                    {
                        id = result.GetInt32(0),
                        nombre = result.GetString(1),
                        rfc = result.IsDBNull(2) ? "" : result.GetString(2),
                        calle = result.IsDBNull(3) ? "" : result.GetString(3),
                        numero_exterior = result.IsDBNull(4) ? "" : result.GetString(4),
                        numero_interior = result.IsDBNull(5) ? "" : result.GetString(5),
                        codigo_postal = result.IsDBNull(6) ? "" : result.GetString(6),
                        colonia = result.IsDBNull(7) ? "" : result.GetString(7),
                        ciudad = result.IsDBNull(8) ? "" : result.GetString(8),
                        telefono = result.GetString(9),
                        correo = result.GetString(10),
                        activo = -1
                    };
                }
            }
            return cliente;
        }
        public Producto GetProductByCode(string code)
        {
            Producto producto = null;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM productos WHERE activo=1 AND codigo = @setCode";
            command.Parameters.AddWithValue("setCode", code);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                producto = new Producto();

                producto.id = result.GetInt32(0);
                producto.codigo = result.GetString(1);
                producto.nombre = result.GetString(2);
                producto.presentacion = result.IsDBNull(3) ? "" : result.GetString(3);
                producto.iva = result.GetFloat(4);
                producto.menudeo = result.GetFloat(5);
                producto.mayoreo = result.GetFloat(6);
                producto.cantidad_mayoreo = result.GetInt32(7);
                producto.especial = result.GetFloat(8);
                producto.vendedor = result.GetFloat(9);
                producto.imagen = result.IsDBNull(10) ? "" : result.GetString(10);
                producto.activo = result.GetInt32(11);
                producto.created_at = result.GetString(12);
                producto.updated_at = result.GetString(13);
                producto.medida_id = result.GetInt32(14);
                producto.categoria_id = result.GetInt32(15);

            }
            else
            {
                command.Reset();
                command.CommandText = "SELECT * FROM alta_temporal WHERE codigo = @setCode";
                command.Parameters.AddWithValue("setCode", code);
                result = command.ExecuteReader();
                if (result.Read())
                {
                    producto = new Producto();

                    producto.id = 0;
                    producto.codigo = result.GetString(1);
                    producto.nombre = result.GetString(2);
                    producto.presentacion = result.IsDBNull(3) ? "" : result.GetString(3);
                    producto.menudeo = result.GetFloat(4);
                    producto.mayoreo = result.GetFloat(5);
                    producto.cantidad_mayoreo = result.GetInt32(6);
                    producto.especial = result.GetFloat(7);
                    producto.vendedor = result.GetFloat(8);
                    producto.imagen = "";
                    producto.medida_id = result.GetInt32(9);
                    producto.categoria_id = result.GetInt32(10);

                }
            }
            return producto;
        }
        public Producto searchProductByCode(string code)
        {
            Producto producto = null;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM productos WHERE codigo = @setCode";
            command.Parameters.AddWithValue("setCode", code);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                producto = new Producto();

                producto.id = result.GetInt32(0);
                producto.codigo = result.GetString(1);
                producto.nombre = result.GetString(2);
                producto.presentacion = result.IsDBNull(3) ? "" : result.GetString(3);
                producto.iva = result.GetFloat(4);
                producto.menudeo = result.GetFloat(5);
                producto.mayoreo = result.GetFloat(6);
                producto.cantidad_mayoreo = result.GetInt32(7);
                producto.especial = result.GetFloat(8);
                producto.vendedor = result.GetFloat(9);
                producto.imagen = result.IsDBNull(10) ? "" : result.GetString(10);
                producto.activo = result.GetInt32(11);
                producto.created_at = result.GetString(12);
                producto.updated_at = result.GetString(13);
                producto.medida_id = result.GetInt32(14);
                producto.categoria_id = result.GetInt32(15);

            }
            else
            {
                command.Reset();
                command.CommandText = "SELECT * FROM alta_temporal WHERE codigo = @setCode";
                command.Parameters.AddWithValue("setCode", code);
                result = command.ExecuteReader();
                if (result.Read())
                {
                    producto = new Producto();

                    producto.id = result.GetInt32(0);
                    producto.codigo = result.GetString(1);
                    producto.nombre = result.GetString(2);
                    producto.presentacion = result.IsDBNull(3) ? "" : result.GetString(3);
                    producto.menudeo = result.GetFloat(4);
                    producto.mayoreo = result.GetFloat(5);
                    producto.cantidad_mayoreo = result.GetInt32(6);
                    producto.especial = result.GetFloat(7);
                    producto.vendedor = result.GetFloat(8);
                    producto.activo = 1;
                    producto.medida_id = result.GetInt32(9);
                    producto.categoria_id = result.GetInt32(10);

                }
            }

            return producto;
        }
        public void quitarAltaTempóral(List<int> ids)
        {
            if (ids.Count > 0)
            {
                string wherearg = " WHERE id IN (";
                bool ban = false;
                foreach (int id in ids)
                {
                    if (ban)
                    {
                        wherearg += ", " + id;
                    }
                    else
                    {
                        wherearg += id;
                        ban = true;
                    }

                }
                wherearg += ")";
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM alta_temporal " + wherearg;
                command.ExecuteNonQuery();
            }
        }
       
        public int clienteTemporal(NuevoCliente cliente)
        {
            int id = -1;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO clientes_temporal (nombre, rfc, calle, no_exterior, no_interior, cp, colonia, ciudad, telefono, correo)" +
                "VALUES (@setNombre, @setRfc, @setCalle, @setNoext, @setNoint, @setCp, @setColonia, @setCiudad, @setTelefono, @setCorreo)";
            command.Parameters.AddWithValue("setNombre", cliente.nombre);
            command.Parameters.AddWithValue("setRfc", cliente.rfc);
            command.Parameters.AddWithValue("setCalle", cliente.calle);
            command.Parameters.AddWithValue("setNoext", cliente.numero_exterior);
            command.Parameters.AddWithValue("setNoint", cliente.numero_interior);
            command.Parameters.AddWithValue("setCp", cliente.codigo_postal);
            command.Parameters.AddWithValue("setColonia", cliente.colonia);
            command.Parameters.AddWithValue("setCiudad", cliente.ciudad);
            command.Parameters.AddWithValue("setTelefono", cliente.telefono);
            command.Parameters.AddWithValue("setCorreo", cliente.correo);
            command.ExecuteNonQuery();
            command.CommandText = "select last_insert_rowid()";
            Int64 LastRowID64 = (Int64)command.ExecuteScalar();
            id = (int)LastRowID64;
            return id;
        }
        public int apartadoTemporal(Apartado apartado)
        {
            int id = -1;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO apartados_temporal (productos, total, total_pagado, fecha_apartado, folio_corte, fecha_entrega, estado, cliente_creditos_id, id_cajero_registro, id_cajero_entrega, sucursal_id, temporal, observaciones)" +
                "VALUES (@setProductos, @setTotal, @setPagado, @setFapar, @setFolioCorte, @setFentr, @setEstado, @setCliente, @setIdregistro, @setIdentrega, @setSucursal, @setTemporal, @setObser)";
            command.Parameters.AddWithValue("setProductos", apartado.productos);
            command.Parameters.AddWithValue("setTotal", apartado.total);
            command.Parameters.AddWithValue("setPagado", apartado.total_pagado);
            command.Parameters.AddWithValue("setFapar", apartado.fecha_de_apartado);
            command.Parameters.AddWithValue("setFolioCorte", apartado.folio_corte);
            command.Parameters.AddWithValue("setFentr", apartado.fecha_entrega);
            command.Parameters.AddWithValue("setEstado", apartado.estado);
            command.Parameters.AddWithValue("setCliente", apartado.cliente_creditos_id);
            command.Parameters.AddWithValue("setIdregistro", apartado.id_cajero_registro);
            command.Parameters.AddWithValue("setIdentrega", apartado.id_cajero_entrega);
            command.Parameters.AddWithValue("setSucursal", apartado.sucursal_id);
            command.Parameters.AddWithValue("setTemporal", apartado.temporal);
            command.Parameters.AddWithValue("setObser", apartado.observaciones);
            command.ExecuteNonQuery();
            command.CommandText = "select last_insert_rowid()";
            Int64 LastRowID64 = (Int64)command.ExecuteScalar();
            id = (int)LastRowID64;
            apartado.folio_corte += id.ToString().PadLeft(4, '0');
            command.Reset();
            command.CommandText = "UPDATE apartados_temporal SET folio_corte = @setFolioCorte WHERE id = @setId";
            command.Parameters.AddWithValue("setFolioCorte", apartado.folio_corte);
            command.Parameters.AddWithValue("setId", id);
            command.ExecuteNonQuery();
            return id;
        }
        public int abonoApartadoTemporal(AbonoApartado abono)
        {
            int id = -1;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO abonos_apartado_temporal (folio, metodo_pago, total_abonado, fecha, folio_apartado, id_apartado, folio_corte, id_cajero)" +
                "VALUES (@setFolio, @setMetodo, @setTotal, @setFecha, @setFapartado, @setIdapartado, @setFcorte, @setCajero)";
            command.Parameters.AddWithValue("setFolio", abono.folio);
            command.Parameters.AddWithValue("setMetodo", abono.metodo_pago);
            command.Parameters.AddWithValue("setTotal", abono.total_abonado);
            command.Parameters.AddWithValue("setFecha", abono.fecha);
            command.Parameters.AddWithValue("setFapartado", abono.folio_apartado);
            command.Parameters.AddWithValue("setIdapartado", abono.apartado_id);
            command.Parameters.AddWithValue("setFcorte", abono.folio_corte);
            command.Parameters.AddWithValue("setCajero", abono.usuario_id);
            command.ExecuteNonQuery();
            command.CommandText = "select last_insert_rowid()";
            Int64 LastRowID64 = (Int64)command.ExecuteScalar();
            id = (int)LastRowID64;
            abono.folio += id.ToString().PadLeft(4, '0');
            command.Reset();
            command.CommandText = "UPDATE abonos_apartado_temporal SET folio = @setFolio WHERE id = @setId";
            command.Parameters.AddWithValue("setFolio", abono.folio);
            command.Parameters.AddWithValue("setId", id);
            command.ExecuteNonQuery();
            return id;
        }
        public int creditoTemporal(Credito credito)
        {
            int id = -1;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO creditos_temporal (productos, total, total_pagado, fecha_de_credito, folio, estado, cliente_creditos_id, id_cajero_registro, sucursal_id, temporal, observaciones)" +
                "VALUES (@setProductos, @setTotal, @setPagado, @setFapar, @setFolio, @setEstado, @setCliente, @setIdregistro, @setSucursal, @setTemporal, @setObser)";
            command.Parameters.AddWithValue("setProductos", credito.productos);
            command.Parameters.AddWithValue("setTotal", credito.total);
            command.Parameters.AddWithValue("setPagado", credito.total_pagado);
            command.Parameters.AddWithValue("setFapar", credito.fecha_de_credito);
            command.Parameters.AddWithValue("setFolio", credito.folio);
            command.Parameters.AddWithValue("setEstado", credito.estado);
            command.Parameters.AddWithValue("setCliente", credito.cliente_creditos_id);
            command.Parameters.AddWithValue("setIdregistro", credito.id_cajero_registro);
            command.Parameters.AddWithValue("setSucursal", credito.sucursal_id);
            command.Parameters.AddWithValue("setTemporal", credito.temporal);
            command.Parameters.AddWithValue("setObser", credito.observaciones);
            command.ExecuteNonQuery();
            command.CommandText = "select last_insert_rowid()";
            Int64 LastRowID64 = (Int64)command.ExecuteScalar();
            id = (int)LastRowID64;
            credito.folio += id.ToString().PadLeft(4, '0');
            command.Reset();
            command.CommandText = "UPDATE creditos_temporal SET folio = @setFolio WHERE id = @setId";
            command.Parameters.AddWithValue("setFolio", credito.folio);
            command.Parameters.AddWithValue("setId", id);
            command.ExecuteNonQuery();
            return id;
        }
        public int abonoCreditoTemporal(AbonoCredito abono)
        {
            int id = -1;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO abonos_credito_temporal (folio, metodo_pago, total_abonado, fecha, folio_credito, id_credito, folio_corte, id_cajero)" +
                "VALUES (@setFolio, @setMetodo, @setTotal, @setFecha, @setFcredito, @setIdcredito, @setFcorte, @setCajero)";
            command.Parameters.AddWithValue("setFolio", abono.folio);
            command.Parameters.AddWithValue("setMetodo", abono.metodo_pago);
            command.Parameters.AddWithValue("setTotal", abono.total_abonado);
            command.Parameters.AddWithValue("setFecha", abono.fecha);
            command.Parameters.AddWithValue("setFcredito", abono.folio_credito);
            command.Parameters.AddWithValue("setIdcredito", abono.credito_id);
            command.Parameters.AddWithValue("setFcorte", abono.folio_corte);
            command.Parameters.AddWithValue("setCajero", abono.usuario_id);
            command.ExecuteNonQuery();
            command.CommandText = "select last_insert_rowid()";
            Int64 LastRowID64 = (Int64)command.ExecuteScalar();
            id = (int)LastRowID64;
            abono.folio += id.ToString().PadLeft(4, '0');
            command.Reset();
            command.CommandText = "UPDATE abonos_credito_temporal SET folio = @setFolio WHERE id = @setId";
            command.Parameters.AddWithValue("setFolio", abono.folio);
            command.Parameters.AddWithValue("setId", id);
            command.ExecuteNonQuery();
            return id;
        }
        public int ExisteCliente(string nombre, string correo, string telefono)
        {
            int id = -1;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT id FROM clientes WHERE nombre = @setNombre AND correo = @setCorreo AND telefono = @setTelefono";
            command.Parameters.AddWithValue("setNombre", nombre);
            command.Parameters.AddWithValue("setCorreo", correo);
            command.Parameters.AddWithValue("setTelefono", telefono);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                id = result.GetInt32(0);
            }
            return id;
        }
        public NuevoCliente getClienteeTemporal(int id)
        {
            NuevoCliente c = null;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM clientes_temporal WHERE id = @setId";
            command.Parameters.AddWithValue("setId", id);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                c = new NuevoCliente
                {
                    id_temporal = result.GetInt32(0),
                    nombre = result.GetString(1),
                    rfc = result.GetString(2),
                    calle = result.GetString(3),
                    numero_exterior = result.GetString(4),
                    numero_interior = result.GetString(5),
                    codigo_postal = result.GetString(6),
                    colonia = result.GetString(7),
                    ciudad = result.GetString(8),
                    telefono = result.GetString(9),
                    correo = result.GetString(10),
                };
            }
            return c;
        }
        public bool UpdateCliente(Cliente cliente)
        {
            try
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"UPDATE clientes SET nombre = @nombre, rfc = @rfc, calle = @calle, no_exterior = @no_exterior, no_interior = @no_interior, cp = @cp, colonia = @colonia, ciudad = @ciudad, telefono = @telefono, correo = @correo WHERE id = @id";
                    command.Parameters.AddWithValue("@id", cliente.id);
                    command.Parameters.AddWithValue("@nombre", cliente.nombre);
                    command.Parameters.AddWithValue("@rfc", cliente.rfc);
                    command.Parameters.AddWithValue("@calle", cliente.calle);
                    command.Parameters.AddWithValue("@no_exterior", cliente.numero_exterior);
                    command.Parameters.AddWithValue("@no_interior", cliente.numero_interior);
                    command.Parameters.AddWithValue("@cp", cliente.codigo_postal);
                    command.Parameters.AddWithValue("@colonia", cliente.colonia);
                    command.Parameters.AddWithValue("@ciudad", cliente.ciudad);
                    command.Parameters.AddWithValue("@telefono", cliente.telefono);
                    command.Parameters.AddWithValue("@correo", cliente.correo);

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    // Log the SQL command for debugging
                    Console.WriteLine(command.CommandText);
                    foreach (SQLiteParameter parameter in command.Parameters)
                    {
                        Console.WriteLine($"{parameter.ParameterName}: {parameter.Value}");
                    }

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                // Show a message box with the error
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }


        public Cliente getCliente(string nombre)
        {
            Cliente c = null;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM clientes WHERE nombre = @setNombre";
            command.Parameters.AddWithValue("setNombre", nombre);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                c = new Cliente
                {
                    id = result.GetInt32(0),
                    nombre = result.GetString(1),
                    rfc = result.GetString(2),
                    calle = result.GetString(3),
                    numero_exterior = result.GetString(4),
                    numero_interior = result.GetString(5),
                    codigo_postal = result.GetString(6),
                    colonia = result.GetString(7),
                    ciudad = result.GetString(8),
                    telefono = result.GetString(9),
                    correo = result.GetString(10),
                };
            }
            return c;
        }
        public Cliente getCliente(int id)
        {   
            Cliente c = null;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM clientes WHERE id = @setId";
            command.Parameters.AddWithValue("setId", id);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                c = new Cliente
                {
                    id = result.GetInt32(0),
                    nombre = result.GetString(1),
                    rfc = result.GetString(2),
                    calle = result.GetString(3),
                    numero_exterior = result.GetString(4),
                    numero_interior = result.GetString(5),
                    codigo_postal = result.GetString(6),
                    colonia = result.GetString(7),
                    ciudad = result.GetString(8),
                    telefono = result.GetString(9),
                    correo = result.GetString(10),
                };
            }
            return c;
        }
        public void eliminarCliente(int id)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "DELETE FROM clientes WHERE id = @setId";
            command.Parameters.AddWithValue("setId", id);
            command.ExecuteNonQuery();
        }
        public void eliminarCortes()
        {
            SQLiteCommand getLastCorteCommand = connection.CreateCommand();
            getLastCorteCommand.CommandText = "SELECT id FROM cortes ORDER BY id DESC LIMIT 1";
            var lastCorteId = getLastCorteCommand.ExecuteScalar();

            if (lastCorteId != null)
            {
                SQLiteCommand deleteCortesCommand = connection.CreateCommand();
                deleteCortesCommand.CommandText = "DELETE FROM cortes WHERE id != @setId";
                deleteCortesCommand.Parameters.AddWithValue("@setId", lastCorteId);
                deleteCortesCommand.ExecuteNonQuery();
            }
            else
            {
                MessageBox.Show("No hay cortes en la base de datos.", "Error");
            }
        }

        public void eliminarClienteTemporal(int id)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "DELETE FROM clientes_temporal WHERE id = @setId";
            command.Parameters.AddWithValue("setId", id);
            command.ExecuteNonQuery();
        }
        public void reconectarApartados(int viejoID, int nuevoID)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE apartados_temporal SET cliente_creditos_id = @nuevoId, temporal = 0 WHERE cliente_creditos_id = @viejoId AND temporal = 1";
            command.Parameters.AddWithValue("nuevoId", nuevoID);
            command.Parameters.AddWithValue("viejoId", viejoID);
            command.ExecuteNonQuery();
        }
        public void reconectarCreditos(int viejoID, int nuevoID)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE creditos_temporal SET cliente_creditos_id = @nuevoId, temporal = 0 WHERE cliente_creditos_id = @viejoId AND temporal = 1";
            command.Parameters.AddWithValue("nuevoId", nuevoID);
            command.Parameters.AddWithValue("viejoId", viejoID);
            command.ExecuteNonQuery();
        }
        public DataTable getCortes()
        {
            DataTable dtCortes = new DataTable();
            SQLiteCommand commandCortes = connection.CreateCommand();
            commandCortes.CommandText = @"
SELECT cortes.id AS ID,
       cortes.folio_corte AS FOLIO,
       cortes.fondo_apertura AS 'FONDO_APERTURA',
       cortes.fecha_corte_caja AS 'FECHA_CORTE',
       cortes.sucursal_id AS SUCURSAL,
       cortes.usuario_id AS USUARIO,
       (cortes.total_efectivo + 
        cortes.total_tarjetas_debito + 
        cortes.total_tarjetas_credito + 
        cortes.total_cheques + 
        cortes.total_transferencias) AS TOTAL
FROM cortes
WHERE cortes.id < (SELECT MAX(id) FROM cortes)
ORDER BY cortes.id DESC";

            SQLiteDataAdapter adapterCortes = new SQLiteDataAdapter(commandCortes);
            adapterCortes.Fill(dtCortes);

            DataTable dtSucursales = new DataTable();
            SQLiteCommand commandSucursales = connection.CreateCommand();
            commandSucursales.CommandText = @"
SELECT id, razon_social
FROM sucursales";

            SQLiteDataAdapter adapterSucursales = new SQLiteDataAdapter(commandSucursales);
            adapterSucursales.Fill(dtSucursales);

            DataTable dtUsuarios = new DataTable();
            SQLiteCommand commandUsuarios = connection.CreateCommand();
            commandUsuarios.CommandText = @"
SELECT id, nombre
FROM usuarios";

            SQLiteDataAdapter adapterUsuarios = new SQLiteDataAdapter(commandUsuarios);
            adapterUsuarios.Fill(dtUsuarios);

            // Crear columnas para nombre de usuario y razon social
            dtCortes.Columns.Add("USUARIO_NOMBRE", typeof(string));
            dtCortes.Columns.Add("SUCURSAL_NOMBRE", typeof(string));

            // Añadir datos a las nuevas columnas
            foreach (DataRow row in dtCortes.Rows)
            {
                int usuarioId = Convert.ToInt32(row["USUARIO"]);
                int sucursalId = Convert.ToInt32(row["SUCURSAL"]);

                DataRow[] usuarioRows = dtUsuarios.Select("id = " + usuarioId);
                if (usuarioRows.Length > 0)
                {
                    row["USUARIO_NOMBRE"] = usuarioRows[0]["nombre"];
                }

                DataRow[] sucursalRows = dtSucursales.Select("id = " + sucursalId);
                if (sucursalRows.Length > 0)
                {
                    row["SUCURSAL_NOMBRE"] = sucursalRows[0]["razon_social"];
                }
            }

            // Eliminar columnas de ID de usuario y sucursal si no son necesarias
            dtCortes.Columns.Remove("USUARIO");
            dtCortes.Columns.Remove("SUCURSAL");

            // Renombrar columnas
            dtCortes.Columns["USUARIO_NOMBRE"].ColumnName = "USUARIO";
            dtCortes.Columns["SUCURSAL_NOMBRE"].ColumnName = "SUCURSAL";

            return dtCortes;
        }
        public DataTable getCortesBySucursal(int sucursalId)
        {
            DataTable dtCortes = new DataTable();
            SQLiteCommand commandCortes = connection.CreateCommand();
            commandCortes.CommandText = @"
SELECT cortes.id AS ID,
       cortes.folio_corte AS FOLIO,
       cortes.fondo_apertura AS 'FONDO_APERTURA',
       cortes.fecha_corte_caja AS 'FECHA_CORTE',
       cortes.sucursal_id AS SUCURSAL,
       cortes.usuario_id AS USUARIO,
       (cortes.total_efectivo + 
        cortes.total_tarjetas_debito + 
        cortes.total_tarjetas_credito + 
        cortes.total_cheques + 
        cortes.total_transferencias) AS TOTAL
FROM cortes
WHERE cortes.sucursal_id = @sucursalId
  AND cortes.id < (SELECT MAX(id) FROM cortes)
ORDER BY cortes.id DESC";

            commandCortes.Parameters.AddWithValue("@sucursalId", sucursalId);

            SQLiteDataAdapter adapterCortes = new SQLiteDataAdapter(commandCortes);
            adapterCortes.Fill(dtCortes);

            DataTable dtSucursales = new DataTable();
            SQLiteCommand commandSucursales = connection.CreateCommand();
            commandSucursales.CommandText = @"
SELECT id, razon_social
FROM sucursales";

            SQLiteDataAdapter adapterSucursales = new SQLiteDataAdapter(commandSucursales);
            adapterSucursales.Fill(dtSucursales);

            DataTable dtUsuarios = new DataTable();
            SQLiteCommand commandUsuarios = connection.CreateCommand();
            commandUsuarios.CommandText = @"
SELECT id, nombre
FROM usuarios";

            SQLiteDataAdapter adapterUsuarios = new SQLiteDataAdapter(commandUsuarios);
            adapterUsuarios.Fill(dtUsuarios);

            // Crear columnas para nombre de usuario y razon social
            dtCortes.Columns.Add("USUARIO_NOMBRE", typeof(string));
            dtCortes.Columns.Add("SUCURSAL_NOMBRE", typeof(string));

            // Añadir datos a las nuevas columnas
            foreach (DataRow row in dtCortes.Rows)
            {
                int usuarioId = Convert.ToInt32(row["USUARIO"]);
                int sucursalIdFromRow = Convert.ToInt32(row["SUCURSAL"]);

                DataRow[] usuarioRows = dtUsuarios.Select("id = " + usuarioId);
                if (usuarioRows.Length > 0)
                {
                    row["USUARIO_NOMBRE"] = usuarioRows[0]["nombre"];
                }

                DataRow[] sucursalRows = dtSucursales.Select("id = " + sucursalIdFromRow);
                if (sucursalRows.Length > 0)
                {
                    row["SUCURSAL_NOMBRE"] = sucursalRows[0]["razon_social"];
                }
            }

            // Eliminar columnas de ID de usuario y sucursal si no son necesarias
            dtCortes.Columns.Remove("USUARIO");
            dtCortes.Columns.Remove("SUCURSAL");

            // Renombrar columnas
            dtCortes.Columns["USUARIO_NOMBRE"].ColumnName = "USUARIO";
            dtCortes.Columns["SUCURSAL_NOMBRE"].ColumnName = "SUCURSAL";

            return dtCortes;
        }

        public DataTable getClientes()
        {
            DataTable dt = new DataTable();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = @"SELECT clientes.id AS ID,
                                   clientes.nombre AS NOMBRE,
                                   clientes.telefono AS TELEFONO,
                                   clientes.correo AS CORREO,
                                   clientes.rfc AS RFC,
                                   clientes.no_exterior AS [No. EXTERIOR],
                                   clientes.no_interior AS [No. INTERIOR],
                                   clientes.cp AS [C.P.],
                                   clientes.calle AS CALLE,
                                   clientes.colonia AS COLONIA,
                                   clientes.ciudad AS CIUDAD 
                            FROM clientes 
                            WHERE clientes.activo = 1"; // Filtrar solo activos

            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(dt);
            return dt;
        }


        public List<NuevoCliente> GetClientesTemporales()
        {
            List<NuevoCliente> clientes = new List<NuevoCliente>();

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM clientes_temporal";
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                clientes.Add(new NuevoCliente
                {
                    id_temporal = result.GetInt32(0),
                    nombre = result.GetString(1),
                    rfc = result.GetString(2),
                    calle = result.GetString(3),
                    numero_exterior = result.GetString(4),
                    numero_interior = result.GetString(5),
                    codigo_postal = result.GetString(6),
                    colonia = result.GetString(7),
                    ciudad = result.GetString(8),
                    telefono = result.GetString(9),
                    correo = result.GetString(10)
                });
                ;
            }
            return clientes;
        }
        public Apartado getApartadoFolio(string folio)
        {
            Apartado ap = null;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM apartados WHERE folio_corte = @setFolioCorte LIMIT 1";
            command.Parameters.AddWithValue("setFolioCorte", folio);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                ap = new Apartado
                {
                    id = result.GetInt32(0),
                    productos = result.GetString(1),
                    total = result.GetDouble(2),
                    total_pagado = result.GetDouble(3),
                    fecha_de_apartado = result.GetString(4),
                    folio_corte = result.GetString(5),
                    fecha_entrega = result.IsDBNull(6) ? "" : result.GetString(6),
                    estado = result.GetInt32(7),
                    cliente_creditos_id = result.GetInt32(8),
                    id_cajero_registro = result.GetInt32(9),
                    id_cajero_entrega = result.IsDBNull(10) ? "" : result.GetInt32(10).ToString(),
                    sucursal_id = result.GetInt32(11),
                    observaciones = result.GetString(12),
                    created_at = result.GetString(13),
                    updated_at = result.GetString(14),
                };
            }
            return ap;
        }
        public Credito getCreditoFolio(string folio)
        {
            Credito cr = null;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM creditos WHERE folio = @setFolio LIMIT 1";
            command.Parameters.AddWithValue("setFolio", folio);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                cr = new Credito
                {
                    id = result.GetInt32(0),
                    productos = result.GetString(1),
                    total = result.GetDouble(2),
                    total_pagado = result.GetDouble(3),
                    fecha_de_credito = result.GetString(4),
                    folio = result.GetString(5),
                    estado = result.GetInt32(6),
                    cliente_creditos_id = result.GetInt32(7),
                    id_cajero_registro = result.GetInt32(8),
                    sucursal_id = result.GetInt32(9),
                    observaciones = result.GetString(10),
                    created_at = result.GetString(11),
                    updated_at = result.GetString(12),
                };
            }
            return cr;
        }
        public List<Apartado> GetApartadosTemporales()
        {
            List<Apartado> apartados = new List<Apartado>();

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM apartados_temporal";
                using (SQLiteDataReader result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        Apartado apartado = new Apartado
                        {
                            productos = result.IsDBNull(1) ? null : result.GetString(1),
                            total = result.IsDBNull(2) ? 0.0 : result.GetDouble(2),
                            total_pagado = result.IsDBNull(3) ? 0.0 : result.GetDouble(3),
                            fecha_de_apartado = result.IsDBNull(4) ? null : result.GetString(4),
                            folio_corte = result.IsDBNull(5) ? null : result.GetString(5),
                            fecha_entrega = result.IsDBNull(6) ? null : result.GetString(6),
                            estado = result.IsDBNull(7) ? 0 : result.GetInt32(7),
                            cliente_creditos_id = result.IsDBNull(8) ? 0 : result.GetInt32(8),
                            id_cajero_registro = result.IsDBNull(9) ? 0 : result.GetInt32(9),
                            id_cajero_entrega = result.IsDBNull(10) ? null : result.GetInt32(10).ToString(),
                            sucursal_id = result.IsDBNull(11) ? 0 : result.GetInt32(11),
                            temporal = result.IsDBNull(12) ? 0 : result.GetInt32(12),
                            observaciones = result.IsDBNull(13) ? null : result.GetString(13)
                        };

                        // Cargar los abonos para este apartado
                        using (SQLiteCommand subcomm = connection.CreateCommand())
                        {
                            subcomm.CommandText = "SELECT * FROM abonos_apartado_temporal WHERE folio_apartado = @setFolio";
                            subcomm.Parameters.AddWithValue("setFolio", apartado.folio_corte);

                            using (SQLiteDataReader reader = subcomm.ExecuteReader())
                            {
                                apartado.abonos = new List<AbonoApartado>();
                                while (reader.Read())
                                {
                                    apartado.abonos.Add(new AbonoApartado
                                    {
                                        folio = reader.IsDBNull(1) ? null : reader.GetString(1),
                                        metodo_pago = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        total_abonado = reader.IsDBNull(3) ? 0.0 : reader.GetDouble(3),
                                        fecha = reader.IsDBNull(4) ? null : reader.GetString(4),
                                        folio_apartado = reader.IsDBNull(5) ? null : reader.GetString(5),
                                        apartado_id = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                        folio_corte = reader.IsDBNull(7) ? null : reader.GetString(7),
                                        usuario_id = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                                    });
                                }
                            }
                        }

                        apartados.Add(apartado);
                    }
                }
            }
            return apartados;
        }

        public Apartado GetApartadoTemporal(string folio)
        {
            Apartado apartado = new Apartado();

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM apartados_temporal WHERE folio_corte = @setFolioCorte";
            command.Parameters.AddWithValue("setFolioCorte", folio);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                apartado = new Apartado
                {

                    productos = result.GetString(1),
                    total = result.GetDouble(2),
                    total_pagado = result.GetDouble(3),
                    fecha_de_apartado = result.GetString(4),
                    folio_corte = result.GetString(5),
                    fecha_entrega = result.IsDBNull(6) ? null : result.GetString(6),
                    estado = result.GetInt32(7),
                    cliente_creditos_id = result.GetInt32(8),
                    id_cajero_registro = result.GetInt32(9),
                    id_cajero_entrega = result.IsDBNull(10) ? null : result.GetInt32(10).ToString(),
                    sucursal_id = result.GetInt32(11),
                    temporal = result.GetInt32(12),
                    observaciones = result.GetString(13)
                };
                SQLiteCommand subcomm = connection.CreateCommand();
                subcomm.CommandText = "SELECT * FROM abonos_apartado_temporal WHERE folio_apartado = @setFolio";
                subcomm.Parameters.AddWithValue("setFolio", apartado.folio_corte);
                SQLiteDataReader reader = subcomm.ExecuteReader();
                apartado.abonos = new List<AbonoApartado>();
                while (reader.Read())
                {
                    apartado.abonos.Add(new AbonoApartado
                    {
                        folio = reader.GetString(1),
                        metodo_pago = reader.GetString(2),
                        total_abonado = reader.GetDouble(3),
                        fecha = reader.GetString(4),
                        folio_apartado = reader.GetString(5),
                        apartado_id = reader.GetInt32(6),
                        folio_corte = reader.GetString(7),
                        usuario_id = reader.GetInt32(8)
                    });
                }

            }
            return apartado;
        }
        public Credito GetCreditoTemporal(string folio)
        {
            Credito credito = null;

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM creditos_temporal WHERE folio = @setFolio";
            command.Parameters.AddWithValue("setFolio", folio);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                credito = new Credito
                {

                    productos = result.GetString(1),
                    total = result.GetDouble(2),
                    total_pagado = result.GetDouble(3),
                    fecha_de_credito = result.GetString(4),
                    folio = result.GetString(5),
                    estado = result.GetInt32(6),
                    cliente_creditos_id = result.GetInt32(7),
                    id_cajero_registro = result.GetInt32(8),
                    sucursal_id = result.GetInt32(9),
                    temporal = result.GetInt32(10),
                    observaciones = result.GetString(11)
                };
                SQLiteCommand subcomm = connection.CreateCommand();
                subcomm.CommandText = "SELECT * FROM abonos_credito_temporal WHERE folio_credito = @setFolio";
                subcomm.Parameters.AddWithValue("setFolio", credito.folio);
                SQLiteDataReader reader = subcomm.ExecuteReader();
                credito.abonos = new List<AbonoCredito>();
                while (reader.Read())
                {
                    credito.abonos.Add(new AbonoCredito
                    {
                        folio = reader.GetString(1),
                        metodo_pago = reader.GetString(2),
                        total_abonado = reader.GetDouble(3),
                        fecha = reader.GetString(4),
                        folio_credito = reader.GetString(5),
                        credito_id = reader.GetInt32(6),
                        folio_corte = reader.GetString(7),
                        usuario_id = reader.GetInt32(8)
                    });
                }

            }
            return credito;
        }
        public List<Credito> GetCreditosTemporales()
        {
            List<Credito> creditos = new List<Credito>();

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM creditos_temporal";
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                Credito credito = new Credito
                {

                    productos = result.GetString(1),
                    total = result.GetDouble(2),
                    total_pagado = result.GetDouble(3),
                    fecha_de_credito = result.GetString(4),
                    folio = result.GetString(5),
                    estado = result.GetInt32(6),
                    cliente_creditos_id = result.GetInt32(7),
                    id_cajero_registro = result.GetInt32(8),
                    sucursal_id = result.GetInt32(9),
                    temporal = result.GetInt32(10),
                    observaciones = result.GetString(11)
                };
                SQLiteCommand subcomm = connection.CreateCommand();
                subcomm.CommandText = "SELECT * FROM abonos_credito_temporal WHERE folio_credito = @setFolio";
                subcomm.Parameters.AddWithValue("setFolio", credito.folio);
                SQLiteDataReader reader = subcomm.ExecuteReader();
                credito.abonos = new List<AbonoCredito>();
                while (reader.Read())
                {
                    credito.abonos.Add(new AbonoCredito
                    {
                        folio = reader.GetString(1),
                        metodo_pago = reader.GetString(2),
                        total_abonado = reader.GetDouble(3),
                        fecha = reader.GetString(4),
                        folio_credito = reader.GetString(5),
                        credito_id = reader.GetInt32(6),
                        folio_corte = reader.GetString(7),
                        usuario_id = reader.GetInt32(8)
                    });
                }
                creditos.Add(credito);

            }
            return creditos;
        }
        public List<AbonoCredito> GetAbonosCreditoTemporales()
        {
            List<AbonoCredito> abonos = new List<AbonoCredito>();

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM abonos_credito_temporal";
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                AbonoCredito abono = new AbonoCredito
                {
                    folio = result.GetString(1),
                    metodo_pago = result.GetString(2),
                    total_abonado = result.GetDouble(3),
                    fecha = result.GetString(4),
                    folio_credito = result.GetString(5),
                    credito_id = result.GetInt32(6),
                    folio_corte = result.GetString(7),
                    usuario_id = result.GetInt32(8),
                };
                abonos.Add(abono);
            }
            return abonos;
        }
        public List<AbonoApartado> GetAbonosApartadoTemporales()
        {
            List<AbonoApartado> abonos = new List<AbonoApartado>();

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM abonos_apartado_temporal";
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                AbonoApartado abono = new AbonoApartado
                {
                    folio = result.GetString(1),
                    metodo_pago = result.GetString(2),
                    total_abonado = result.GetDouble(3),
                    fecha = result.GetString(4),
                    folio_apartado = result.GetString(5),
                    apartado_id = result.GetInt32(6),
                    folio_corte = result.GetString(7),
                    usuario_id = result.GetInt32(8),
                };
                abonos.Add(abono);
            }
            return abonos;
        }

        public void AnalizarClientesTemporales()
        {
            List<NuevoCliente> clientes = GetClientesTemporales();
            if (clientes.Count > 0)
            {
                foreach (NuevoCliente np in clientes)
                {
                    SQLiteCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT id FROM clientes WHERE nombre = @setANombre AND telefono = @setTel AND correo = @setCorreo";
                    command.Parameters.AddWithValue("setANombre", np.nombre);
                    command.Parameters.AddWithValue("setTel", np.telefono);
                    command.Parameters.AddWithValue("setCorreo", np.correo);
                    SQLiteDataReader result = command.ExecuteReader();
                    if (result.Read())
                    {
                        int id = result.GetInt32(0);
                        command.Reset();
                        command.CommandText = "DELETE FROM clientes_temporal WHERE id = @setId";
                        command.Parameters.AddWithValue("setId", np.id_temporal);
                        command.ExecuteNonQuery();
                        reconectarApartados(np.id_temporal, id);
                        reconectarCreditos(np.id_temporal, id);
                        //command.CommandText = "UPDATE producto_venta SET producto_id = @setId WHERE codigo = @setCodigo";
                        //command.Parameters.AddWithValue("setId", id);
                        //command.Parameters.AddWithValue("setCodigo", np.codigo);
                        //command.ExecuteScalar();
                    }
                }
            }
        }
        public void AnalizarApartadosTemporales()
        {
            List<Apartado> apartados = GetApartadosTemporales();

            if (apartados.Count > 0)
            {
                foreach (Apartado apartado in apartados)
                {
                    SQLiteCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT id FROM apartados WHERE folio_corte = @setFolioCorte";
                    command.Parameters.AddWithValue("setFolioCorte", apartado.folio_corte);
                    SQLiteDataReader result = command.ExecuteReader();
                    if (result.Read())
                    {
                        int id = result.GetInt32(0);
                        command.Reset();
                        command.CommandText = "DELETE FROM apartados_temporal WHERE folio_corte = @setFolioCorte";
                        command.Parameters.AddWithValue("setFolioCorte", apartado.folio_corte);
                        command.ExecuteNonQuery();
                        command.Reset();
                        command.CommandText = "UPDATE abonos_apartado_temporal SET id_apartado = @setId WHERE folio_apartado = @setFolio";
                        command.Parameters.AddWithValue("setId", id);
                        command.Parameters.AddWithValue("setFolio", apartado.folio_corte);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        public void AnalizarCrerditosTemporales()
        {
            List<Credito> creditos = GetCreditosTemporales();

            if (creditos.Count > 0)
            {
                foreach (Credito credito in creditos)
                {
                    SQLiteCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT id FROM creditos WHERE folio = @setFolio";
                    command.Parameters.AddWithValue("setFolio", credito.folio);
                    SQLiteDataReader result = command.ExecuteReader();
                    if (result.Read())
                    {
                        int id = result.GetInt32(0);
                        command.Reset();
                        command.CommandText = "DELETE FROM creditos_temporal WHERE folio = @setFolio";
                        command.Parameters.AddWithValue("setFolio", credito.folio);
                        command.ExecuteNonQuery();
                        command.Reset();
                        command.CommandText = "UPDATE abonos_credito_temporal SET id_credito = @setId WHERE folio_credito = @setFolio";
                        command.Parameters.AddWithValue("setId", id);
                        command.Parameters.AddWithValue("setFolio", credito.folio);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        public void AnalizarAbonosCreditoTemporales()
        {
            List<AbonoCredito> abonos = GetAbonosCreditoTemporales();
            foreach (AbonoCredito abono in abonos)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT id FROM abonos_credito WHERE folio = @setFolio";
                command.Parameters.AddWithValue("setFolio", abono.folio);
                SQLiteDataReader result = command.ExecuteReader();
                if (result.Read())
                {
                    int id = result.GetInt32(0);
                    command.Reset();
                    command.CommandText = "DELETE FROM abonos_credito_temporal WHERE folio = @setFolio";
                    command.Parameters.AddWithValue("setFolio", abono.folio);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void AnalizarAbonosApartadoTemporales()
        {
            string createText = "";
            List<AbonoApartado> abonos = GetAbonosApartadoTemporales();
            foreach (AbonoApartado abono in abonos)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT id FROM abonos_apartado WHERE folio = @setFolio";
                command.Parameters.AddWithValue("setFolio", abono.folio);
                createText += "Buscando folio: " + abono.folio + Environment.NewLine;
                SQLiteDataReader result = command.ExecuteReader();
                if (result.Read())
                {
                    createText += "ENCONTRADO" + Environment.NewLine;
                    int id = result.GetInt32(0);
                    command.Reset();
                    command.CommandText = "DELETE FROM abonos_apartado_temporal WHERE folio = @setFolio";
                    command.Parameters.AddWithValue("setFolio", abono.folio);
                    command.ExecuteNonQuery();
                    createText += "BORRANDO folio: " + abono.folio + Environment.NewLine;
                }
            }
            string txtpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "log.txt");

            File.WriteAllText(txtpath, createText);

        }
        public List<int> altaTemporal(List<NuevoProducto> productos)
        {
            List<int> ids = new List<int>();
            foreach (NuevoProducto producto in productos)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO alta_temporal (codigo, nombre, presentacion, menudeo, mayoreo, cantidad_mayoreo, especial, vendedor, medida_id, categoria_id, estado, detalles) " +
                "VALUES(@setCodigo, @setNombre, @setPresentacion, @setMenudeo, @setMayoreo, @setCantidad_mayoreo, @setEspecial, @setVendedor, @setMedida_id, @setCategoria_id, @setEstado, @setDetalles) ";
                command.Parameters.AddWithValue("setCodigo", producto.codigo);
                command.Parameters.AddWithValue("setNombre", producto.nombre);
                command.Parameters.AddWithValue("setPresentacion", producto.presentacion);
                command.Parameters.AddWithValue("setMenudeo", producto.menudeo);
                command.Parameters.AddWithValue("setMayoreo", producto.mayoreo);
                command.Parameters.AddWithValue("setCantidad_mayoreo", producto.cantidad_mayoreo);
                command.Parameters.AddWithValue("setEspecial", producto.especial);
                command.Parameters.AddWithValue("setVendedor", producto.vendedor);
                command.Parameters.AddWithValue("setMedida_id", producto.medida_id);
                command.Parameters.AddWithValue("setCategoria_id", producto.categoria_id);
                command.Parameters.AddWithValue("setEstado", 1);
                command.Parameters.AddWithValue("setDetalles", "Pendiente de envío");
                command.ExecuteScalar();
                command.CommandText = "select last_insert_rowid()";
                Int64 LastRowID64 = (Int64)command.ExecuteScalar();
                ids.Add((int)LastRowID64);
            }
            return ids;
        }
        public void changeEstadoVenta(int id, int estado, string detalles)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE ventas SET estado = @setEstado, detalles = @setDetalles WHERE id = @setId";
            command.Parameters.AddWithValue("setEstado", estado);
            command.Parameters.AddWithValue("setDetalles", detalles);
            command.Parameters.AddWithValue("@setId", id);
            command.ExecuteScalar();
            command.CommandText = "UPDATE producto_venta SET estado = @setEstado, detalles = @setDetalles WHERE venta_id = @setId";
            command.Parameters.AddWithValue("setEstado", estado);
            command.Parameters.AddWithValue("setDetalles", detalles);
            command.Parameters.AddWithValue("@setId", id);
            command.ExecuteScalar();
        }
        public void changeEstadoCorte(int id, int estado, string detalles)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE cortes SET estado = @setEstado, detalles = @setDetalles WHERE id = @setId";
            command.Parameters.AddWithValue("setEstado", estado);
            command.Parameters.AddWithValue("setDetalles", detalles);
            command.Parameters.AddWithValue("setId", id);
            command.ExecuteScalar();
        }
        public void changeEstadoEntrada(int id, int estado, string detalles)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE entradas SET estado = @setEstado, detalles = @setDetalles WHERE id = @setId";
            command.Parameters.AddWithValue("setEstado", estado);
            command.Parameters.AddWithValue("setDetalles", detalles);
            command.Parameters.AddWithValue("@setId", id);
            command.ExecuteScalar();
            command.CommandText = "UPDATE producto_entrada SET estado = @setEstado, detalles = @setDetalles WHERE entrada_id = @setId";
            command.Parameters.AddWithValue("setEstado", estado);
            command.Parameters.AddWithValue("setDetalles", detalles);
            command.Parameters.AddWithValue("@setId", id);
            command.ExecuteScalar();
        }
        public void AnalizarProductosTemporales()
        {
            List<NuevoProducto> productos = GetProductosTemporales();
            if (productos.Count > 0)
            {
                foreach (NuevoProducto np in productos)
                {
                    SQLiteCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT id FROM productos WHERE codigo = @setCodigo";
                    command.Parameters.AddWithValue("setCodigo", np.codigo);
                    SQLiteDataReader result = command.ExecuteReader();
                    if (result.Read())
                    {
                        int id = result.GetInt32(0);
                        command.Reset();
                        command.CommandText = "DELETE FROM alta_temporal WHERE codigo = @setCodigo";
                        command.Parameters.AddWithValue("setCodigo", np.codigo);
                        command.ExecuteNonQuery();
                        command.Reset();
                        command.CommandText = "UPDATE producto_venta SET producto_id = @setId WHERE codigo = @setCodigo";
                        command.Parameters.AddWithValue("setId", id);
                        command.Parameters.AddWithValue("setCodigo", np.codigo);
                        command.ExecuteScalar();
                    }
                }
            }
        }
        public List<NuevoProducto> GetProductosTemporales()
        {
            List<NuevoProducto> productos = new List<NuevoProducto>();

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM alta_temporal";
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                productos.Add(new NuevoProducto
                {
                    codigo = result.GetString(1),
                    nombre = result.GetString(2),
                    presentacion = result.IsDBNull(3) ? "" : result.GetString(3),
                    menudeo = result.IsDBNull(4) ? 0 : result.GetDouble(4),
                    mayoreo = result.IsDBNull(5) ? 0 : result.GetDouble(5),
                    cantidad_mayoreo = result.IsDBNull(6) ? 0 : result.GetInt32(6),
                    especial = result.IsDBNull(7) ? 0 : result.GetDouble(7),
                    vendedor = result.IsDBNull(8) ? 0 : result.GetDouble(8),
                    imagen = "...",
                    medida_id = result.IsDBNull(9) ? 0 : result.GetInt32(9),
                    categoria_id = result.IsDBNull(10) ? 0 : result.GetInt32(10),

                });
                ;
            }
            return productos;
        }
        public void reconnectProductosVenta(List<NuevoProducto> productos)
        {
            foreach (NuevoProducto p in productos)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT id from productos WHERE codigo = @setCodigo";
                command.Parameters.AddWithValue("setCodigo", p.codigo);
                SQLiteDataReader result = command.ExecuteReader();
                if (result.Read())
                {
                    int id = result.GetInt32(0);
                    SQLiteCommand command2 = connection.CreateCommand();
                    command2.CommandText = "UPDATE producto_venta SET producto_id = @setId WHERE codigo = @setCodigo";
                    command2.Parameters.AddWithValue("setId", id);
                    command2.Parameters.AddWithValue("setCodigo", p.codigo);
                    command2.ExecuteScalar();
                }
            }
        }
        public void reconnectProductosEntrada(List<NuevoProducto> productos)
        {
            foreach (NuevoProducto p in productos)
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT id from productos WHERE codigo = @setCodigo";
                command.Parameters.AddWithValue("setCodigo", p.codigo);
                SQLiteDataReader result = command.ExecuteReader();
                if (result.Read())
                {
                    int id = result.GetInt32(0);
                    SQLiteCommand command2 = connection.CreateCommand();
                    command2.CommandText = "UPDATE producto_entrada SET producto_id = @setId WHERE codigo = @setCodigo";
                    command2.Parameters.AddWithValue("setId", id);
                    command2.Parameters.AddWithValue("setCodigo", p.codigo);
                    command2.ExecuteScalar();
                }
            }
        }
        public bool hasTemporal(int id, string tabla)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM @setTabla WHERE venta_id = @setVenta AND producto_id = @setProducto";
            command.Parameters.AddWithValue("setTabla", tabla);
            command.Parameters.AddWithValue("setVenta", id);
            command.Parameters.AddWithValue("setProducto", 0);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                return true;
            }
            return false;
        }
        public List<ProductoVenta> getCarrito(int idVenta)
        {
            List<ProductoVenta> productos = new List<ProductoVenta>();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT producto_venta.producto_id, productos.nombre, producto_venta.codigo, producto_venta.cantidad, producto_venta.precio_venta FROM producto_venta INNER JOIN productos ON producto_venta.Producto_id = productos.id  WHERE producto_venta.venta_id = @setVenta";
            command.Parameters.AddWithValue("setVenta", idVenta);
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                productos.Add(new ProductoVenta
                {
                    id = result.GetInt32(0),
                    nombre = result.GetString(1),
                    codigo = result.GetString(2),
                    cantidad = result.GetInt32(3),
                    precio_venta = result.GetDouble(4)
                });
            }
            return productos;
        }
        //public List<ProductoEntrada> getListaEntrada(int idEntrada)
        //{
        //    List<ProductoEntrada> productos = new List<ProductoEntrada>();
        //    SQLiteCommand command = connection.CreateCommand();
        //    command.CommandText = "SELECT * FROM producto_entrada WHERE entrada_id = @setEntrada";
        //    command.Parameters.AddWithValue("setEntrada", idEntrada);
        //    SQLiteDataReader result = command.ExecuteReader();
        //    while (result.Read())
        //    {
        //        productos.Add(new ProductoEntrada
        //        {
        //            id = result.GetInt32(2),
        //            codigo = result.GetString(3),
        //            cantidad = result.GetInt32(4),
        //            costo = result.GetDouble(5)
        //        });
        //    }
        //    return productos;
        //}
        public void clearAltaTemporal()
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "DELETE FROM alta_temporal";
            command.ExecuteScalar();
        }
        //public int registrarEntrada(Dictionary<string, object> entrada, List<ProductoEntrada> productos)
        //{
        //    SQLiteCommand command = connection.CreateCommand();
        //    command.CommandText = "INSERT INTO entradas (fecha_factura, total_factura, folio_factura, usuario_id, sucursal_id, proveedor_id, cancelacion, estado, detalles)" +
        //        "VALUES (@setFecha, @setTotal, @setFolio, @setUsuario, @setSucursal, @setProveedor, @setCancelacion, @setEstado, @setDetalles)";
        //    command.Parameters.AddWithValue("setFecha", entrada["fecha_factura"].ToString());
        //    command.Parameters.AddWithValue("setTotal", entrada["total_factura"].ToString());
        //    command.Parameters.AddWithValue("setFolio", entrada["folio_factura"].ToString());
        //    command.Parameters.AddWithValue("setUsuario", entrada["usuario_id"].ToString());
        //    command.Parameters.AddWithValue("setSucursal", entrada["sucursal_id"].ToString());
        //    command.Parameters.AddWithValue("setProveedor", entrada["proveedor_id"].ToString());
        //    command.Parameters.AddWithValue("setCancelacion", 0);
        //    command.Parameters.AddWithValue("setEstado", 1);
        //    command.Parameters.AddWithValue("setDetalles", "Pendiente de envío");
        //    command.ExecuteScalar();
        //    command.CommandText = "select last_insert_rowid()";
        //    Int64 LastRowID64 = (Int64)command.ExecuteScalar();
        //    int id = (int)LastRowID64;
        //    foreach (ProductoEntrada p in productos)
        //    {
        //        command.CommandText = "INSERT INTO producto_entrada (entrada_id, producto_id, codigo, cantidad, costo, estado, detalles)" +
        //            "VALUES (@setEntrada, @setProducto, @setCodigo, @setCantidad, @setCosto, @setEstado, @setDetalles)";
        //        command.Parameters.AddWithValue("setEntrada", id);
        //        command.Parameters.AddWithValue("setProducto", p.id);
        //        command.Parameters.AddWithValue("setCodigo", p.codigo);
        //        command.Parameters.AddWithValue("setCosto", p.costo);
        //        command.Parameters.AddWithValue("setCantidad", p.cantidad);
        //        command.Parameters.AddWithValue("setEstado", 1);
        //        command.Parameters.AddWithValue("setDetalles", "Pendiente de envío");
        //        command.ExecuteScalar();
        //    }
        //    return id;
        //}
        //public int nuevaSalida(int idsucursal)
        //{
        //    int response = 0;
        //    SQLiteCommand command = connection.CreateCommand();
        //    command.CommandText = "SELECT id FROM salidas_temporal WHERE estado = 0 ORDER BY id DESC LIMIT 1";
        //    SQLiteDataReader result = command.ExecuteReader();
        //    if (result.Read())
        //    {
        //        response = result.GetInt32(0);
        //    }
        //    else
        //    {
        //        command.Reset();
        //        command.CommandText = "INSERT INTO salidas_temporal (id_sucursal_destino, estado)" +
        //            "VALUES(@setSucursal, @setEstado)";
        //        command.Parameters.AddWithValue("setSucursal", idsucursal);
        //        command.Parameters.AddWithValue("setEstado", 0);
        //        command.ExecuteNonQuery();
        //        command.CommandText = "select last_insert_rowid()";
        //        Int64 LastRowID64 = (Int64)command.ExecuteScalar();
        //        response = (int)LastRowID64;
        //    }
        //    return response;
        //}
        public int getCajaAbierta()
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM cortes ORDER BY id DESC LIMIT 1";
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                int estado = result.GetInt32(17);
                int id = result.GetInt32(0);
                if (estado == 0)
                {
                    return id;
                }
            }
            return -1;
        }
        public string getFolioCorte(int id)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT folio_corte FROM cortes WHERE id = @setId";
            command.Parameters.AddWithValue("setId", id);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                string folio = result.GetString(0);
                return folio;
            }
            return null;
        }
        public double getEfectivoCaja(int idcorte)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM cortes WHERE id = @setId";
            command.Parameters.AddWithValue("setId", idcorte);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                int estado = result.GetInt32(17);
                double id = result.GetDouble(3);
                if (estado == 0)
                {
                    return id;
                }
            }
            return 0;
        }
        public int crearCorte(double apertura, int idsucursal)
        {
            DateTime localDate = DateTime.Now;
            int id;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO cortes (fondo_apertura, total_efectivo, total_tarjetas_debito, total_tarjetas_credito, total_cheques, total_transferencias, efectivo_apartados, efectivo_creditos, gastos, ingresos, sobrante, fecha_apertura_caja, estado, detalles, created_at, updated_at)" +
                "values(@setApertura, 0, 0, 0, 0, 0, 0, 0, '{}', '{}', 0, @setFecha, 0, 'Abierta', @setFechaActual, @setFechaActual)";
            command.Parameters.AddWithValue("setApertura", apertura);
            command.Parameters.AddWithValue("setFecha", localDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("setFechaActual", localDate.ToString("yyyy-MM-dd HH:mm:ss"));
            command.ExecuteNonQuery();

            command.CommandText = "select last_insert_rowid()";
            Int64 LastRowID64 = (Int64)command.ExecuteScalar();
            id = (int)LastRowID64;

            string folio = idsucursal.ToString().PadLeft(2, '0') + localDate.Day.ToString().PadLeft(2, '0') + localDate.Month.ToString().PadLeft(2, '0') + localDate.Year + id.ToString().PadLeft(4, '0');
            command.CommandText = "UPDATE cortes SET folio_corte = @setFolio WHERE id = @setId";
            command.Parameters.AddWithValue("setId", id);
            command.Parameters.AddWithValue("setFolio", folio);
            command.ExecuteNonQuery();

            return id;
        }

        public void completarCorte(int idcorte, int idsucursal, int idusuario, string fecha, double sobrante, Dictionary<string, string> data)
        {
            DateTime localDate = DateTime.Now;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE cortes SET sobrante = @setSobrante, fecha_corte_caja = @setFecha, sucursal_id = @setSucursal, usuario_id = @setUsuario, estado = 1, detalles = @setDetalles, total_apartados = @setTotalApartados, total_creditos = @setTotalCreditos, updated_at = @setFechaActual WHERE id = @setId";

            command.Parameters.AddWithValue("setId", idcorte);
            command.Parameters.AddWithValue("setSobrante", sobrante);
            command.Parameters.AddWithValue("setFecha", fecha);
            command.Parameters.AddWithValue("setSucursal", idsucursal);
            command.Parameters.AddWithValue("setUsuario", idusuario);
            command.Parameters.AddWithValue("setDetalles", "Pendiente de envío");
            command.Parameters.AddWithValue("setTotalApartados", data.ContainsKey("total_apartados") ? data["total_apartados"] : "0");
            command.Parameters.AddWithValue("setTotalCreditos", data.ContainsKey("total_creditos") ? data["total_creditos"] : "0");
            command.Parameters.AddWithValue("setFechaActual", localDate.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        }

        //Agregar los parametros de total_apartados y total_creditos
        public Dictionary<string, string> getCorte(int id)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM cortes WHERE id = @setId";
            command.Parameters.AddWithValue("setId", id);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data["fondo_apertura"] = result.GetDouble(2).ToString("0.00");
                data["total_efectivo"] = result.GetDouble(3).ToString("0.00");
                data["folio_corte"] = result.IsDBNull(1) ? "-" : result.GetString(1);
                data["total_tarjetas_debito"] = result.GetDouble(4).ToString("0.00");
                data["total_tarjetas_credito"] = result.GetDouble(5).ToString("0.00");
                data["total_cheques"] = result.GetDouble(6).ToString("0.00");
                data["total_transferencias"] = result.GetDouble(7).ToString("0.00");
                data["efectivo_apartados"] = result.GetDouble(8).ToString("0.00");
                data["efectivo_creditos"] = result.GetDouble(9).ToString("0.00");
                data["gastos"] = result.GetString(10);
                data["ingresos"] = result.GetString(11);
                data["sobrante"] = result.GetDouble(12).ToString("0.00");
                data["fecha_apertura_caja"] = result.GetString(13);
                data["fecha_corte_caja"] = result.IsDBNull(14) ? "-" : result.GetString(14);
                data["sucursal_id"] = result.IsDBNull(15) ? "-" : result.GetInt32(15).ToString();
                data["usuario_id"] = result.IsDBNull(16) ? "-" : result.GetInt32(16).ToString();
                data["estado"] = result.GetInt32(17).ToString();
                data["detalles"] = "Enviado";
                return data;
            }
            return null;
        }
        /*public Dictionary<string, string> getCorte2(int id)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM cortes WHERE id = @setId";
            command.Parameters.AddWithValue("setId", id);
            SQLiteDataReader result = command.ExecuteReader();

            if (result.Read())
            {
                Dictionary<string, string> data = new Dictionary<string, string>();

                // Cargar datos de la tabla "cortes"
                data["fondo_apertura"] = result.GetDouble(2).ToString("0.00");
                data["total_efectivo"] = result.GetDouble(3).ToString("0.00");
                data["folio_corte"] = result.IsDBNull(1) ? "-" : result.GetString(1);
                data["total_tarjetas_debito"] = result.GetDouble(4).ToString("0.00");
                data["total_tarjetas_credito"] = result.GetDouble(5).ToString("0.00");
                data["total_cheques"] = result.GetDouble(6).ToString("0.00");
                data["total_transferencias"] = result.GetDouble(7).ToString("0.00");
                data["efectivo_apartados"] = result.GetDouble(8).ToString("0.00");
                data["efectivo_creditos"] = result.GetDouble(9).ToString("0.00");
                data["gastos"] = result.GetString(10);
                data["ingresos"] = result.GetString(11);
                data["sobrante"] = result.GetDouble(12).ToString("0.00");
                data["fecha_apertura_caja"] = result.GetString(13);
                data["fecha_corte_caja"] = result.IsDBNull(14) ? "-" : result.GetString(14);
                data["sucursal_id"] = result.IsDBNull(15) ? "-" : result.GetInt32(15).ToString();
                data["usuario_id"] = result.IsDBNull(16) ? "-" : result.GetInt32(16).ToString();
                data["estado"] = result.GetInt32(17).ToString();
                data["detalles"] = "Enviado";

                result.Close();  // Cerrar el lector antes de hacer nuevas consultas.

                // Obtener la suma de total_abonado de la tabla abonos_apartado
                command.CommandText = "SELECT SUM(total_abonado) FROM abonos_apartado WHERE folio_corte = @setFolioCorte";
                command.Parameters.AddWithValue("@setFolioCorte", data["folio_corte"]);  // Aquí usas el folio_corte
                object resultApartados = command.ExecuteScalar();
                double totalApartados = (resultApartados == DBNull.Value || resultApartados == null) ? 0 : Convert.ToDouble(resultApartados);
                data["total_apartados"] = totalApartados.ToString("0.00");

                // Obtener la suma de total_abonado de la tabla abonos_credito
                command.CommandText = "SELECT SUM(total_abonado) FROM abonos_credito WHERE folio_corte = @setFolioCorte";
                object resultCreditos = command.ExecuteScalar();
                double totalCreditos = (resultCreditos == DBNull.Value || resultCreditos == null) ? 0 : Convert.ToDouble(resultCreditos);
                data["total_creditos"] = totalCreditos.ToString("0.00");

                return data;
            }

            return null;
        }*/
        public Dictionary<string, string> getCorte2(int id)
        {
            SQLiteCommand command = connection.CreateCommand();
            // Modificación 1:  Añadir total_apartados y total_creditos al SELECT query
            command.CommandText = "SELECT *, total_apartados, total_creditos FROM cortes WHERE id = @setId";
            command.Parameters.AddWithValue("setId", id);
            SQLiteDataReader result = command.ExecuteReader();

            if (result.Read())
            {
                Dictionary<string, string> data = new Dictionary<string, string>();

                // Cargar datos de la tabla "cortes"
                data["fondo_apertura"] = result.GetDouble(2).ToString("0.00");
                data["total_efectivo"] = result.GetDouble(3).ToString("0.00");
                data["folio_corte"] = result.IsDBNull(1) ? "-" : result.GetString(1);
                data["total_tarjetas_debito"] = result.GetDouble(4).ToString("0.00");
                data["total_tarjetas_credito"] = result.GetDouble(5).ToString("0.00");
                data["total_cheques"] = result.GetDouble(6).ToString("0.00");
                data["total_transferencias"] = result.GetDouble(7).ToString("0.00");
                data["efectivo_apartados"] = result.GetDouble(8).ToString("0.00");
                data["efectivo_creditos"] = result.GetDouble(9).ToString("0.00");
                data["gastos"] = result.GetString(10);
                data["ingresos"] = result.GetString(11);
                data["sobrante"] = result.GetDouble(12).ToString("0.00");
                data["fecha_apertura_caja"] = result.GetString(13);
                data["fecha_corte_caja"] = result.IsDBNull(14) ? "-" : result.GetString(14);
                data["sucursal_id"] = result.IsDBNull(15) ? "-" : result.GetInt32(15).ToString();
                data["usuario_id"] = result.IsDBNull(16) ? "-" : result.GetInt32(16).ToString();
                data["estado"] = result.GetInt32(17).ToString();
                data["detalles"] = "Enviado";


                // Modificación 2: Obtener total_apartados y total_creditos directamente del resultado
                // Contando las columnas antes de total_apartados y total_creditos en el CREATE TABLE,
                // sabemos que total_apartados estará en el índice 21 y total_creditos en el índice 22
                data["total_apartados"] = result.GetDouble(21).ToString("0.00"); // Índice 21 para total_apartados
                data["total_creditos"] = result.GetDouble(22).ToString("0.00");  // Índice 22 para total_creditos

                result.Close();  // Cerrar el lector

                // Modificación 3: Eliminar las consultas extra a abonos_apartado y abonos_credito
                // Ya no son necesarias porque total_apartados y total_creditos están en la tabla 'cortes'

                return data;
            }

            return null;
        }
        public bool actualizarTotalesCorte(int idCorte, decimal cantidad, bool esApartado)
        {

            string columnaActualizar = "";
            try
            {
                SQLiteCommand command = connection.CreateCommand();

                if (esApartado)
                {
                    columnaActualizar = "total_apartados";
                }
                else
                {
                    columnaActualizar = "total_creditos";
                }

                // Utiliza parámetros para evitar inyección SQL y para trabajar con decimales correctamente
                command.CommandText = $"UPDATE cortes SET {columnaActualizar} = {columnaActualizar} + @cantidad WHERE id = @idCorte";
                command.Parameters.AddWithValue("@idCorte", idCorte);
                command.Parameters.AddWithValue("@cantidad", cantidad); // Usar AddWithValue para decimal

                int filasActualizadas = command.ExecuteNonQuery();

                return filasActualizadas > 0; // Devuelve true si al menos una fila fue actualizada, false si no.

            }
            catch (Exception ex)
            {
                // En caso de error, puedes registrar el error o manejarlo de otra manera.
                // Aquí simplemente devolvemos false y podrías, opcionalmente, registrar el error.
                Console.WriteLine($"Error al actualizar {columnaActualizar} del corte con ID {idCorte}: {ex.Message}");
                return false;
            }
        }
        public List<Dictionary<string, string>> GetPendingCortes()
        {
            List<Dictionary<string, string>> cortes = new List<Dictionary<string, string>>();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM cortes WHERE detalles = 'Pendiente de envío'";
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    ["id"] = result.GetInt32(0).ToString(),
                    ["folio_corte"] = result.IsDBNull(1) ? "-" : result.GetString(1),
                    ["fondo_apertura"] = result.GetDouble(2).ToString("0.00"),
                    ["total_efectivo"] = result.GetDouble(3).ToString("0.00"),
                    ["total_tarjetas_debito"] = result.GetDouble(4).ToString("0.00"),
                    ["total_tarjetas_credito"] = result.GetDouble(5).ToString("0.00"),
                    ["total_cheques"] = result.GetDouble(6).ToString("0.00"),
                    ["total_transferencias"] = result.GetDouble(7).ToString("0.00"),
                    ["efectivo_apartados"] = result.GetDouble(8).ToString("0.00"),
                    ["efectivo_creditos"] = result.GetDouble(9).ToString("0.00"),
                    ["gastos"] = result.GetString(10),
                    ["ingresos"] = result.GetString(11),
                    ["sobrante"] = result.GetDouble(12).ToString("0.00"),
                    ["fecha_apertura_caja"] = result.GetString(13),
                    ["fecha_corte_caja"] = result.IsDBNull(14) ? "-" : result.GetString(14),
                    ["sucursal_id"] = result.IsDBNull(15) ? "-" : result.GetInt32(15).ToString(),
                    ["usuario_id"] = result.IsDBNull(16) ? "-" : result.GetInt32(16).ToString(),
                    ["estado"] = result.GetInt32(17).ToString()
                };
                cortes.Add(data);
            }
            return cortes;
        }

        public void UpdateCorteDetalles(int id, string detalles)
        {
            SQLiteCommand updateCommand = connection.CreateCommand();
            updateCommand.CommandText = "UPDATE cortes SET detalles = @detalles WHERE id = @id";
            updateCommand.Parameters.AddWithValue("id", id);
            updateCommand.Parameters.AddWithValue("detalles", detalles);
            updateCommand.ExecuteNonQuery();
        }

        public double getTotalApartados(string foliocorte)
{
    double res = 0;

    // Primer comando para obtener datos de la tabla abonos_apartado
    using (SQLiteCommand command = new SQLiteCommand(connection))
    {
        command.CommandText = "SELECT total_abonado FROM abonos_apartado WHERE folio_corte = @setFolioCorte";
        command.Parameters.AddWithValue("@setFolioCorte", foliocorte);

        using (SQLiteDataReader result = command.ExecuteReader())
        {
            while (result.Read())
            {
                res += result.GetDouble(0);
            }
        }
    }

    // Segundo comando para obtener datos de la tabla abonos_apartado_temporal
    using (SQLiteCommand command = new SQLiteCommand(connection))
    {
        command.CommandText = "SELECT total_abonado FROM abonos_apartado_temporal WHERE folio_corte = @setFolioCorte";
        command.Parameters.AddWithValue("@setFolioCorte", foliocorte);

        using (SQLiteDataReader result = command.ExecuteReader())
        {
            while (result.Read())
            {
                res += result.GetDouble(0);
            }
        }
    }

    return res;
}

        public double getTotalCreditos(string foliocorte)
        {
            double res = 0;

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT total_abonado FROM abonos_credito WHERE folio_corte = @setFolio";
            command.Parameters.AddWithValue("setFolio", foliocorte);
            SQLiteDataReader result = command.ExecuteReader();
            while (result.Read())
            {
                res += result.GetDouble(0);
            }
            command.Reset();
            command.CommandText = "SELECT total_abonado FROM abonos_credito_temporal WHERE folio_corte = @setFolio";
            command.Parameters.AddWithValue("setFolio", foliocorte);
            result = command.ExecuteReader();
            while (result.Read())
            {
                res += result.GetDouble(0);
            }

            return res;
        }
        public void acumularPagos(Dictionary<string, double> pagos, int idcorte, double cambio)
        {
            try
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE cortes SET total_efectivo = total_efectivo + @setEfectivo - @setCambio, " +
                        "total_tarjetas_debito = total_tarjetas_debito + @setDebito, " +
                        "total_tarjetas_credito = total_tarjetas_credito + @setCredito, " +
                        "total_cheques = total_cheques + @setCheques, " +
                        "total_transferencias = total_transferencias + @setTransferencias " +
                        "WHERE id = @setId";

                    command.Parameters.AddWithValue("@setId", idcorte);
                    command.Parameters.AddWithValue("@setEfectivo", pagos.ContainsKey("efectivo") ? pagos["efectivo"] : 0);
                    command.Parameters.AddWithValue("@setDebito", pagos.ContainsKey("debito") ? pagos["debito"] : 0);
                    command.Parameters.AddWithValue("@setCredito", pagos.ContainsKey("credito") ? pagos["credito"] : 0);
                    command.Parameters.AddWithValue("@setCheques", pagos.ContainsKey("cheque") ? pagos["cheque"] : 0);
                    command.Parameters.AddWithValue("@setTransferencias", pagos.ContainsKey("transferencia") ? pagos["transferencia"] : 0);
                    command.Parameters.AddWithValue("@setCambio", cambio);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        Console.WriteLine("No se actualizó ningún registro. Verifica el idcorte.");
                    }
                    else
                    {
                        Console.WriteLine("Registro actualizado correctamente.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al actualizar pagos: " + ex.Message);
            }
        }



        public void acumularEfectivoApartado(double efectivo, int idcorte)
        {
            //Console.WriteLine("Metodo acumular Efecivo A:" + efectivo);
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE cortes SET efectivo_apartados = efectivo_apartados + @setEfectivo WHERE id = @setId";
            command.Parameters.AddWithValue("setId", idcorte);
            command.Parameters.AddWithValue("setEfectivo", efectivo);
            command.ExecuteNonQuery();
        }
        public void acumularEfectivoCredito(double efectivo, int idcorte)
        {
            //Console.WriteLine("Metodo acumular Efecivo C:" + efectivo);
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE cortes SET efectivo_creditos = efectivo_creditos + @setEfectivo WHERE id = @setId";
            command.Parameters.AddWithValue("setId", idcorte);
            command.Parameters.AddWithValue("setEfectivo", efectivo);
            command.ExecuteNonQuery();
        }

        /*
        public void registrarIngreso(string concepto, double monto, int idcorte)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT ingresos FROM cortes WHERE id = @setId";
            command.Parameters.AddWithValue("setId", idcorte);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                var temp = JsonConvert.DeserializeObject<Dictionary<string, double>>(result.GetString(0));
                if (temp.ContainsKey(concepto))
                    temp[concepto] += monto;
                else
                    temp.Add(concepto, monto);
                command.Reset();
                command.CommandText = "UPDATE cortes SET  ingresos = @setIngresos WHERE id = @setId";
                //borre: total_efectivo = total_efectivo + @setMonto,
                //command.Parameters.AddWithValue("setMonto", monto);
                command.Parameters.AddWithValue("setId", idcorte);
                command.Parameters.AddWithValue("setIngresos", JsonConvert.SerializeObject(temp));
                command.ExecuteNonQuery();
            }
        }
        public void registrarGasto(string concepto, double monto, int idcorte)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT gastos FROM cortes WHERE id = @setId";
            command.Parameters.AddWithValue("setId", idcorte);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                var temp = JsonConvert.DeserializeObject<Dictionary<string, double>>(result.GetString(0));
                if (temp.ContainsKey(concepto))
                    temp[concepto] += monto;
                else
                    temp.Add(concepto, monto);
                command.Reset();
                command.CommandText = "UPDATE cortes SET  gastos = @setGastos WHERE id = @setId";
                //quite: total_efectivo = total_efectivo - @setMonto,
                //command.Parameters.AddWithValue("setMonto", monto);
                command.Parameters.AddWithValue("setId", idcorte);
                command.Parameters.AddWithValue("setGastos", JsonConvert.SerializeObject(temp));
                command.ExecuteNonQuery();
            }
        }
        */
        public void registrarIngreso(string concepto, double monto, int idcorte)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT ingresos, total_efectivo FROM cortes WHERE id = @setId";
            command.Parameters.AddWithValue("setId", idcorte);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                var temp = JsonConvert.DeserializeObject<Dictionary<string, double>>(result.GetString(0));
                double total_efectivo = result.GetDouble(1);
                if (temp.ContainsKey(concepto))
                    temp[concepto] += monto;
                else
                    temp.Add(concepto, monto);
                command.Reset();
                command.CommandText = "UPDATE cortes SET total_efectivo = @setTotalEfectivo, ingresos = @setIngresos WHERE id = @setId";
                command.Parameters.AddWithValue("setTotalEfectivo", total_efectivo + monto);
                command.Parameters.AddWithValue("setId", idcorte);
                command.Parameters.AddWithValue("setIngresos", JsonConvert.SerializeObject(temp));
                command.ExecuteNonQuery();
            }
        }

        public void registrarGasto(string concepto, double monto, int idcorte)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT gastos, total_efectivo FROM cortes WHERE id = @setId";
            command.Parameters.AddWithValue("setId", idcorte);
            SQLiteDataReader result = command.ExecuteReader();
            if (result.Read())
            {
                var temp = JsonConvert.DeserializeObject<Dictionary<string, double>>(result.GetString(0));
                double total_efectivo = result.GetDouble(1);
                result.Close(); // Cierra el DataReader antes de ejecutar otro comando

                if (temp.ContainsKey(concepto))
                    temp[concepto] += monto;
                else
                    temp.Add(concepto, monto);

                command.CommandText = "UPDATE cortes SET total_efectivo = @setTotalEfectivo, gastos = @setGastos WHERE id = @setId";
                command.Parameters.AddWithValue("setTotalEfectivo", total_efectivo - monto);
                command.Parameters.AddWithValue("setGastos", JsonConvert.SerializeObject(temp));
                command.ExecuteNonQuery();
            }
            else
            {
                result.Close(); // Asegúrate de cerrar el DataReader si no se leen datos
            }
        }

        //public void guardarSalidaTemporal(Salida salida, int idsalida)
        //{
        //    connection.Open();
        //    SQLiteCommand command = connection.CreateCommand();
        //    command.CommandText = "UPDATE salidas_temporal SET id_sucursal_origen = @setOrigen, id_sucursal_destino = @setDestino, productos = @setProductos, folio = @setFolio," +
        //        " fecha_salida = @setFecha, usuario_id = @setUsuario, total_importe = @setTotal, estado = @setEstado WHERE id = @setId";
        //    command.Parameters.AddWithValue("setOrigen", salida.id_sucursal_origen);
        //    command.Parameters.AddWithValue("setDestino", salida.id_sucursal_destino);
        //    command.Parameters.AddWithValue("setProductos", salida.productos);
        //    command.Parameters.AddWithValue("setFolio", salida.folio);
        //    command.Parameters.AddWithValue("setFecha", salida.fecha_salida);
        //    command.Parameters.AddWithValue("setUsuario", salida.usuario_id);
        //    command.Parameters.AddWithValue("setTotal", salida.total_importe);
        //    command.Parameters.AddWithValue("setEstado", 1);
        //    command.Parameters.AddWithValue("setId", idsalida);
        //    command.ExecuteNonQuery();
        //    connection.Close();
        //}
        //public void confirmarSalidaTemporal(int idsalida)
        //{
        //    connection.Open();
        //    SQLiteCommand command = connection.CreateCommand();
        //    command.CommandText = "UPDATE salidas_temporal SET estado = 2 WHERE id = @setID";
        //    command.Parameters.AddWithValue("setId", idsalida);
        //    command.ExecuteNonQuery();
        //    connection.Close();
        //}

        // Reemplazar completamente el método imprimirTicket en LocalDataManager
        public bool imprimirTicket(Dictionary<string, string> venta, List<ProductoVenta> productos, Dictionary<string, double> pagos,
            string cajero, string sucursalName, string sucursalDir, bool re, double cambio, bool esDescuento, double descuento)
        {
            double total = double.Parse(venta["total"].ToString());
            bool state = false;
            int art = 0;

            if (!impresora.Equals(""))
            {
                state = true;
                CreaTicket Ticket1 = new CreaTicket();
                Ticket1.impresora = impresora;

                if (!re)
                    Ticket1.AbreCajon();

                Ticket1.TextoCentro("CASA CEJA");
                Ticket1.TextoCentro("Sucursal: " + sucursalName.ToUpper());
                Ticket1.TextoCentro(sucursalDir.ToUpper());
                Ticket1.TextoCentro(venta["fecha_venta"]);
                Ticket1.TextoCentro("FOLIO: " + venta["folio"]);
                Ticket1.TextoCentro(" ");
                Ticket1.EncabezadoVenta();

                // CALCULAR SUBTOTAL SIN DESCUENTOS y DESCUENTOS TOTALES
                double subtotalSinDescuentos = 0;
                double totalDescuentoCategoria = 0;
                double totalDescuentoPrecioEspecial = 0;

                // MOSTRAR productos con indicadores y calcular totales
                foreach (ProductoVenta p in productos)
                {
                    art++;
                    string nombreProducto = p.nombre;

                    // AGREGAR INDICADORES de descuentos
                    string indicadores = "";

                    if (p.es_precio_especial)
                    {
                        indicadores += "*ESP";
                    }

                    // MOSTRAR INDICADOR DE CATEGORÍA SI LO TUVO ORIGINALMENTE
                    if (p.tuvo_descuento_categoria_original)
                    {
                        if (indicadores.Length > 0) indicadores += " ";
                        indicadores += $"*CAT{p.porcentaje_categoria_original:0}%";
                    }
                    else if (p.es_descuento_categoria)
                    {
                        if (indicadores.Length > 0) indicadores += " ";
                        indicadores += $"*CAT{p.porcentaje_descuento_categoria:0}%";
                    }

                    nombreProducto += indicadores;

                    // PRECIO ORIGINAL (sin descuentos) - P.UNIT
                    double precioUnitarioOriginal;
                    if (p.precio_original > 0)
                    {
                        precioUnitarioOriginal = p.precio_original;
                    }
                    else
                    {
                        // Obtener producto completo para precio menudeo
                        Producto productoCompleto = GetProductByCode(p.codigo);
                        precioUnitarioOriginal = productoCompleto?.menudeo ?? p.precio_venta;
                    }

                    // PRECIO FINAL CON DESCUENTOS - P.TOTAL
                    double precioFinalConDescuentos = p.precio_venta * p.cantidad;

                    // ACUMULAR para subtotal
                    subtotalSinDescuentos += precioUnitarioOriginal * p.cantidad;

                    // *** ACUMULAR DESCUENTOS USANDO VALORES ORIGINALES ***
                    if (p.tuvo_descuento_categoria_original)
                    {
                        totalDescuentoCategoria += p.descuento_categoria_original * p.cantidad;
                    }
                    else if (p.es_descuento_categoria)
                    {
                        totalDescuentoCategoria += p.descuento_categoria_unitario * p.cantidad;
                    }

                    if (p.es_precio_especial)
                    {
                        totalDescuentoPrecioEspecial += p.descuento_unitario * p.cantidad;
                    }

                    Ticket1.AgregaArticulo(nombreProducto, p.cantidad, precioUnitarioOriginal, precioFinalConDescuentos);
                }

                Ticket1.LineasGuion();

                // MOSTRAR SUBTOTAL
                Ticket1.AgregaTotales("SUBTOTAL $", subtotalSinDescuentos);

                // MOSTRAR DESCUENTOS POR SEPARADO (solo si existen)
                if (totalDescuentoCategoria > 0)
                {
                    Ticket1.AgregaTotales("DESC. POR CATEGORIA", -totalDescuentoCategoria);
                }

                if (totalDescuentoPrecioEspecial > 0)
                {
                    Ticket1.AgregaTotales("DESC. PRECIO ESPECIAL", -totalDescuentoPrecioEspecial);
                }

                if (esDescuento && descuento > 0)
                {
                    Ticket1.AgregaTotales("DESCUENTO DE VENTA", -descuento);
                }

                // CALCULAR Y MOSTRAR TOTAL FINAL
                double totalFinalConTodosLosDescuentos = subtotalSinDescuentos - totalDescuentoCategoria - totalDescuentoPrecioEspecial - descuento;
                Ticket1.AgregaTotales("TOTAL FINAL $", totalFinalConTodosLosDescuentos);

                Ticket1.LineasGuion();

                // MÉTODOS DE PAGO
                if (pagos.ContainsKey("debito"))
                {
                    Ticket1.AgregaTotales("PAGO T. DEBITO", pagos["debito"]);
                    cambio -= pagos["debito"];
                }
                if (pagos.ContainsKey("credito"))
                {
                    Ticket1.AgregaTotales("PAGO T.CREDITO", pagos["credito"]);
                    cambio -= pagos["credito"];
                }
                if (pagos.ContainsKey("cheque"))
                {
                    Ticket1.AgregaTotales("PAGO CHEQUES", pagos["cheque"]);
                    cambio -= pagos["cheque"];
                }
                if (pagos.ContainsKey("transferencia"))
                {
                    Ticket1.AgregaTotales("PAGO TRANSFERENCIA", pagos["transferencia"]);
                    cambio -= pagos["transferencia"];
                }
                if (pagos.ContainsKey("efectivo"))
                {
                    Ticket1.AgregaTotales("EFECTIVO ENTREGADO", pagos["efectivo"]);
                    cambio -= pagos["efectivo"];
                }

                Ticket1.AgregaTotales("SU CAMBIO $", cambio * -1);
                Ticket1.LineasGuion();
                Ticket1.TextoCentro(" ");
                Ticket1.TextoCentro("LE ATENDIO: " + cajero.ToUpper());
                Ticket1.TextoCentro("NO DE ARTICULOS: " + art.ToString().PadLeft(5, '0'));
                Ticket1.TextoCentro("GRACIAS POR SU COMPRA");
                Ticket1.TextoCentro(" ");
                
                //Ticket1.TextoCentro("ANTONIO CEJA MARON");

                // Agregar RFC desde configuración
                string rfc = Settings.Default["rfc"].ToString();
                if (!string.IsNullOrEmpty(rfc))
                {
                    Ticket1.TextoCentro("RFC: " + rfc);
                    Ticket1.TextoCentro(" ");
                }

                // Agregar el pie de ticket si está disponible
                string piedeticket = Settings.Default["pieDeTicket"].ToString();
                if (!string.IsNullOrEmpty(piedeticket))
                {
                    Ticket1.LineasGuion();
                    Ticket1.TextoCentro(piedeticket);
                    Ticket1.LineasGuion();
                }

                Ticket1.TextoCentro("SI DESEA FACTURAR ESTA COMPRA INGRESE A");
                Ticket1.TextoCentro("https://cm-papeleria.com/public/facturacion");

                Ticket1.CortaTicket(); // corta el ticket
            }

            return state;
        }

        // Reemplazar completamente el método imprimirApartado en LocalDataManager
        public bool imprimirApartado(Apartado apartado, List<ProductoVenta> productos, Dictionary<string, double> pagos,
            string cajero, string sucursalName, string sucursalDir, string fechavencimiento, string clienteNombre, string clienteTelefono)
        {
            double total = (double)apartado.total;
            bool state = false;
            int art = 0;

            if (!impresora.Equals(""))
            {
                state = true;
                CreaTicket Ticket1 = new CreaTicket();
                Ticket1.impresora = impresora;
                Ticket1.AbreCajon();

                Ticket1.TextoCentro("CASA CEJA");
                Ticket1.TextoCentro("Sucursal: " + sucursalName.ToUpper());
                Ticket1.TextoCentro(sucursalDir.ToUpper());
                Ticket1.TextoCentro(apartado.fecha_de_apartado);
                Ticket1.TextoCentro("FOLIO: " + apartado.folio_corte);
                Ticket1.TextoCentro(" ");
                Ticket1.TextoCentro("TICKET DE APARTADO");
                Ticket1.EncabezadoVenta();

                // CALCULAR SUBTOTAL SIN DESCUENTOS y DESCUENTOS TOTALES
                double subtotalSinDescuentos = 0;
                double totalDescuentoCategoria = 0;
                double totalDescuentoPrecioEspecial = 0;

                // MOSTRAR productos con indicadores y calcular totales
                foreach (ProductoVenta p in productos)
                {
                    art++;
                    string nombreProducto = p.nombre;

                    // AGREGAR INDICADORES de descuentos
                    string indicadores = "";

                    if (p.es_precio_especial)
                    {
                        indicadores += "*ESP";
                    }

                    // MOSTRAR INDICADOR DE CATEGORÍA SI LO TUVO ORIGINALMENTE
                    if (p.tuvo_descuento_categoria_original)
                    {
                        if (indicadores.Length > 0) indicadores += " ";
                        indicadores += $"*CAT{p.porcentaje_categoria_original:0}%";
                    }

                    nombreProducto += indicadores;

                    // PRECIO ORIGINAL (sin descuentos) - P.UNIT
                    double precioUnitarioOriginal;
                    if (p.precio_original > 0)
                    {
                        precioUnitarioOriginal = p.precio_original;
                    }
                    else
                    {
                        // Obtener producto completo para precio menudeo
                        Producto productoCompleto = GetProductByCode(p.codigo);
                        precioUnitarioOriginal = productoCompleto?.menudeo ?? p.precio_venta;
                    }

                    // PRECIO FINAL CON DESCUENTOS - P.TOTAL
                    double precioFinalConDescuentos = p.precio_venta * p.cantidad;

                    // ACUMULAR para subtotal
                    subtotalSinDescuentos += precioUnitarioOriginal * p.cantidad;

                    // *** ACUMULAR DESCUENTOS USANDO VALORES ORIGINALES ***
                    if (p.tuvo_descuento_categoria_original)
                    {
                        totalDescuentoCategoria += p.descuento_categoria_original * p.cantidad;
                    }

                    if (p.es_precio_especial)
                    {
                        totalDescuentoPrecioEspecial += p.descuento_unitario * p.cantidad;
                    }

                    Ticket1.AgregaArticulo(nombreProducto, p.cantidad, precioUnitarioOriginal, precioFinalConDescuentos);
                }

                Ticket1.LineasGuion();

                // MOSTRAR SUBTOTAL
                Ticket1.AgregaTotales("SUBTOTAL $", subtotalSinDescuentos);

                // MOSTRAR DESCUENTOS POR SEPARADO (solo si existen)
                if (totalDescuentoCategoria > 0)
                {
                    Ticket1.AgregaTotales("DESC. POR CATEGORIA", -totalDescuentoCategoria);
                }

                if (totalDescuentoPrecioEspecial > 0)
                {
                    Ticket1.AgregaTotales("DESC. PRECIO ESPECIAL", -totalDescuentoPrecioEspecial);
                }

                // TOTAL FINAL
                Ticket1.AgregaTotales("TOTAL $", total);

                // MÉTODOS DE PAGO
                if (pagos.ContainsKey("debito"))
                {
                    Ticket1.AgregaTotales("PAGO T. DEBITO", pagos["debito"]);
                }
                if (pagos.ContainsKey("credito"))
                {
                    Ticket1.AgregaTotales("PAGO T.CREDITO", pagos["credito"]);
                }
                if (pagos.ContainsKey("cheque"))
                {
                    Ticket1.AgregaTotales("PAGO CHEQUES", pagos["cheque"]);
                }
                if (pagos.ContainsKey("transferencia"))
                {
                    Ticket1.AgregaTotales("PAGO TRANSFERENCIA", pagos["transferencia"]);
                }
                if (pagos.ContainsKey("efectivo"))
                {
                    Ticket1.AgregaTotales("EFECTIVO ENTREGADO", pagos["efectivo"]);
                }

                Ticket1.LineasGuion();
                Ticket1.AgregaTotales("POR PAGAR $", (apartado.total - apartado.total_pagado));
                Ticket1.TextoCentro(" ");
                Ticket1.TextoCentro("LE ATENDIO: " + cajero.ToUpper());
                Ticket1.TextoCentro("NO DE ARTICULOS: " + art.ToString().PadLeft(5, '0'));
                Ticket1.TextoCentro("FECHA DE VENCIMIENTO:");
                Ticket1.TextoCentro(fechavencimiento);
                Ticket1.TextoCentro("CLIENTE:");
                Ticket1.TextoCentro(clienteNombre);
                Ticket1.TextoCentro("NUMERO TELEFONICO:");
                Ticket1.TextoCentro(clienteTelefono);
                Ticket1.TextoCentro(" ");
                
                //Ticket1.TextoCentro("ANTONIO CEJA MARON");
                
                // Agregar RFC desde configuración
                string rfc = Settings.Default["rfc"].ToString();
                if (!string.IsNullOrEmpty(rfc))
                {
                    Ticket1.TextoCentro("RFC: " + rfc);
                    Ticket1.TextoCentro(" ");
                }

                // Agregar el pie de ticket si está disponible
                string piedeticket = Settings.Default["pieDeTicket"].ToString();
                if (!string.IsNullOrEmpty(piedeticket))
                {
                    Ticket1.LineasGuion();
                    Ticket1.TextoCentro(piedeticket);
                    Ticket1.LineasGuion();
                }

                Ticket1.TextoCentro("SI DESEA FACTURAR ESTA COMPRA INGRESE A");
                Ticket1.TextoCentro("https://cm-papeleria.com/public/facturacion");

                Ticket1.CortaTicket(); // corta el ticket
            }

            return state;
        }

        // Reemplazar completamente el método imprimirCredito en LocalDataManager
        public bool imprimirCredito(Credito credito, List<ProductoVenta> productos, Dictionary<string, double> pagos,
            string cajero, string sucursalName, string sucursalDir, string fechavencimiento, string clienteNombre, string clienteTelefono)
        {
            double total = (double)credito.total;
            bool state = false;
            int art = 0;

            if (!impresora.Equals(""))
            {
                state = true;
                CreaTicket Ticket1 = new CreaTicket();
                Ticket1.impresora = impresora;
                Ticket1.AbreCajon();

                Ticket1.TextoCentro("CASA CEJA");
                Ticket1.TextoCentro("Sucursal: " + sucursalName.ToUpper());
                Ticket1.TextoCentro(sucursalDir.ToUpper());
                Ticket1.TextoCentro(credito.fecha_de_credito);
                Ticket1.TextoCentro("FOLIO: " + credito.folio);
                Ticket1.TextoCentro(" ");
                Ticket1.TextoCentro("TICKET DE CREDITO");
                Ticket1.EncabezadoVenta();

                // CALCULAR SUBTOTAL SIN DESCUENTOS y DESCUENTOS TOTALES
                double subtotalSinDescuentos = 0;
                double totalDescuentoCategoria = 0;
                double totalDescuentoPrecioEspecial = 0;

                // MOSTRAR productos con indicadores y calcular totales
                foreach (ProductoVenta p in productos)
                {
                    art++;
                    string nombreProducto = p.nombre;

                    // AGREGAR INDICADORES de descuentos
                    string indicadores = "";

                    if (p.es_precio_especial)
                    {
                        indicadores += "*ESP";
                    }

                    // MOSTRAR INDICADOR DE CATEGORÍA SI LO TUVO ORIGINALMENTE
                    if (p.tuvo_descuento_categoria_original)
                    {
                        if (indicadores.Length > 0) indicadores += " ";
                        indicadores += $"*CAT{p.porcentaje_categoria_original:0}%";
                    }

                    nombreProducto += indicadores;

                    // PRECIO ORIGINAL (sin descuentos) - P.UNIT
                    double precioUnitarioOriginal;
                    if (p.precio_original > 0)
                    {
                        precioUnitarioOriginal = p.precio_original;
                    }
                    else
                    {
                        // Obtener producto completo para precio menudeo
                        Producto productoCompleto = GetProductByCode(p.codigo);
                        precioUnitarioOriginal = productoCompleto?.menudeo ?? p.precio_venta;
                    }

                    // PRECIO FINAL CON DESCUENTOS - P.TOTAL
                    double precioFinalConDescuentos = p.precio_venta * p.cantidad;

                    // ACUMULAR para subtotal
                    subtotalSinDescuentos += precioUnitarioOriginal * p.cantidad;

                    // *** ACUMULAR DESCUENTOS USANDO VALORES ORIGINALES ***
                    if (p.tuvo_descuento_categoria_original)
                    {
                        totalDescuentoCategoria += p.descuento_categoria_original * p.cantidad;
                    }

                    if (p.es_precio_especial)
                    {
                        totalDescuentoPrecioEspecial += p.descuento_unitario * p.cantidad;
                    }

                    Ticket1.AgregaArticulo(nombreProducto, p.cantidad, precioUnitarioOriginal, precioFinalConDescuentos);
                }

                Ticket1.LineasGuion();

                // MOSTRAR SUBTOTAL
                Ticket1.AgregaTotales("SUBTOTAL $", subtotalSinDescuentos);

                // MOSTRAR DESCUENTOS POR SEPARADO (solo si existen)
                if (totalDescuentoCategoria > 0)
                {
                    Ticket1.AgregaTotales("DESC. POR CATEGORIA", -totalDescuentoCategoria);
                }

                if (totalDescuentoPrecioEspecial > 0)
                {
                    Ticket1.AgregaTotales("DESC. PRECIO ESPECIAL", -totalDescuentoPrecioEspecial);
                }

                // TOTAL FINAL
                Ticket1.AgregaTotales("TOTAL $", total);

                // MÉTODOS DE PAGO
                if (pagos.ContainsKey("debito"))
                {
                    Ticket1.AgregaTotales("PAGO T. DEBITO", pagos["debito"]);
                }
                if (pagos.ContainsKey("credito"))
                {
                    Ticket1.AgregaTotales("PAGO T.CREDITO", pagos["credito"]);
                }
                if (pagos.ContainsKey("cheque"))
                {
                    Ticket1.AgregaTotales("PAGO CHEQUES", pagos["cheque"]);
                }
                if (pagos.ContainsKey("transferencia"))
                {
                    Ticket1.AgregaTotales("PAGO TRANSFERENCIA", pagos["transferencia"]);
                }
                if (pagos.ContainsKey("efectivo"))
                {
                    Ticket1.AgregaTotales("EFECTIVO ENTREGADO", pagos["efectivo"]);
                }

                Ticket1.LineasGuion();
                Ticket1.AgregaTotales("POR PAGAR $", (credito.total - credito.total_pagado));
                Ticket1.TextoCentro(" ");
                Ticket1.TextoCentro("LE ATENDIO: " + cajero.ToUpper());
                Ticket1.TextoCentro("NO DE ARTICULOS: " + art.ToString().PadLeft(5, '0'));
                Ticket1.TextoCentro("FECHA DE VENCIMIENTO:");
                Ticket1.TextoCentro(fechavencimiento);
                Ticket1.TextoCentro("CLIENTE:");
                Ticket1.TextoCentro(clienteNombre);
                Ticket1.TextoCentro("NUMERO TELEFONICO:");
                Ticket1.TextoCentro(clienteTelefono);
                Ticket1.TextoCentro(" ");
                
                //Ticket1.TextoCentro("ANTONIO CEJA MARON");
                
                // Agregar RFC desde configuración
                string rfc = Settings.Default["rfc"].ToString();
                if (!string.IsNullOrEmpty(rfc))
                {
                    Ticket1.TextoCentro("RFC: " + rfc);
                    Ticket1.TextoCentro(" ");
                }

                // Agregar el pie de ticket si está disponible
                string piedeticket = Settings.Default["pieDeTicket"].ToString();
                if (!string.IsNullOrEmpty(piedeticket))
                {
                    Ticket1.LineasGuion();
                    Ticket1.TextoCentro(piedeticket);
                    Ticket1.LineasGuion();
                }

                Ticket1.TextoCentro("SI DESEA FACTURAR ESTA COMPRA INGRESE A");
                Ticket1.TextoCentro("https://cm-papeleria.com/public/facturacion");

                Ticket1.CortaTicket(); // corta el ticket
            }

            return state;
        }
        public bool imprimirAbono(int tipo, Dictionary<string, double> pagos, string cajero, string sucursalName, string sucursalDir, string fecha, double abonado, double porpagar, string folioAbono, string folioOperacion)
        {
            bool state = false;
            if (!impresora.Equals(""))
            {
                state = true;
                CreaTicket Ticket1 = new CreaTicket();
                Ticket1.impresora = impresora;
                Ticket1.AbreCajon();
                Ticket1.TextoCentro("CASA CEJA");
                Ticket1.TextoCentro("Sucursal: " + sucursalName.ToUpper());
                Ticket1.TextoCentro(sucursalDir.ToUpper());
                Ticket1.TextoCentro(fecha);
                Ticket1.TextoCentro("FOLIO: " + folioAbono);
                Ticket1.TextoCentro("TICKET DE ABONO");
                Ticket1.TextoCentro(" ");
                Ticket1.TextoCentro("CONCEPTO:");
                if (tipo == 0)
                {
                    Ticket1.TextoCentro("CREDITO CON FOLIO: " + folioOperacion);
                }
                else
                {
                    Ticket1.TextoCentro("APARTADO CON FOLIO: " + folioOperacion);
                }

                Ticket1.LineasGuion();
                if (pagos.ContainsKey("debito"))
                {
                    Ticket1.AgregaTotales("PAGO T. DEBITO", pagos["debito"]);
                }
                if (pagos.ContainsKey("credito"))
                {
                    Ticket1.AgregaTotales("PAGO T.CREDITO", pagos["credito"]);
                }
                if (pagos.ContainsKey("cheque"))
                {
                    Ticket1.AgregaTotales("PAGO CHEQUES", pagos["cheque"]);
                }
                if (pagos.ContainsKey("transferencia"))
                {
                    Ticket1.AgregaTotales("PAGO TRANSFERENCIA", pagos["transferencia"]);
                }
                if (pagos.ContainsKey("efectivo"))
                {
                    Ticket1.AgregaTotales("EFECTIVO ENTREGADO", pagos["efectivo"]);
                }
                Ticket1.LineasGuion();
                Ticket1.AgregaTotales("TOTAL ABONADO", abonado);
                Ticket1.LineasGuion();
                Ticket1.AgregaTotales("POR PAGAR $", porpagar);

                Ticket1.TextoCentro(" ");
                Ticket1.TextoCentro("LE ATENDIO: " + cajero.ToUpper());
                Ticket1.TextoCentro("GRACIAS POR SU PREFERENCIA");
                Ticket1.TextoCentro(" ");
                
                // Agregar RFC desde configuración
                string rfc = Settings.Default["rfc"].ToString();
                if (!string.IsNullOrEmpty(rfc))
                {
                    Ticket1.TextoCentro("RFC: " + rfc);
                    Ticket1.TextoCentro(" ");
                }
                
                Ticket1.CortaTicket(); // corta el ticket
            }
            return state;
        }
        public void reimprimirAbonosCredito(Credito selCred, string sucursalName, string sucursalDir, string cajero)
        {
            if (selCred != null)
            {
                List<AbonoCredito> listaAbonosCredito = selCred.abonos;
                List<ProductoVenta> carrito = JsonConvert.DeserializeObject<List<ProductoVenta>>(selCred.productos);
                double totalcarrito = selCred.total;
                double totalpagado = selCred.total_pagado;
                string folio = selCred.folio;
                int idOperacion = selCred.id;
                int art = 0;
                if (!impresora.Equals(""))
                {
                    CreaTicket Ticket1 = new CreaTicket();
                    Ticket1.impresora = impresora;
                    Ticket1.AbreCajon();
                    Ticket1.TextoCentro("CASA CEJA");
                    Ticket1.TextoCentro("Sucursal: " + sucursalName.ToUpper());
                    Ticket1.TextoCentro(sucursalDir.ToUpper());
                    Ticket1.TextoCentro(selCred.fecha_de_credito);
                    Ticket1.TextoCentro("FOLIO: " + selCred.folio);
                    Ticket1.TextoCentro(" ");
                    Ticket1.TextoCentro("TICKET DE CREDITO");
                    Ticket1.EncabezadoVenta();
                    foreach (ProductoVenta p in carrito)
                    {
                        art++;
                        Ticket1.AgregaArticulo(p.nombre, p.cantidad, p.precio_venta, p.cantidad * p.precio_venta);
                    }
                    Ticket1.LineasGuion();
                    Ticket1.AgregaTotales("Total", selCred.total);
                    Ticket1.LineasGuion();
                    if (listaAbonosCredito != null)
                    {
                        if (listaAbonosCredito.Count > 0)
                        {
                            Ticket1.TextoCentro("HISTORIAL DE PAGOS:");
                        }
                        foreach (AbonoCredito abono in listaAbonosCredito)
                        {

                            Dictionary<string, double> pagos = JsonConvert.DeserializeObject<Dictionary<string, double>>(abono.metodo_pago);
                            if (pagos.ContainsKey("debito"))
                            {
                                Ticket1.AgregaTotales("PAGO T. DEBITO", double.Parse(pagos["debito"].ToString()));
                            }
                            if (pagos.ContainsKey("credito"))
                            {
                                Ticket1.AgregaTotales("PAGO T.CREDITO", double.Parse(pagos["credito"].ToString()));
                            }
                            if (pagos.ContainsKey("cheque"))
                            {
                                Ticket1.AgregaTotales("PAGO CHEQUES", double.Parse(pagos["cheque"].ToString()));
                            }
                            if (pagos.ContainsKey("transferencia"))
                            {
                                Ticket1.AgregaTotales("PAGO TRANSFERENCIA", double.Parse(pagos["transferencia"].ToString()));
                            }
                            if (pagos.ContainsKey("efectivo"))
                            {
                                Ticket1.AgregaTotales("EFECTIVO ENTREGADO", double.Parse(pagos["efectivo"].ToString()));
                            }
                            Ticket1.LineasGuion();
                        }
                    }
                    Ticket1.AgregaTotales("POR PAGAR $", (selCred.total - selCred.total_pagado));

                    Ticket1.TextoCentro(" ");
                    Ticket1.TextoCentro("LE ATENDIO: " + cajero.ToUpper());
                    Ticket1.TextoCentro("NO DE ARTICULOS: " + art.ToString().PadLeft(5, '0'));
                    Ticket1.TextoCentro(" ");

                    // Agregar RFC desde configuración
                    string rfc = Settings.Default["rfc"].ToString();
                    if (!string.IsNullOrEmpty(rfc))
                    {
                        Ticket1.TextoCentro("RFC: " + rfc);
                        Ticket1.TextoCentro(" ");
                    }

                    Ticket1.CortaTicket(); // corta el ticket
                }
            }
        }
        public void endDatabase()
        {
            connection.Close();
            connection.Dispose();
        }
        public void reimprimirAbonosApartado(Apartado selApa, string sucursalName, string sucursalDir, string cajero)
        {
            if (selApa != null)
            {
                List<AbonoApartado> listaAbonosApartado = selApa.abonos;
                List<ProductoVenta> carrito = JsonConvert.DeserializeObject<List<ProductoVenta>>(selApa.productos);
                double totalcarrito = selApa.total;
                double totalpagado = selApa.total_pagado;
                string folio = selApa.folio_corte;
                int idOperacion = selApa.id;
                int art = 0;
                if (!impresora.Equals(""))
                {
                    CreaTicket Ticket1 = new CreaTicket();
                    Ticket1.impresora = impresora;
                    Ticket1.AbreCajon();
                    Ticket1.TextoCentro("CASA CEJA");
                    Ticket1.TextoCentro("Sucursal: " + sucursalName.ToUpper());
                    Ticket1.TextoCentro(sucursalDir.ToUpper());
                    Ticket1.TextoCentro(selApa.fecha_de_apartado);
                    Ticket1.TextoCentro("FOLIO: " + selApa.folio_corte);
                    Ticket1.TextoCentro(" ");
                    Ticket1.TextoCentro("TICKET DE APARTADO");
                    Ticket1.EncabezadoVenta();
                    foreach (ProductoVenta p in carrito)
                    {
                        art++;
                        Ticket1.AgregaArticulo(p.nombre, p.cantidad, p.precio_venta, p.cantidad * p.precio_venta);
                    }
                    Ticket1.LineasGuion();
                    Ticket1.AgregaTotales("Total", selApa.total);
                    Ticket1.LineasGuion();
                    if (listaAbonosApartado != null)
                    {
                        if (listaAbonosApartado.Count > 0)
                        {
                            Ticket1.TextoCentro("HISTORIAL DE PAGOS:");
                        }
                        foreach (AbonoApartado abono in listaAbonosApartado)
                        {

                            Dictionary<string, double> pagos = JsonConvert.DeserializeObject<Dictionary<string, double>>(abono.metodo_pago);
                            if (pagos.ContainsKey("debito"))
                            {
                                Ticket1.AgregaTotales("PAGO T. DEBITO", double.Parse(pagos["debito"].ToString()));
                            }
                            if (pagos.ContainsKey("credito"))
                            {
                                Ticket1.AgregaTotales("PAGO T.CREDITO", double.Parse(pagos["credito"].ToString()));
                            }
                            if (pagos.ContainsKey("cheque"))
                            {
                                Ticket1.AgregaTotales("PAGO CHEQUES", double.Parse(pagos["cheque"].ToString()));
                            }
                            if (pagos.ContainsKey("transferencia"))
                            {
                                Ticket1.AgregaTotales("PAGO TRANSFERENCIA", double.Parse(pagos["transferencia"].ToString()));
                            }
                            if (pagos.ContainsKey("efectivo"))
                            {
                                Ticket1.AgregaTotales("EFECTIVO ENTREGADO", double.Parse(pagos["efectivo"].ToString()));
                            }
                            Ticket1.LineasGuion();
                        }
                    }
                    Ticket1.AgregaTotales("POR PAGAR $", (selApa.total - selApa.total_pagado));

                    Ticket1.TextoCentro(" ");
                    Ticket1.TextoCentro("LE ATENDIO: " + cajero.ToUpper());
                    Ticket1.TextoCentro("NO DE ARTICULOS: " + art.ToString().PadLeft(5, '0'));
                    Ticket1.TextoCentro(" ");

                    // Agregar RFC desde configuración
                    string rfc = Settings.Default["rfc"].ToString();
                    if (!string.IsNullOrEmpty(rfc))
                    {
                        Ticket1.TextoCentro("RFC: " + rfc);
                        Ticket1.TextoCentro(" ");
                    }

                    Ticket1.CortaTicket(); // corta el ticket
                }
            }
        }
    }
}
