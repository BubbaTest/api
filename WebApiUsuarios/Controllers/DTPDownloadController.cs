using MaxMind.Db;
using MaxMind.GeoIP2.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
//using System.Web.Mvc;

namespace Nicarao.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class FTPDonwloadController : ControllerBase
    {
        [HttpGet]
        public IActionResult DownloadFTPDirectory(string Dep, string Mun)
        {
            // Directorio temporal donde se almacenarán los archivos descargados
            string tempDirectory = Path.Combine(Path.GetTempPath(), "FTPDownload");

            try
            {
                // Descargar archivos desde el servidor FTP
                bool resultado = DownloadFilesFromFTP(Dep, Mun, tempDirectory);

                if (!resultado)
                {
                    return StatusCode(500, "Error al descargar archivos desde el servidor FTP");
                }

                // Leer los archivos descargados en el directorio temporal
                string[] fileNames = Directory.GetFiles(tempDirectory);

                // Devolver el primer archivo como una respuesta HTTP al cliente
                foreach (string filePath in fileNames)
                {
                    // Leer el contenido del archivo
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    string fileName = Path.GetFileName(filePath);

                    // Devolver el archivo como una respuesta HTTP al cliente
                    return File(fileBytes, "application/octet-stream", fileName);
                }

                return StatusCode(500, "No se encontraron archivos para descargar");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error en el servidor: {ex.Message}");
            }
            finally
            {
                // Eliminar directorio temporal después de enviar la respuesta
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }

        private bool DownloadFilesFromFTP(string Dep, string Mun, string localDirectoryPath)
        {
            try
            {
                // Crear directorio temporal si no existe
                Directory.CreateDirectory(localDirectoryPath);

                // Crear solicitud FTP para listar los archivos en el directorio remoto
                //if (Origen=="mapa") { ftpServerUrl = "ftp://192.168.202.226/0000/" + Dep + "/" + Mun + "/"; }
                //else { ftpServerUrl = "ftp://192.168.202.226/0000/Adb/"; }
                var ftpServerUrl = "ftp://192.168.202.226/0000/" + Dep + "/" + Mun + "/";
                FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(ftpServerUrl);
                listRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                listRequest.Credentials = new NetworkCredential("CEPOVNI", "cePV.2024,$%");
                listRequest.UsePassive = true;
                listRequest.KeepAlive = false;

                // Obtener respuesta del servidor FTP con la lista de archivos
                using (FtpWebResponse listResponse = (FtpWebResponse)listRequest.GetResponse())
                using (StreamReader listReader = new StreamReader(listResponse.GetResponseStream()))
                {
                    // Leer nombres de archivos de la respuesta del servidor
                    string line;
                    while ((line = listReader.ReadLine()) != null)
                    {
                        // Crear ruta local para descargar el archivo
                        string localFilePath = Path.Combine(localDirectoryPath, line);

                        // Crear solicitud FTP para descargar el archivo
                        string fileUrl = ftpServerUrl + line;
                        FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(fileUrl);
                        downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                        downloadRequest.Credentials = new NetworkCredential("CEPOVNI", "cePV.2024,$%");

                        // Obtener respuesta del servidor FTP para la descarga del archivo
                        using (FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
                        using (Stream responseStream = downloadResponse.GetResponseStream())
                        using (FileStream fileStream = System.IO.File.Create(localFilePath))
                        {
                            // Leer datos del servidor FTP y escribir en el archivo local
                            byte[] buffer = new byte[1024];
                            int bytesRead;
                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fileStream.Write(buffer, 0, bytesRead);
                            }
                            //downloadResponse.Close();
                            //responseStream.Close();
                        }
                    }
                }                

                return true;
            }
            catch (WebException ex)
            {
                // Manejar error de descarga desde el servidor FTP
                string status = ((FtpWebResponse)ex.Response).StatusDescription;
                throw new Exception($"Error al descargar archivos desde el servidor FTP: {status}");
            }
        }
    }
//    public class FTPDonwloadController : ControllerBase
//    {
//        [HttpGet]
//        public IActionResult DownloadFTPDirectory(string Dep, string Mun, string Origen)
//        {
//            string tempDirectory = "";
//            // Directorio temporal donde se almacenarán los archivos descargados
//            //esta linea la comentarie para manejar las dos rutas de crreacion en temporales q los crea a ver si asi quedaban en el cliente
//            //string tempDirectory1 = Path.Combine(Path.GetTempPath(), "FTPDownload");
//            if (Origen =="mapa") { tempDirectory =@"C:\\MMPK\\"; }
//            else { tempDirectory = @"C:\\Adb\\"; }

//            try
//            {
//                // Descargar archivos desde el servidor FTP tempDirectory
//                bool resultado = DownloadFilesFromFTP(Dep, Mun, Origen, tempDirectory);

//                if (!resultado)
//                {
//                    return StatusCode(500, "Error al descargar archivos desde el servidor FTP");
//                }

//                // Leer los archivos descargados en el directorio temporal
//                string[] fileNames = Directory.GetFiles(tempDirectory);

//                // Devolver el primer archivo como una respuesta HTTP al cliente
//                foreach (string filePath in fileNames)
//                {
//                    // Leer el contenido del archivo
//                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
//                    string fileName = Path.GetFileName(filePath);

//                    // Devolver el archivo como una respuesta HTTP al cliente
//                    return File(fileBytes, "application/octet-stream", fileName);
//                }

//                return StatusCode(500, "No se encontraron archivos para descargar");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Error en el servidor: {ex.Message}");
//            }

//            //esta linea la comentarie para que no elimnira la carpta temproal en este caso mis carpetas rutas de archvios
//            //finally
//            //{
//            //    // Eliminar directorio temporal después de enviar la respuesta
//            //    //if (Directory.Exists(tempDirectory))
//            //    //{
//            //    //    Directory.Delete(tempDirectory, false);
//            //    //}
//            //}
//        }

//        private bool DownloadFilesFromFTP(string Dep, string Mun, string Origen, string localDirectoryPath)
//        {
//            try
//            {
//                string ftpServerUrl = "";
//                // Crear directorio temporal si no existe
//                Directory.CreateDirectory(localDirectoryPath);

//                //ahora el api apunta al nombre para que se resuelva el mismo cuando esta dento y fuera de l ainstitucion siendo un unico api
//                // Crear solicitud FTP para listar los archivos en el directorio remoto  190.53.36.60
//                if (Origen =="mapa") { ftpServerUrl = "ftp://TAMARINDO/0000/" + Dep + "/" + Mun + "/"; }
//                else { ftpServerUrl = "ftp://TAMARINDO/0000/Adb/"; }


//                FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(ftpServerUrl);
//                listRequest.Method = WebRequestMethods.Ftp.ListDirectory;
//                listRequest.Credentials = new NetworkCredential("CEPOVNI", "cePV.2024,$%");
//                listRequest.UsePassive = true;
//                listRequest.KeepAlive = false;

//                // Obtener respuesta del servidor FTP con la lista de archivos
//                using (FtpWebResponse listResponse = (FtpWebResponse)listRequest.GetResponse())
//                using (StreamReader listReader = new StreamReader(listResponse.GetResponseStream()))
//                {
//                    // Leer nombres de archivos de la respuesta del servidor
//                    string line;
//                    while ((line = listReader.ReadLine()) != null)
//                    {
//                        // Crear ruta local para descargar el archivo
//                        string localFilePath = Path.Combine(localDirectoryPath, line);

//                        // Crear solicitud FTP para descargar el archivo
//                        string fileUrl = ftpServerUrl + line;
//                        FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(fileUrl);
//                        downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
//                        downloadRequest.Credentials = new NetworkCredential("CEPOVNI", "cePV.2024,$%");

//                        // Obtener respuesta del servidor FTP para la descarga del archivo
//                        using (FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
//                        using (Stream responseStream = downloadResponse.GetResponseStream())
//                        using (FileStream fileStream = System.IO.File.Create(localFilePath))
//                        {
//                            // Leer datos del servidor FTP y escribir en el archivo local
//                            byte[] buffer = new byte[1024];
//                            int bytesRead;
//                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
//                            {
//                                fileStream.Write(buffer, 0, bytesRead);
//                            }
//                        }
//                    }
//                }

//                return true;
//            }
//            catch (WebException ex)
//            {
//                // Manejar error de descarga desde el servidor FTP
//                string status = ((FtpWebResponse)ex.Response).StatusDescription;
//                throw new Exception($"Error al descargar archivos desde el servidor FTP: {status}");
//            }
//        }
//    }
}