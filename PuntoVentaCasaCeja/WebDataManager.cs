using Newtonsoft.Json;
using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Windows.Storage;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Data;
using System.Text;
using Newtonsoft.Json.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Linq;
using System.Diagnostics;

namespace PuntoVentaCasaCeja
{
    public class WebDataManager
    {
        static HttpClient client;
        public LocaldataManager localDM;
        string url;
        string productos_lastupdate;
        string categorias_lastupdate;
        string medidas_lastupdate;
        string usuarios_lastupdate;
        string proveedores_lastupdate;
        string apartados_lastupdate;
        string creditos_lastupdate;
        string abonos_credito_lu;
        string abonos_apartado_lu;
        string sucursales_lastupdate;
        string clientes_lastupdate;
        String cortes_lastupdate;
        public int sucursal_id;
        Action<int> refreshData;
        public Usuario activeUser;
        public WebDataManager(LocaldataManager localdataManager, Action<int> RefreshData, Usuario ActiveUser)
        {
            this.activeUser = ActiveUser;
            this.localDM = localdataManager;
            this.refreshData = RefreshData;
            //url = "https://jorobadonciador/";
            //url = "https://f832-187-254-98-104.ngrok.io/";
            url = "https://cm-papeleria.com/public/";
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Inicialización optimizada para tablas precargadas           
            if (localDM.IsCatalogPreloaded)
            {
                Console.WriteLine("Obteniendo fecha de actualizacion para base de datos precargada");
                productos_lastupdate = localDM.getTableLastUpdate("productos");
                categorias_lastupdate = localDM.getTableLastUpdate("categorias");
                medidas_lastupdate = localDM.getTableLastUpdate("medidas");
            }
            else
            {
                // Comportamiento normal para instalaciones sin precarga
                Console.WriteLine("Obteniendo fecha de actualizacion para base de datos normal");
                productos_lastupdate = localDM.getTableLastUpdate("productos");
                categorias_lastupdate = localDM.getTableLastUpdate("categorias");
                medidas_lastupdate = localDM.getTableLastUpdate("medidas");
            }            
            proveedores_lastupdate = localDM.getTableLastUpdate("proveedores");
            usuarios_lastupdate = localDM.getTableLastUpdate("usuarios");            
            sucursales_lastupdate = localDM.getTableLastUpdate("sucursales");
            apartados_lastupdate = localDM.getTableLastUpdate("apartados");
            creditos_lastupdate = localDM.getTableLastUpdate("creditos");
            clientes_lastupdate = localDM.getTableLastUpdate("clientes");
            abonos_credito_lu = localDM.getTableLastUpdate("abonos_credito");
            abonos_apartado_lu = localDM.getTableLastUpdate("abonos_apartado");
            cortes_lastupdate = localDM.getTableLastUpdate("cortes");
        }
        public void resetDates()
        {
            productos_lastupdate = localDM.getTableLastUpdate("productos");
            categorias_lastupdate = localDM.getTableLastUpdate("categorias");
            medidas_lastupdate = localDM.getTableLastUpdate("medidas");
            usuarios_lastupdate = localDM.getTableLastUpdate("usuarios");
            proveedores_lastupdate = localDM.getTableLastUpdate("proveedores");
            sucursales_lastupdate = localDM.getTableLastUpdate("sucursales");
            apartados_lastupdate = localDM.getTableLastUpdate("apartados");
            creditos_lastupdate = localDM.getTableLastUpdate("creditos");
            abonos_credito_lu = localDM.getTableLastUpdate("abonos_credito");
            abonos_apartado_lu = localDM.getTableLastUpdate("abonos_apartado");
            clientes_lastupdate = localDM.getTableLastUpdate("clientes");
            cortes_lastupdate = localDM.getTableLastUpdate("cortes");
        }
        public async Task<bool> PingServerAsync()
        {
            Ping ping = new Ping();
            PingReply result = await ping.SendPingAsync(url);
            return result.Status == IPStatus.Success;
        }
        public async Task<bool> GetCategorias()
        {
            if (localDM.IsCatalogPreloaded)
            {
                // No descargar categorías si están precargadas
                return true;
            }

            string res = "";
            Dictionary<string, string> date = new Dictionary<string, string>();
            date["fecha_de_actualizacion"] = categorias_lastupdate;

            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/categorias/sincronizar", date);
                res = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var categorias = JsonConvert.DeserializeObject<List<Categoria>>(data["categorias"].ToString());

                        localDM.saveCategorias(categorias);
                        categorias_lastupdate = localDM.getTableLastUpdate("categorias");
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error sincronizando categorías: {e.Message}");
            }
            return false;
        }

        public async Task<bool> ForzarSincronizacionCategorias()
        {
            Console.WriteLine("=== POS - FORZAR SINCRONIZACIÓN CATEGORÍAS ===");

            string res = "";
            Dictionary<string, string> date = new Dictionary<string, string>();
            date["fecha_de_actualizacion"] = categorias_lastupdate;

            Console.WriteLine($"POS - Fecha última actualización: {categorias_lastupdate}");

            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/categorias/sincronizar", date);
                res = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"POS - Respuesta del servidor: {res}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());

                        // DESERIALIZAR COMO DYNAMIC para evitar problemas con propiedades faltantes
                        var categoriasJson = data["categorias"].ToString();
                        Console.WriteLine($"POS - JSON de categorías recibido: {categoriasJson}");

                        // Convertir JSON a objetos Categoria usando deserialización dinámica
                        var categoriasRaw = JsonConvert.DeserializeObject(categoriasJson);
                        var categorias = JsonConvert.DeserializeObject<List<Categoria>>(categoriasJson);

                        Console.WriteLine($"POS - Categorías recibidas: {categorias?.Count ?? 0}");

                        // NO intentar acceder a isdescuento/descuento aquí - 
                        // saveCategorias() se encargará de manejar las columnas correctamente

                        localDM.saveCategorias(categorias);
                        categorias_lastupdate = localDM.getTableLastUpdate("categorias");

                        Console.WriteLine("POS - Categorías sincronizadas exitosamente");
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"POS - Error forzando sincronización: {e.Message}");
                Console.WriteLine($"POS - StackTrace: {e.StackTrace}");
            }

            return false;
        }

        public async Task<bool> GetMedidas()
        {
            if (localDM.IsCatalogPreloaded)
            {
                // No descargar medidas si están precargadas
                return true;
            }

            string res = "";
            Dictionary<string, string> date = new Dictionary<string, string>();
            date["fecha_de_actualizacion"] = medidas_lastupdate;

            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/medidas/sincronizar", date);
                res = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var medidas = JsonConvert.DeserializeObject<List<Medida>>(data["medidas"].ToString());

                        localDM.saveMedidas(medidas);
                        medidas_lastupdate = localDM.getTableLastUpdate("medidas");
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error sincronizando medidas: {e.Message}");
            }
            return false;
        }
        //public async Task<bool> GetProveedores()
        //{
        //    string res = "";
        //    Dictionary<string, string> date = new Dictionary<string, string>();
        //    date["fecha_de_actualizacion"] = proveedores_lastupdate;
        //    try
        //    {
        //        HttpResponseMessage response = await client.GetAsync(url + "api/proveedores");
        //        res = await response.Content.ReadAsStringAsync();
        //        if (response.IsSuccessStatusCode)
        //        {
        //            res = await response.Content.ReadAsStringAsync();

        //            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
        //            if (result["status"].ToString().Equals("success"))
        //            {
        //                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
        //                var proveedores = JsonConvert.DeserializeObject<List<Proveedor>>(data["proovedores"].ToString());

        //                localDM.saveProveedores(proveedores);
        //                proveedores_lastupdate = localDM.getTableLastUpdate("proveedores");
        //                return true;
        //            }
        //            else
        //            {
        //                //MessageBox.Show("Error", result["data"].ToString());
        //            }

        //        }
        //        else
        //        {
        //            //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor", "Error");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
        //    }
        //    return false;
        //}
        public async Task<Dictionary<string, object>> GetExistencias(string idSucursal, string arg)
        {
            string res = "";
            try
            {
                HttpResponseMessage response = await client.GetAsync(url + "api/sucursales/"+idSucursal+"/productos/"+arg);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        return data;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }

                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return null;
        }
        public async Task<bool> GetProductos()
        {
            string res = "";
            Dictionary<string, string> date = new Dictionary<string, string>();

            // Usamos la fecha de última actualización o fecha mínima si es precargado
            date["fecha_de_actualizacion"] = productos_lastupdate;

            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/productos/sincronizar", date);
                res = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var productos = JsonConvert.DeserializeObject<List<Producto>>(data["productos"].ToString());

                        // Procesamos los productos recibidos
                        if (localDM.IsCatalogPreloaded)
                        {
                            // Para precarga, solo actualizamos los productos modificados
                            localDM.UpdateExistingProducts(productos);
                        }
                        else
                        {
                            // Para instalación nueva, guardamos todos los productos
                            localDM.saveProductos(productos);
                        }

                        // Actualizamos la fecha de última sincronización
                        productos_lastupdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Errorrrrr sincronizando productos: {e.Message}");
            }
            return false;
        }

        public async Task<bool> GetApartados()
        {
            string res = "";
            Dictionary<string, object> date = new Dictionary<string, object>();
            date["fecha_de_actualizacion"] = apartados_lastupdate;
            date["sucursal_id"] = sucursal_id;
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/apartados/sincronizar", date);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))

                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var apartados = JsonConvert.DeserializeObject<List<Apartado>>(data["apartados"].ToString());
                        localDM.saveApartados(apartados);
                        localDM.AnalizarApartadosTemporales();
                        apartados_lastupdate = localDM.getTableLastUpdate("apartados");
                        Console.WriteLine("Ultima fecha de apartados "+apartados_lastupdate);
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }

            return false;
        }
        public async Task<bool> GetAbonosApartado()
        {
            string res = "";
            Dictionary<string, object> date = new Dictionary<string, object>();
            date["fecha_de_actualizacion"] = abonos_apartado_lu;
            date["sucursal_id"] = sucursal_id;
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/abonos_apartado/sincronizar", date);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))

                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var apartados = JsonConvert.DeserializeObject<List<AbonoApartado>>(data["abonos_apartado"].ToString());
                        localDM.saveAbonosApartado(apartados);
                        localDM.AnalizarAbonosApartadoTemporales();
                        abonos_apartado_lu = localDM.getTableLastUpdate("abonos_apartado");
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }

            return false;
        }
        public async Task<bool> SendPendingCortes()
        {
            List<Dictionary<string, string>> pendingCortes = localDM.GetPendingCortes();

            foreach (var corte in pendingCortes)
            {
                try
                {
                    int id = int.Parse(corte["id"]);

                    // Actualiza el estado del corte a "Enviado" antes de enviarlo
                    localDM.UpdateCorteDetalles(id, "Enviado");

                    string apiUrl = url + "api/cortes";
                    HttpResponseMessage response = await client.PostAsJsonAsync(apiUrl, corte);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Corte ID: {id} enviado exitosamente.");
                    }
                    else
                    {
                        Console.WriteLine($"Error al enviar el corte ID: {id}, Status Code: {response.StatusCode}");

                        // Si no jalo se revierte el estado del corte a "Pendiente de envío"
                        localDM.UpdateCorteDetalles(id, "Pendiente de envío");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception al enviar el corte ID: {corte["id"]}, Message: {e.Message}");

                    // Revertir el estado del corte a "Pendiente de envío" en caso de excepción
                    int id = int.Parse(corte["id"]);
                    localDM.UpdateCorteDetalles(id, "Pendiente de envío");
                }
            }

            return true;
        }
        public async Task<bool> restarExistencia(int sucursalId, int productoId, double cantidad)
        {
            string apiUrl = $"{url}api/sucursales/{sucursalId}/productos/{productoId}";
            Console.WriteLine($"Making request to: {apiUrl}");
            var requestData = new
            {
                cantidad = cantidad
            };

            try
            {
                HttpResponseMessage response = await client.PutAsJsonAsync(apiUrl, requestData);
                string res = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response status code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Producto actualizado con éxito.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Error al actualizar el producto.");
                    Console.WriteLine(res); 
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<bool> GetCortes()
        {
            string res = "";
            Dictionary<string, object> date = new Dictionary<string, object>();
            date["fecha_de_actualizacion"] = cortes_lastupdate;
            date["sucursal_id"] = sucursal_id;

            try
            {
                string apiUrl = url + "api/cortes/sincronizar";
                //Console.WriteLine($"Making request to: {apiUrl}");

                HttpResponseMessage response = await client.PostAsJsonAsync(apiUrl, date);
                res = await response.Content.ReadAsStringAsync();

                //Console.WriteLine($"Response status code: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);

                    if (result["status"].ToString().Equals("success"))
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var cortes = JsonConvert.DeserializeObject<List<Corte>>(data["cortes"].ToString());
                        localDM.saveCortes(cortes);
                        //Console.WriteLine(data["cortes"].ToString() );

                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Hubo un problema al establecer la conexión con el servidor", "Error");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Hubo un problema al establecer la conexión con el servidor");
            }
            return false;
        }

        public async Task<bool> GetCreditos()
        {
            string res = "";
            Dictionary<string, object> date = new Dictionary<string, object>();
            date["fecha_de_actualizacion"] = creditos_lastupdate;
            date["sucursal_id"] = sucursal_id;
            try
            {

                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/creditos/sincronizar", date);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))

                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var creditos = JsonConvert.DeserializeObject<List<Credito>>(data["creditos"].ToString());
                        localDM.saveCreditos(creditos);
                        localDM.AnalizarCrerditosTemporales();
                        creditos_lastupdate = localDM.getTableLastUpdate("creditos");
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Hubo un problema al establecer la conexion con el servidor", "Error");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }

            return false;
        }
        public async Task<bool> GetAbonosCredito()
        {
            string res = "";
            Dictionary<string, object> date = new Dictionary<string, object>();
            date["fecha_de_actualizacion"] = abonos_credito_lu;
            date["sucursal_id"] = sucursal_id;
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/abonos_credito/sincronizar", date);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))

                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var acreditos = JsonConvert.DeserializeObject<List<AbonoCredito>>(data["abonos_creditos"].ToString());
                        localDM.saveAbonosCredito(acreditos);
                        localDM.AnalizarAbonosCreditoTemporales();
                        abonos_credito_lu = localDM.getTableLastUpdate("abonos_credito");
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }

            return false;
        }
        public async Task<bool> GetClientes()
        {
            string res = "";
            Dictionary<string, string> date = new Dictionary<string, string>();
            date["fecha_de_actualizacion"] = clientes_lastupdate;
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/clientes_creditos/sincronizar", date);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))

                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var clientes = JsonConvert.DeserializeObject<List<Cliente>>(data["clientes_credito"].ToString());
                        localDM.saveClientes(clientes);
                        localDM.AnalizarClientesTemporales();
                        clientes_lastupdate = localDM.getTableLastUpdate("clientes");
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }

            return false;
        }
        public async Task<Dictionary<string, string>> UpdateClienteAsync(Cliente cliente)
        {
            var result = new Dictionary<string, string>();
            try
            {
                // Serializar el objeto cliente a JSON
                var json = JsonConvert.SerializeObject(cliente);

                // Crear el contenido de la solicitud
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Enviar la solicitud PUT al servidor
                var response = await client.PutAsync($"{url}api/clientes_creditos/{cliente.id}", content);

                if (response.IsSuccessStatusCode)
                {
                    result["status"] = "success";
                    result["message"] = "Cliente modificado con éxito";
                    //MessageBox.Show("Cliente modificado con éxito", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    result["status"] = "error";
                    result["message"] = $"Error al modificar el cliente: {response.StatusCode}"; // generalmente sale cuando hay un dato duplicado
                    //MessageBox.Show($"Error al modificar el cliente: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                result["status"] = "error";
                result["message"] = $"Excepción capturada: {ex.Message}";
                //MessageBox.Show($"Excepción capturada: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return result;
        }
        public async Task<Dictionary<string, string>> DesactivarClienteAsync(int idCliente)
        {
            var result = new Dictionary<string, string>();
            try
            {
                // Crear un objeto vacío para enviar en el cuerpo (puede ser "{}" o null según el servidor)
                var json = "{}"; // O puedes usar "null" si el servidor lo acepta así

                // Crear el contenido de la solicitud
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Enviar la solicitud POST al servidor
                var response = await client.PostAsync($"{url}api/clientes_creditos/desactivar/{idCliente}", content);
                Console.WriteLine("Respuesta del servidor: " + response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    result["status"] = "success";
                    result["message"] = "Cliente desactivado correctamente";
                    Console.WriteLine("Cliente desactivado correctamente");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    result["status"] = "error";
                    result["message"] = $"Error al desactivar el cliente: {response.StatusCode}, Detalles: {responseContent}";
                    Console.WriteLine($"Error al desactivar el cliente: {response.StatusCode}, Detalles: {responseContent}");
                    MessageBox.Show($"Error al desactivar el cliente: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                result["status"] = "error";
                result["message"] = $"Excepción capturada: {ex.Message}";
                Console.WriteLine("Excepción capturada: " + ex.Message);
                MessageBox.Show($"Excepción capturada: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return result;
        }

        /*public async Task<Dictionary<string, string>> DeleteClienteAsync(int idCliente)
        {
            var result = new Dictionary<string, string>();
            try
            {
                // Correct the URL format
                var response = await client.DeleteAsync($"{url}api/clientes_creditos/{idCliente}");

                if (response.IsSuccessStatusCode)
                {
                    result["status"] = "success";
                    result["message"] = "Cliente eliminado con éxito";
                    MessageBox.Show("Cliente dado de baja con éxito", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    result["status"] = "error";
                    result["message"] = $"Error al eliminar el cliente: {response.StatusCode}";
                    MessageBox.Show($"Error al eliminar el cliente: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                result["status"] = "error";
                result["message"] = $"Excepción capturada: {ex.Message}";
                MessageBox.Show($"Excepción capturada: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return result;
        }*/


        public async Task<bool> GetUsuarios()
        {
            string res = "";
            Dictionary<string, string> date = new Dictionary<string, string>();
            date["fecha_de_actualizacion"] = usuarios_lastupdate;
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/usuarios/sincronizar", date);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var usuarios = JsonConvert.DeserializeObject<List<Usuario>>(data["usuarios"].ToString());

                        localDM.saveUsuarios(usuarios);
                        usuarios_lastupdate = localDM.getTableLastUpdate("usuarios");
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }

                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;
        }
        //public async Task<bool> GetOperaciones()
        //{
        //    string res = "";
        //    try
        //    {
        //        HttpResponseMessage response = await client.GetAsync(url + "api/operaciones");
        //        if (response.IsSuccessStatusCode)
        //        {
        //            res = await response.Content.ReadAsStringAsync();

        //            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
        //            if (result["status"].ToString().Equals("success"))
        //            {
        //                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
        //                var operaciones = JsonConvert.DeserializeObject<List<Operacion>>(data["operaciones"].ToString());

        //                localDM.saveOperaciones(operaciones);
        //                usuarios_lastupdate = localDM.getTableLastUpdate("operaciones");
        //                return true;
        //            }
        //            else
        //            {
        //                //MessageBox.Show("Error", result["data"].ToString());
        //            }

        //        }
        //        else
        //        {
        //            //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor", "Error");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
        //    }
        //    return false;
        //}
        public async Task<bool> GetSucursales()
        {
            string res = "";
            Dictionary<string, string> date = new Dictionary<string, string>();
            date["fecha_de_actualizacion"] = sucursales_lastupdate;
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/sucursales/sincronizar", date);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var sucursales = JsonConvert.DeserializeObject<List<Sucursal>>(data["sucursales"].ToString());

                        localDM.saveSucursales(sucursales);
                        sucursales_lastupdate = localDM.getTableLastUpdate("sucursales");
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }

                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;
        }

        public async Task<Dictionary<string, string>> SendProductosAsync(List<NuevoProducto> productos)
        {
            Dictionary<string, string> methodResponse = new Dictionary<string, string>();
            string res = "";
            try
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                data["usuario_id"] = activeUser.id.ToString();
                data["nuevos_productos"] = productos.ToArray();
                HttpResponseMessage response = await client.PostAsJsonAsync(
                url + "api/productos", data);
                //if (response.IsSuccessStatusCode)
                //{
                    res = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        methodResponse["message"] ="Los productos se han enviado con exito";
                        methodResponse["status"] = "success";
                        await GetProductos();
                        refreshData(3);
                    }
                    else
                    {
                        methodResponse["status"] = "error";
                    if(result.ContainsKey("data"))
                        methodResponse["message"] = result["data"].ToString();
                    else
                        methodResponse["message"] = res;
                }
                //}
                //else
                //{
                //    MessageBox.Show("No se recibió success", "Error");
                //}
            }
            catch (Exception e)
            {
                methodResponse["status"] = "error2";
                methodResponse["message"] = "Hubo un problema al establecer la conexion al servidor, los productos se han almacenado localmente\nDetalles:\n"+e.Message;
            }
            return methodResponse;

        }
        public async Task<Dictionary<string, string>> SendapartadoAsync(Apartado apartado)
        {
            Dictionary<string, string> methodResponse = new Dictionary<string, string>();
            string res = "";
            bool avance = true;
            if ( apartado.temporal == 1)
            {
                NuevoCliente cliente = localDM.getClienteeTemporal(apartado.cliente_creditos_id);
                Dictionary <string, string> r = await SendClienteAsync(cliente);
                if (r["status"].Equals("success"))
                {
                    apartado.cliente_creditos_id = int.Parse(r["id"]);
                }
                else
                {
                    avance = false;
                    methodResponse["status"] = "Error al enviar el cliente";
                    methodResponse["message"] = r["message"];
                }
            }
            if(avance)
            {
                try
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync(
                    url + "api/apartados", apartado);
                    //if (response.IsSuccessStatusCode)
                    //{
                    res = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        methodResponse["message"] = "El registro de apartado se ha enviado con exito";
                        methodResponse["status"] = "success";
                        await GetApartados();
                        await GetAbonosApartado();
                        //refreshData(3);
                    }
                    else
                    {   
                        
                        methodResponse["status"] = "error GRANDE";
                        if (result.ContainsKey("data"))
                            methodResponse["message"] = result["data"].ToString();
                        else
                            methodResponse["message"] = res;
                    }
                }
                catch (Exception e)
                {
                    methodResponse["status"] = "error2";
                    methodResponse["message"] = "Hubo un problema al establecer la conexion al servidor\nDetalles:\n" + e.Message;
                }
            }
            else
            {
                methodResponse["status"] = "error3";
                methodResponse["message"] = "Hubo un problema al establecer la conexion al servidor, el registro de cliente y los productos se han almacenado localmente";
            }


            return methodResponse;

        }
        public async Task<Dictionary<string, string>> SendcreditoAsync(Credito credito)
        {
            Dictionary<string, string> methodResponse = new Dictionary<string, string>();
            string res = "";
            bool avance = true;
            if (credito.temporal == 1)
            {
                NuevoCliente cliente = localDM.getClienteeTemporal(credito.cliente_creditos_id);
                Dictionary<string, string> r = await SendClienteAsync(cliente);
                if (r["status"].Equals("success"))
                {
                    credito.cliente_creditos_id = int.Parse(r["id"]);
                }
                else
                {
                    avance = false;
                }
            }
            if (avance)
            {
                try
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync(
                    url + "api/creditos", credito);
                    //if (response.IsSuccessStatusCode)
                    //{
                    res = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        methodResponse["message"] = "El registro de credito se ha enviado con exito";
                        methodResponse["status"] = "success";
                        await GetCreditos();
                        await GetAbonosCredito();
                        //refreshData(3);
                    }
                    else
                    {
                        methodResponse["status"] = "error";
                        if (result.ContainsKey("data"))
                            methodResponse["message"] = result["data"].ToString();
                        else
                            methodResponse["message"] = res;
                    }
                //}
                //else
                //{
                //    MessageBox.Show("No se recibió success", "Error");
                //}
                }
                catch (Exception e)
                {
                    methodResponse["status"] = "error2";
                    methodResponse["message"] = "Hubo un problema al establecer la conexion al servidor, los productos se han almacenado localmente \nDetalles: \n" + e.Message;
                }
            }
            else
            {
                methodResponse["status"] = "error3";
                methodResponse["message"] = "Hubo un problema al establecer la conexion al servidor, el registro de cliente y los productos se han almacenado localmente";
            }

            return methodResponse;

        }
        
        public async Task<bool> enviarAbonoCredito(AbonoCredito abono, int idCredito)
        {
            string res = "";
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/creditos/" + idCredito + "/abonar", abono);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        await GetCreditos();
                        await GetAbonosCredito();
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor, favor de intentar mas tarde", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }

            return false;
        }
        public async Task<bool> enviarAbonoApartado(AbonoApartado abono, int idApartado)
        {
            string res = "";
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/apartados/" + idApartado + "/abonar", abono);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        await GetApartados();
                        await GetAbonosApartado();
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor, favor de intentar mas tarde", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }

            return false;
        }
        public async Task<Dictionary<string, string>> SendClienteAsync(NuevoCliente cliente)
        {
            Dictionary<string, string> methodResponse = new Dictionary<string, string>();
            string res = "";
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(
                url + "api/clientes_creditos", cliente);
                //if (response.IsSuccessStatusCode)
                //{
                res = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                if (result["status"].ToString().Equals("success"))
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, int>>(result["data"].ToString());
                    int id = data["cliente_creditos_id"];
                    methodResponse["message"] = "Cliente creado con la ID: "+ id;
                    methodResponse["status"] = "success";
                    methodResponse["id"] = id.ToString();
                    localDM.reconectarApartados(cliente.id_temporal, id);
                    localDM.reconectarCreditos(cliente.id_temporal, id);
                    await GetClientes();
                    //refreshData(3);
                }
                else
                {
                    methodResponse["status"] = "error";
                    if (result.ContainsKey("data"))
                        methodResponse["message"] = result["data"].ToString();
                    else
                        methodResponse["message"] = res;
                }
                //}
                //else
                //{
                //    MessageBox.Show("No se recibió success", "Error");
                //}
            }
            catch (Exception e)
            {
                methodResponse["status"] = "error2";
                methodResponse["message"] = "Hubo un problema al establecer la conexion al servidor\nDetalles:\n" + e.Message;
            }
            return methodResponse;

        }
        
        public async Task<bool> SendMedidaAsync(string nombreMedida)
        {
            string res = "";
            try
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data["usuario_id"] = activeUser.id.ToString();
                data["nombre"] = nombreMedida;
                HttpResponseMessage response = await client.PostAsJsonAsync(
                    url + "api/medidas", data);
                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Medida creada", "Completado");
                        await GetMedidas();
                        refreshData(1);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("No se recibió success", "Error");
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;
        }
        public async Task<bool> SendCategoriaAsync(string categoria)
        {
            string res = "";
            try
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data["usuario_id"] = activeUser.id.ToString();
                data["nombre"] = categoria;
                HttpResponseMessage response = await client.PostAsJsonAsync(
                    url + "api/categorias", data);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Categoria creada", "Completado");
                        await GetCategorias();
                        refreshData(2);
                        return true;
                    }
                    else
                    {
                    MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("No se recibió success", "Error");
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;
        }
        public async Task<bool> SendUsuarioAsync(Dictionary<string, string> usuario)
        {
            string res = "";
            try
            {                
                usuario["usuario_id"]= activeUser.id.ToString();
                HttpResponseMessage response = await client.PostAsJsonAsync(
                    url + "api/usuarios", usuario);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Usuario creado", "Completado");
                        await GetUsuarios();
                        refreshData(7);
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor, favor de intentar mas tarde", "Error");
                }

            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;
        }
        public async Task<bool> SendSucursalAsync(Dictionary<string, string> sucursal)
        {
            string res = "";
            try
            {
                sucursal["usuario_id"] = activeUser.id.ToString();
                HttpResponseMessage response = await client.PostAsJsonAsync(
                    url + "api/sucursales", sucursal);
                res = await response.Content.ReadAsStringAsync();
                //if (response.IsSuccessStatusCode)
                //{
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Sucursal creada", "Completado");
                        await GetSucursales();
                        refreshData(4);
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show(result["data"].ToString(),"Error");
                    }
                //}
                //else
                //{
                //    MessageBox.Show("Hubo un problema al establecer la conexion con el servidor, favor de intentar mas tarde", "Error");
                //}

            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;
        }
        public async Task<(bool Success, string Message)> SendCorte(Dictionary<string, string> data)
        {
            try
            {

                HttpResponseMessage response = await client.PostAsJsonAsync($"{url}api/cortes", data);
                string res = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (IsValidJson(res))
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                        if (result.TryGetValue("status", out var status) && status.ToString().Equals("success"))
                        {
                            return (true, string.Empty);
                        }
                        else if (result.TryGetValue("data", out var dataMessage))
                        {
                            return (false, dataMessage.ToString());
                        }
                    }
                    return (false, "Respuesta del servidor no válida.");
                }
                else
                {
                    return (false, res);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Hubo un problema al establecer la conexión con el servidor: {ex.Message}");
            }
        }

        private bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) return false;
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || // For object
                (strInput.StartsWith("[") && strInput.EndsWith("]")))   // For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException) // Invalid JSON
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> SendVentaAsync(Dictionary<string, string> venta, bool hasTemporal, int id)
        {
            List<ProductoVenta> productos = localDM.getCarrito(id);

            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach (var x in venta)
            {
                data[x.Key] = x.Value;
            }
            string res = "";
            bool success = true;

            if (hasTemporal)
            {
                if (await enviarAltaTemporal())
                {
                    productos = localDM.getCarrito(id);
                }
                else
                {
                    success = false;
                }
            }

            if (success)
            {
                data["productos"] = productos.Select(p => new
                {
                    id = p.id,
                    cantidad = p.cantidad,
                    precio_venta = p.precio_venta
                }).ToList();

                try
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync(url + "api/ventas", data);
                    res = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                        if (result["status"].ToString().Equals("success"))
                        {
                            refreshData(5);
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"Error: {result["data"].ToString()}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error: {res}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception: {e.Message}");
                }
            }

            return false;
        }


        public async Task<bool> ModifyProductoAsync(string id, Dictionary<string, string> producto)
        {
            string res = "";
            try
            {
                producto["usuario_id"] = activeUser.id.ToString();
                HttpResponseMessage response = await client.PutAsJsonAsync(
                    url + "api/productos/" + id, producto);
                response.EnsureSuccessStatusCode();
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Producto modificado", "Completado");
                        await GetProductos();
                        refreshData(3);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Hubo un problema al establecer la conexion con el servidor, favor de intentar mas tarde", "Error");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;

        }
        public async Task<bool> ModifyCategoriaAsync(string id, Dictionary<string, string> categoria)
        {
            string res = "";
            try
            {
                categoria["usuario_id"]= activeUser.id.ToString();
                HttpResponseMessage response = await client.PutAsJsonAsync(
                    url + "api/categorias/" + id, categoria);
                response.EnsureSuccessStatusCode();
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Categoría modificada", "Completado");
                        await GetCategorias();
                        refreshData(2);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("No se recibió success", "Error");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;

        }
        public async Task<bool> ModifyMedidaAsync(string id, Dictionary<string, string> medida)
        {
            string res = "";
            try
            {
                medida["usuario_id"] = activeUser.id.ToString();
                HttpResponseMessage response = await client.PutAsJsonAsync(
                url + "api/medidas/" + id, medida);
                response.EnsureSuccessStatusCode();
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Medida modificada", "Completado");
                        await GetMedidas();
                        refreshData(1);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("No se recibió success", "Error");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;

        }
        public async Task<bool> ModifyUsuarioAsync(string id, Dictionary<string, string> usuario)
        {
            string res = "";
            try
            {
                usuario["usuario_id"] = activeUser.id.ToString();
                HttpResponseMessage response = await client.PutAsJsonAsync(
                    url + "api/usuarios/" + id, usuario);
                response.EnsureSuccessStatusCode();
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Registro de usuario modificado", "Completado");
                        await GetUsuarios();
                        refreshData(7);
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor, favor de intentar mas tarde", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;

        }
        //public async Task<bool> ModifyProveedorAsync(string id, Dictionary<string, string> proveedor)
        //{
        //    string res = "";
        //    try
        //    {
        //        proveedor["usuarior_id"] = activeUser.id.ToString();
        //        HttpResponseMessage response = await client.PutAsJsonAsync(
        //            url + "api/proveedores/" + id, proveedor);
        //        response.EnsureSuccessStatusCode();
        //        res = await response.Content.ReadAsStringAsync();
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
        //            if (result["status"].ToString().Equals("success"))
        //            {
        //                MessageBox.Show("Registro de proveedor modificado", "Completado");
        //                await GetProveedores();
        //                refreshData(8);
        //                return true;
        //            }
        //            else
        //            {
        //                //MessageBox.Show("Error", result["data"].ToString());
        //            }
        //        }
        //        else
        //        {
        //            //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor, favor de intentar mas tarde", "Error");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
        //    }
        //    return false;

        //}
        public async Task<bool> ModifySucursalAsync(string id, Dictionary<string, string> sucursal)
        {
            string res = "";
            try
            {
                sucursal["usuario_id"] = activeUser.id.ToString();
                HttpResponseMessage response = await client.PutAsJsonAsync(
                    url + "api/sucursales/" + id, sucursal);
                response.EnsureSuccessStatusCode();
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Registro de sucursal modificado", "Completado");
                        await GetSucursales();
                        refreshData(4);
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor, favor de intentar mas tarde", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;

        }
        public async Task<List<Usuario>> GetUsuariosSucursal(string id)
        {
            string res = "";
            try
            {
                HttpResponseMessage response = await client.GetAsync(url + "api/sucursales/"+id+"/usuarios/");
                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var users = JsonConvert.DeserializeObject<List<Usuario>>(data["sucursales"].ToString());
                       
                        
                        return users;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }

                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return null;
        }
        public async Task<bool> PostUsuarioSucursal(string idSucursal, string idUsuario)
        {
            string res = "";
            try
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data["usuario_id"] = activeUser.id.ToString();
                HttpResponseMessage response = await client.PostAsJsonAsync(
                    url + "api/sucursales/" + idSucursal+"/usuarios/"+idUsuario, data);
                response.EnsureSuccessStatusCode();
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("No se recibió success", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;

        }
        public async Task<bool> RemoveUsuarioSucursal(string idSucursal, string idUsuario)
        {
            string res = "";
            try
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data["_method"] = "delete";
                HttpResponseMessage response = await client.PostAsJsonAsync(
                    url + "api/sucursales/" + idSucursal + "/usuarios/" + idUsuario, data);
                response.EnsureSuccessStatusCode();
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("No se recibió success", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;

        }
        public async Task<bool> DisableProductoAsync(string id)
        {
            string res = "";
            try
            {
                Dictionary<string, string> user_id = new Dictionary<string, string>();
                user_id["usuario_id"] = activeUser.id.ToString();
                HttpResponseMessage response = await client.PostAsJsonAsync(
                url + "api/productos/desactivar/" + id, user_id);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Producto eliminado", "Completado");
                        await GetProductos();
                        refreshData(3);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("No se recibió success", "Error");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;

        }
        public async Task<bool> DisableCategoriaAsync(string id)
        {
            string res = "";
            try
            {
                Dictionary<string, string> user_id = new Dictionary<string, string>();
                user_id["usuario_id"] = activeUser.id.ToString();
                HttpResponseMessage response = await client.PostAsJsonAsync(
                url + "api/categorias/desactivar/" + id, user_id);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Categoria eliminada", "Completado");
                        await GetCategorias();
                        refreshData(2);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("No se recibió success", "Error");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;
        }
        public async Task<bool> DisableMedidaAsync(string id)
        {
            string res = "";
            try
            {
                Dictionary<string, string> user_id = new Dictionary<string, string>();
                user_id["usuario_id"] = activeUser.id.ToString();
                HttpResponseMessage response = await client.PostAsJsonAsync(
                url + "api/medidas/desactivar/" + id, user_id);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Medida eliminada", "Completado");
                        await GetMedidas();
                        refreshData(1);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("No se recibió success", "Error");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;
        }
        public async Task<bool> DisableUsuarioAsync(string id)
        {
            string res = "";
            try
            {
                Dictionary<string, string> user_id = new Dictionary<string, string>();
                user_id["usuario_id"] = activeUser.id.ToString();
                HttpResponseMessage response = await client.PostAsJsonAsync(
                url + "api/usuarios/desactivar/" + id, user_id);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Usuario eliminado", "Completado");
                        await GetUsuarios();
                        refreshData(7);
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor, favor de intentar mas tarde", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;
        }
        
        //public async Task<bool> DisableProveedorAsync(string id)
        //{
        //    string res = "";
        //    try
        //    {
        //        Dictionary<string, string> user_id = new Dictionary<string, string>();
        //        user_id["usuario_id"] = activeUser.id.ToString();
        //        HttpResponseMessage response = await client.PostAsJsonAsync(
        //        url + "api/proveedores/desactivar/" + id, user_id);
        //        res = await response.Content.ReadAsStringAsync();
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
        //            if (result["status"].ToString().Equals("success"))
        //            {
        //                MessageBox.Show("Proveedor eliminado", "Completado");
        //                await GetProveedores();
        //                refreshData(8);
        //                return true;
        //            }
        //            else
        //            {
        //                //MessageBox.Show("Error", result["data"].ToString());
        //            }
        //        }
        //        else
        //        {
        //            //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor, favor de intentar mas tarde", "Error");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
        //    }
        //    return false;
        //}
        public async Task<bool> DisableSucursalAsync(string id)
        {
            string res = "";
            try
            {
                Dictionary<string, string> user_id = new Dictionary<string, string>();
                user_id["usuario_id"] = activeUser.id.ToString();
                HttpResponseMessage response = await client.PostAsJsonAsync(
                url + "api/sucursales/desactivar/" + id, user_id);
                res = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        MessageBox.Show("Sucursal eliminada", "Completado");
                        await GetSucursales();
                        refreshData(4);
                        return true;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }
                }
                else
                {
                    //MessageBox.Show("No se recibió success", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return false;
        }
        public async Task<bool> enviarApartadosTemporal()
        {
            if(await enviarClientesTemporal())
            {
                List<Apartado> apartados = localDM.GetApartadosTemporales();
                foreach (Apartado c in apartados)
                {
                    Dictionary<string, string> result = await SendapartadoAsync(c);
                    if (!result["status"].Equals("success"))
                    {
                        return false;
                    }
                }
                return true;
            }            
            return false;
        }
        public async Task<bool> enviarCreditosTemporal()
        {
            if (await enviarClientesTemporal())
            {
                List<Credito> creditos = localDM.GetCreditosTemporales();
                foreach (Credito c in creditos)
                {
                    Dictionary<string, string> result = await SendcreditoAsync(c);
                    if (!result["status"].Equals("success"))
                    {
                        return false;
                    }
                }
                return true; 
            }
            return false;
        }
        public async Task<bool> enviarCreditoTemporal(string folio)
        {
            Credito credito = localDM.GetCreditoTemporal(folio);
            if (credito != null)
            {
                Dictionary<string, string> result = await SendcreditoAsync(credito);
                if (!result["status"].Equals("success"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            
        return false;
        }
        public async Task<bool> enviarApartadoTemporal(string folio)
        {
            Apartado credito = localDM.GetApartadoTemporal(folio);
            if (credito != null)
            {
                Dictionary<string, string> result = await SendapartadoAsync(credito);
                if (!result["status"].Equals("success"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> enviarAltaTemporal()
        {
            List<NuevoProducto> productos = localDM.GetProductosTemporales();
            Dictionary<string, string> result = await SendProductosAsync(productos);
            if (result["status"].Equals("success"))
            {
                localDM.reconnectProductosVenta(productos);
                localDM.reconnectProductosEntrada(productos);
                localDM.clearAltaTemporal();
                return true;
            }
            return false;
        }
        public async Task<bool> enviarClientesTemporal()
        {
            List<NuevoCliente> clientes = localDM.GetClientesTemporales();
            foreach(NuevoCliente c in clientes)
            {
                Dictionary<string, string> result = await SendClienteAsync(c);
                if (!result["status"].Equals("success"))
                {
                    return false;
                }
            }
            return true;       
        }
        
        //public async Task<bool> SendEntrada(int id, bool hasTemporal, Dictionary<string, object> entrada, List<ProductoEntrada> productos)
        //{
        //    string res;
        //    bool success = true;
        //    if (hasTemporal)
        //    {
        //        if (await enviarAltaTemporal())
        //        {
        //            productos = localDM.getListaEntrada(id);
        //        }
        //        else
        //        {
        //            success = false;
        //        }

        //    }
        //    if (success)
        //    {
        //        entrada["entrada_productos"] = productos;
        //        try
        //        {

        //            HttpResponseMessage response = await client.PostAsJsonAsync(
        //                url + "api/entradas", entrada);
        //            res = await response.Content.ReadAsStringAsync();
        //            if (response.IsSuccessStatusCode)
        //            {
        //                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
        //                if (result["status"].ToString().Equals("success"))
        //                {
        //                    MessageBox.Show("Entrada enviada", "Completado");
        //                    return true;
        //                }
        //                else
        //                {
        //                    //MessageBox.Show("Error", result["data"].ToString());
        //                }
        //            }
        //            else
        //            {
        //                //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor, favor de intentar mas tarde", "Error");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
        //        }
        //    }

        //    return false;
        //}
        public async Task<List<ProductoExistencia>> getProductoExistencia (string idproducto)
        {
            string res;
            try
            {
                HttpResponseMessage response = await client.GetAsync(url + "api/productos/" + idproducto + "/existencias/");
                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(res);
                    if (result["status"].ToString().Equals("success"))
                    {
                        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result["data"].ToString());
                        var prods = JsonConvert.DeserializeObject<List<ProductoExistencia>>(data["existencias"].ToString());


                        return prods;
                    }
                    else
                    {
                        //MessageBox.Show("Error", result["data"].ToString());
                    }

                }
                else
                {
                    //MessageBox.Show("Hubo un problema al establecer la conexion con el servidor", "Error");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message, "Hubo un problema al establecer la conexion con el servidor");
            }
            return null;
        }
    }
}
