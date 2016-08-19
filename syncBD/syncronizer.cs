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
        private string message ="Mensaje:Esto es una prueba";

        /// <summary>
        /// Fecha y hora donde inicia la transferencia de archivos
        /// </summary>
        private string fechaFTPinicio = "FTPStarted at: " ;

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
        private string fechaFTPFin ="FTPEnded at: ";
        private bool flagFolder = false;
        private string horainicioFTP = "";
        private string horaFinFTP = "";
        private string horainicioLog = "";
        private string horafinLog = "";

        private string origin = Directory.GetCurrentDirectory();
        private string destiny = ConfigurationManager.AppSettings["destiny"];
        private string logDestiny = ConfigurationManager.AppSettings["destiny"];
        //private string origin = ConfigurationManager.AppSettings["origin"];
        //private string destiny = ConfigurationManager.AppSettings["destiny"];
        //private string logDestiny = ConfigurationManager.AppSettings["logDestiny"];
        //private string store = ConfigurationManager.AppSettings["store"];

        private string[] transferData = new string[2];
        private string[] datalog = new string[5];

        #endregion

        #region metodos
        /// <summary>
        /// Metodo para crear archivo de texto que se usara para log
        /// </summary>
        /// <param name="Lineas">Arreglo de strings que son las lineas a insertar en el txt</param>
        private void createTxt(string[] Lines)
        {
            try
            {
                // Example #3: Write only some strings in an array to a file.
                // The using statement automatically flushes AND CLOSES the stream and calls 
                // IDisposable.Dispose on the stream object.
                // NOTE: do not use FileStream for text files because it writes bytes, but StreamWriter
                // encodes the output as text.
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(origin + "\\" + sucursal + ".txt"))
                {
                    foreach (string line in Lines)
                    {
                        file.WriteLine(line);
                    }
                }
                Console.Write(".");
                Thread.Sleep(1000);
                Console.Write("[ok]");
            }
            catch (Exception)
            {

                Console.WriteLine("[fail]");
            }

        }

        /// <summary>
        /// Metodo para mandar llamar la herramienta para transferencia de archivos
        /// </summary>
        private void callRobocopy(string[] folders)
        {
            try
            {
                //Folders[]
                //[0] origin
                //[1] destinarion
                Process p = new Process();
                p.StartInfo.Arguments = string.Format("/C Robocopy /S {0} {1}", folders[0], folders[1]);
                p.StartInfo.FileName = "CMD.EXE";
                p.StartInfo.CreateNoWindow = false;
                p.StartInfo.UseShellExecute = true;
                p.Start();
                p.WaitForExit();
                Console.Write("[ok]\n");

                //// TODO: Add exception handling
                //string processName = command.Split(delimiters).ToList().ElementAt(0);    // split up command into pieces, select first "token" as the process name
                //string commandArguments = command.Replace(processName + " ", ""); // remove the process name and following whitespace from the command itself, storing it in a new variable
                //Process commandProcess = new Process(); // declare a new process to be used
                //commandProcess.StartInfo.FileName = processName;    // add file start info for filename to process
                //commandProcess.StartInfo.Arguments = commandArguments;  // add file start info for arguments to process
                //commandProcess.StartInfo.UseShellExecute = false;  // skip permissions request
                //commandProcess.Start();   // start process according to command's data
                //commandProcess.WaitForExit();   // wait for the process to exit before continuing
                //bool commandSuccessful = ParseCommandErrorCode(commandProcess, commandProcess.ExitCode);    // grab error code
                //if (!commandSuccessful)
                //{
                //    // ERROR! abort operation and inform the user of the last completed operation, and how many commands have not been run
                //} // end if
                //Console.WriteLine("Error code: {0}", commandProcess.ExitCode);    // print error code
                //commandProcess.Close(); // close process
            }
            catch (Exception)
            {
                Console.Write("[fail]");
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
        public string[] fillDatalog()
        {
            fechaFTPFin = fechaFTPFin + getDateOrTime(DateTime.Now, "fecha").ToString() + " " + horaFinFTP + getDateOrTime(DateTime.Now, "hora").ToString();
            datalog[0] = message;
            datalog[1] = sucursal;
            datalog[2] = fechaFTPinicio;
            datalog[3] = fechaFTPFin;

            return datalog;
        } 

        /// <summary>
        /// Metodo para llenar las carpetas que seran origin y destino
        /// </summary>
        /// <returns></returns>
        public string[] fillDataFolders()
        {
            transferData[0] = origin;
            transferData[1] = destiny;
            return transferData;
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
            Console.Write("Checking for dayend 1st");
            Thread.Sleep(1000);
            Console.Write(".");
            Thread.Sleep(1000);
            Console.Write(".");
            Thread.Sleep(1000);
            flagFolder = checkFolder();
            for (int i = 0; i <= 4; i++)
            {
                if (flagFolder == true)
                {
                    Console.Write(".[ok]\n");
                    Console.Write("Starting BDF Scan");
                    Thread.Sleep(1000);
                    Console.Write(".");
                    Thread.Sleep(1000);
                    Console.Write(".");
                    Thread.Sleep(1000);
                    Console.Write(".[ok]\n");
                    fillDataFolders();
                    Console.Write("Creating log file");
                    Thread.Sleep(1000);
                    Console.Write(".");
                    Thread.Sleep(1000);
                    fillDatalog();
                    createTxt(datalog);
                    callRobocopy(transferData);
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
                        Thread.Sleep(1000);
                        Console.Write(".");
                        Thread.Sleep(1000);
                        Console.Write(".");
                        Thread.Sleep(1000);
                        Console.Write(".[ok]\n");
                        fillDataFolders();
                        Console.Write("Creating log file");
                        Thread.Sleep(1000);
                        Console.Write(".");
                        Thread.Sleep(1000);
                        fillDatalog();
                        createTxt(datalog);
                        callRobocopy(transferData);
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
                        Thread.Sleep(1000);
                        Console.Write(".");
                        Thread.Sleep(1000);
                        Console.Write(".");
                        Thread.Sleep(1000);
                        Console.Write(".[ok]\n");
                        fillDataFolders();
                        Console.Write("Creating log file");
                        Thread.Sleep(1000);
                        Console.Write(".");
                        Thread.Sleep(1000);
                        fillDatalog();
                        createTxt(datalog);
                        callRobocopy(transferData);
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
                        Thread.Sleep(1000);
                        Console.Write(".");
                        Thread.Sleep(1000);
                        Console.Write(".");
                        Thread.Sleep(1000);
                        Console.Write(".[ok]\n");
                        fillDataFolders();
                        Console.Write("Creating log file");
                        Thread.Sleep(1000);
                        Console.Write(".");
                        Thread.Sleep(1000);
                        fillDatalog();
                        createTxt(datalog);
                        callRobocopy(transferData);
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
                        Thread.Sleep(1000);
                        Console.Write(".");
                        Thread.Sleep(1000);
                        Console.Write(".");
                        Thread.Sleep(1000);
                        Console.Write(".[ok]\n");
                        fillDataFolders();
                        Console.Write("Creating log file");
                        Thread.Sleep(1000);
                        Console.Write(".");
                        Thread.Sleep(1000);
                        fillDatalog();
                        createTxt(datalog);
                        callRobocopy(transferData);
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
