using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Configuration;

namespace syncBD
{
    class syncronizer
    {
        #region variables
        /// <summary>
        /// Mensaje que se imprimira
        /// </summary>
        private string message = "Mensaje:Esto es una prueba";

        /// <summary>
        /// Fecha y hora donde inicia la transferencia de archivos
        /// </summary>
        private string fechaFTPinicio = "FTPStarted at: ";

        /// <summary>
        /// Sucursal donde se realiza la sincronizacion
        /// </summary>
        private string sucursal = ConfigurationManager.AppSettings["store"];

        /// <summary>
        /// Sucursal donde se realiza la sincronizacion
        /// </summary>
        private string plaza = ConfigurationManager.AppSettings["sector"];

        /// <summary>
        /// Fecha donde termina la transferencia de arvhivos
        /// </summary>
        private string fechaFTPFin = "FTPEnded at: ";

        private bool flagFolder = false;
        private string horainicioFTP = "";
        private string horaFinFTP = "";
        private string horainicioLog = "";
        private string horafinLog = "";

        private static string origin = Directory.GetCurrentDirectory();
        private static string destiny = ConfigurationManager.AppSettings["destiny"];
        private string logDestiny = ConfigurationManager.AppSettings["destiny"];
        private string FTPorigin = Directory.GetCurrentDirectory();

        
        private string[] datalog = new string[5];
        private string[] ftpstartupline = new string[1] { "ftp -i -s:upload.bat" };
        private string[] ftpuploadLines = new string[7] 
        {
            "open "+ConfigurationManager.AppSettings["ftIp"],
            ConfigurationManager.AppSettings["ftpusr"],
            ConfigurationManager.AppSettings["ftppwd"],
            "cd myFolder",
            "binary",
            "put \"C:\\path\"",
            "bye"
        };
        private string[] arguments = new string[2] 
        {
            //Argumentos para llamar a robocopy
            string.Format("/C Robocopy /S {0} {1}", origin, destiny),
            //Argumentos para llamar batch de ftp
            string.Format("ftp -i -s:upload.bat")
        };

        #endregion

        #region metodos
        /// <summary>
        /// Metodo para crear archivos
        /// </summary>
        /// <param name="path">Path donde sera creado el archivo</param>
        /// <param name="name">Nombre y extencion del archivo</param>
        /// <param name="Lines">Arreglo de strings que son las lineas a insertar en el archivo</param>
        private void createFile(string path,string name,string[] Lines)
        {
            try
            {
                Console.WriteLine("Creating {0} File", name);
                // Example #3: Write only some strings in an array to a file.
                // The using statement automatically flushes AND CLOSES the stream and calls 
                // IDisposable.Dispose on the stream object.
                // NOTE: do not use FileStream for text files because it writes bytes, but StreamWriter
                // encodes the output as text.
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(path + "\\" + name))
                {
                    foreach (string line in Lines)
                    {
                        file.WriteLine(line);
                    }
                }
                
                Console.WriteLine(name+" Creation Status...[ok]");
            }
            catch (Exception)
            {

                Console.WriteLine(name + " Creation Status...[fail]");
            }

        }

        /// <summary>
        /// Metodo para mandar llamar la herramienta para transferencia de archivos
        /// </summary>
        private void callTransferTool(string arguments)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.Arguments = arguments;
                p.StartInfo.FileName = "CMD.EXE";
                p.StartInfo.CreateNoWindow = false;
                p.StartInfo.UseShellExecute = true;
                p.Start();
                p.WaitForExit();
                message = "Procees Ended [Yes]";
                Console.Write("Process Ended ...[Yes]\n");

            }
            catch (Exception)
            {
                message = "Procees Ended [No]";
                Console.Write("Process status ...[fail]");
            }
        }

        /// <summary>
        /// Metodo para obtener la hora de una fecha en especifivo
        /// </summary>
        /// <param name="fechaPorCambiar">Fecha de donde se extraera la hora</param>
        /// <returns></returns>
        private string getDateOrTime(DateTime fechaPorCambiar, string type)
        {
            string fechaDevuelta = "";
            try
            {
                if (type.Equals("hora"))
                {
                    return fechaPorCambiar.ToString("hh:mm");
                }
                else if (type.Equals("fecha"))
                {
                    fechaDevuelta = fechaPorCambiar.ToString("yyyy/MM/dd").Replace('/', '-');
                    return fechaDevuelta;
                }
                else
                {
                    return "Error gathering date or time...[fail]";
                }

            }
            catch (Exception e)
            {
                return e.Message;
            }

        }

        /// <summary>
        /// Metodo para llenar la informacion que ira en el archivo log.txt
        /// </summary>
        /// <returns></returns>
        public void fillDatalog()
        {
            fechaFTPFin = fechaFTPFin + getDateOrTime(DateTime.Now, "fecha").ToString() + " " + horaFinFTP + getDateOrTime(DateTime.Now, "hora").ToString();
            datalog[0] = message;
            datalog[1] = sucursal;
            datalog[2] = fechaFTPinicio;
            datalog[3] = fechaFTPFin;

        } 

        /// <summary>
        /// Metodo para verificar si existe la carpeta de la fecha actual y saber si esta hecho el corte.
        /// </summary>
        /// <returns></returns>
        public bool checkFolder()
        {
            
            var CurrentPath = Path.GetFullPath(origin);
            var currentDirectoriesinPath = Directory.GetDirectories(CurrentPath);
            string[] foldersWindows = new string[CurrentPath.Length];
            string dirFechaFinded = "";
            try
            {
                for (int i = 0; i < currentDirectoriesinPath.Length; i++)
                {
                    foldersWindows[i] = currentDirectoriesinPath[i].ToString();
                }

                string fechaHoy = getDateOrTime(DateTime.Now, "fecha");
                
                foreach (var item in foldersWindows)
                {
                    if (item.ToString().Equals(origin + "\\" + fechaHoy))
                    {
                        dirFechaFinded = item.ToString();
                        origin = origin + "\\" + fechaHoy;
                        destiny = destiny + "\\" + plaza + "\\" + sucursal + "\\" + fechaHoy;
                        logDestiny = logDestiny + "\\" + plaza + "\\" + sucursal + "\\" + fechaHoy;
                        flagFolder = true;
                        break;
                    }
                    else
                    {
                        flagFolder = false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            
     
            if (flagFolder==true)
            {
                return flagFolder;
            }
            else
            {
                return flagFolder;
            }
        }

        /// <summary>
        /// Metodo para intentar 5 veces el metodo checkfolder
        /// </summary>
        public void folderAttempts()
        {
            Console.Write("Checking for dayend 1st..");
            for (int i = 0; i <= 4; i++)
            {
                flagFolder = checkFolder();
                if (flagFolder == true)
                {
                    Console.Write(".[ok]\n");
                    Console.Write("Starting BDF Scan...[ok]\n");
                    createFile(origin, "startupload.bat", ftpstartupline);
                    createFile(origin, "upload.bat",ftpuploadLines);
                    callTransferTool(arguments[1]);
                    fillDatalog();
                    createFile(origin,sucursal+".txt",datalog);
                    break;
                }
                else if (i == 1 && flagFolder == true)
                {
                    Console.Write("Checking for dayend 2nd try");
                    Thread.Sleep(60000);
                    Console.Write(".");
                    Thread.Sleep(60000);
                    Console.Write(".");
                    Thread.Sleep(60000);
                    if (checkFolder() == true)
                    {
                        Console.Write(".[ok]\n");
                        Console.Write("Starting BDF Scan");
                        Console.Write("Creating log file...");
                        fillDatalog();
                        createFile(origin, sucursal + ".txt", datalog);

                    }
                    else
                    {
                        Console.Write(".[fail]\n");
                    }
                }
                else if (i == 2 && flagFolder == true)
                {
                    Console.Write("Checking for dayend 3rd try");
                    Thread.Sleep(60000);
                    Console.Write(".");
                    Thread.Sleep(60000);
                    Console.Write(".");
                    Thread.Sleep(60000);
                    if (checkFolder() == true)
                    {
                        Console.Write(".[ok]\n");
                        Console.Write("Starting BDF Scan");
                        Console.Write("Creating log file...");
                        fillDatalog();
                        createFile(origin, sucursal + ".txt", datalog);

                    }
                    else
                    {
                        Console.Write(".[fail]\n");
                    }
                }
                else if (i == 3 && flagFolder == true)
                {
                    Console.Write("Checking for dayend 4th try");
                    Thread.Sleep(60000);
                    Console.Write(".");
                    Thread.Sleep(60000);
                    Console.Write(".");
                    Thread.Sleep(60000);
                    if (checkFolder() == true)
                    {
                        Console.Write(".[ok]\n");
                        Console.Write("Starting BDF Scan");
                        Console.Write("Creating log file...");
                        fillDatalog();
                        createFile(origin, sucursal + ".txt", datalog);

                    }
                    else
                    {
                        Console.Write(".[fail]\n");
                    }
                }
                else if (i == 4 && flagFolder == true)
                {
                    Console.Write("Checking for dayend 5th try");
                    Thread.Sleep(60000);
                    Console.Write(".");
                    Thread.Sleep(60000);
                    Console.Write(".");
                    Thread.Sleep(60000);
                    if (checkFolder() == true)
                    {
                        Console.Write(".[ok]\n");
                        Console.Write("Starting BDF Scan");
                        Console.Write("Creating log file...");
                        fillDatalog();
                        createFile(origin, sucursal + ".txt", datalog);

                    }
                    else
                    {
                        Console.Write(".[fail]\n");
                    }
                }
                else
                {
                    Console.Write(".[fail]\n");
                }
            }
        }

        /// <summary>
        /// Metodo para inicia todo el proceso
        /// </summary>
        public void run()
        {
            fechaFTPinicio = fechaFTPinicio + getDateOrTime(DateTime.Now, "fecha").ToString() + " " + getDateOrTime(DateTime.Now, "hora").ToString();
            Console.Write("landing.");
            Thread.Sleep(1000);
            Console.Write(".");
            Thread.Sleep(1000);
            Console.Write(".[ok]\n");
            folderAttempts();
            
        }

        #endregion

    }
}
