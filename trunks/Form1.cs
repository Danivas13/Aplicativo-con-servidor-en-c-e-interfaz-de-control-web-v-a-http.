using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExtractor;


namespace trunks
{
    public partial class Form1 : Form
    {
       
       String ultimarespuesta = "";
       List<String> coladedescargasa= new List<String>();
       List<String> coladedescargasb = new List<String>();
       List<String> titulsdescargas = new List<String>();
       List<String> estadocoladescas = new List<String>();
       List<String> mp3s;
       List<String> mp3subi = new List<String>();
       List<String> enviosupdate = new List<String>();
       Boolean actualmente = false;
       Boolean iniciadomonitoreo = false;
       String session = "";

        public Form1()
        {
            
            InitializeComponent();
            WebServer(5001, "web");
            Start();
            Listen();
 
            
        }

       
         //Variables y (gets & sets)
        private String origenFile { get; set; }
        public String inFile { get; set; }
        private String outFile { get; set; }

        //metodo Extraer
        public void Extraer(String archivo)
        {
            //Obtiene el archivo que se desea modificar
          //  MessageBox.Show(archivo.Replace("/", "\\"));
            
            FileInfo infoFile = new FileInfo(archivo.Replace("/","\\"));
            //Verifica si es menor de 5mb
         //   if (getDuracion(infoFile) < (decimal)5.00)
          //  {
                //Obtine elorige
               origenFile = infoFile.DirectoryName;
               
                //salida de nuevo archivo
               outFile = Path.GetFileNameWithoutExtension(archivo) + ".mp3";
               string inFileOrigen = Path.Combine(origenFile, archivo);
                string outFileOrigen = Path.Combine(origenFile, outFile);
                //llamdo de origenes de archivos   
                string param = string.Format(@" -i ""{0}"" ""{1}""", inFileOrigen, outFileOrigen);
                MessageBox.Show(param);
                //proceso de extraccion
                ProcessStartInfo proceso = new ProcessStartInfo();
                proceso.CreateNoWindow = true;
                proceso.UseShellExecute = false;
                if (IntPtr.Size == 8)
                {
                    // Máquina con procesador de 64 bits
                  //  MessageBox.Show("64");
                    proceso.FileName = AppDomain.CurrentDomain.BaseDirectory + "ffmpeg64.exe";
                }
                else if (IntPtr.Size == 4)
                {
                    // Máquina con procesador de 32 bits
                  //  MessageBox.Show("32");
                    proceso.FileName = AppDomain.CurrentDomain.BaseDirectory + "ffmpeg32.exe";
                }
                
                proceso.Arguments = param;
                proceso.RedirectStandardOutput = true;
                using (Process p = Process.Start(proceso))
                {
                    p.WaitForExit();
                }
                actualizar(Path.GetFileNameWithoutExtension(archivo) + ".mp3", "addcrear");
           // }
          //  else
          //  {
         //       MessageBox.Show("EL fichero es superior a 5 MB");
         //   }

        }
        //obtiene la duracion del archivo
        decimal getDuracion(FileInfo file)
        {
            decimal duracion = ((file.Length / 1024) / 1024);
            return Math.Round(duracion, 2);
        }
        public void actualizar(String id,String text)
        {
            //tipos de renvio
            //B: Buscando
            //D: Descargando
            //C: Convirtiendo
            //E: Enviando al telefono
            //W: Esperando
            //X: Error
            //R: Terminado
          //  MessageBox.Show(text);
            Invoke(new MethodInvoker(delegate()
            {
                if (text == "Buscando")
                {
                    Actualizarcliente(id, "B");
                }
                else if (text == "Descargando")
                {
                    Actualizarcliente(id, "D");
                }
                else if (text == "Convirtiendo")
                {
                    Actualizarcliente(id, "C");
                }
                else if (text == "Enviando")
                {
                    Actualizarcliente(id, "E");
                }
                else if (text == "Terminado")
                {
                   // Actualizarcliente(id, "R");
                    enviosupdate.Add("[{A}][{"+id+"}][{R}]");
                    actualmente = false;

                    if (coladedescargasa.Contains(id) )
                    {
                        coladedescargasa.RemoveAt(0);
                        coladedescargasb.RemoveAt(0);
                        estadocoladescas.RemoveAt(0);
                        titulsdescargas.RemoveAt(0);
                    }
                    if (coladedescargasa.Count > 0)
                    {
                        descargarfyoutube(coladedescargasa[0], coladedescargasb[0]);
                    }

                }
                else if (text == "Espera")
                {
                    Actualizarcliente(id, "W");
                }
                else if (text == "Error")
                {
                    Actualizarcliente(id, "X");
                    actualmente = false;
                }
                else if (text == "Remove")
                {
                    Actualizarcliente(id, "DELETE");
                }
                else if (text == "addcrear")
                {
                    mp3s.Add(id.Replace(".mp3", ""));
                    mp3subi.Add(id);
                    int indx = mp3subi.IndexOf(id);
                    enviosupdate.Add("[{A}][{song" + indx + "/yol/" + id.Replace(".mp3", "") + "}][{MAKE}]");

                }
            }));
            
        }
        public void WebServer(int port, string path)
        {
            this.port = port;
            this.home = path;
            listener = new TcpListener(IPAddress.Any, port);
            
        }

        public void Start()
        {
            listener.Start();
            label1.Text = "En linea";
           // MessageBox.Show(string.Format("Local server started at localhost:{0}", port));
            
           /*/ Console.CancelKeyPress += delegate
            {
                Console.WriteLine("Stopping server");
                StopServer();
            };/*/
        }

        public void Listen()
        {

            //aqui danivas
            Thread threadprin = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        Byte[] result = new Byte[MAX_SIZE];
                        string requestData;
                       // MessageBox.Show("algo");
                        TcpClient client = listener.AcceptTcpClient();
                        NetworkStream stream = client.GetStream();
                        int size = stream.Read(result, 0, result.Length);
                        requestData = System.Text.Encoding.ASCII.GetString(result, 0, size);
                        /*/  MessageBox.Show("Received: {0}", requestData);
                          MessageBox.Show("_1_________________________\n\r " + requestData);/*/
                        Request request = GetRequest(requestData);
                        ProcessRequest(request, stream);
                        client.Close();
                    }
                }
                finally
                {
                    listener.Stop();
                }
            });
            threadprin.Start();
           
        }

        private void ProcessRequest(Request request, NetworkStream stream)
        {
            
           
            if (request == null)
            {
                return;
            }

         
          if (request.Path.Equals("/comunicador.txt"))

            {

            
                   for (int x = 0; x <= coladedescargasa.Count - 1; x++)
                    {
                        if (x > 0)
                        {
                            ultimarespuesta = ultimarespuesta + "&";
                        }
                        ultimarespuesta = ultimarespuesta + "[{A}][{" + coladedescargasa[x] + "}][{" + estadocoladescas[x] + "}][{ " + titulsdescargas[x] + "}]";
                        if (enviosupdate.Count > 0)
                        {
                            ultimarespuesta = ultimarespuesta + "&";
                        }
                    }
                   if (enviosupdate.Count > 0)
                   {
                       for(int y = 0; y <= enviosupdate.Count-1;y++){
                           ultimarespuesta = ultimarespuesta + enviosupdate[y];
                           if (y < enviosupdate.Count - 1)
                           {
                               ultimarespuesta = ultimarespuesta + "&";
                           }
                       }
                       enviosupdate.Clear();
                       
                   }


               
                GenerateResponse("%"+session+"%@"+ultimarespuesta+"@", stream, OK200);
                
                ultimarespuesta = "";
            }
          else if ((request.Path.Length >= 9) && (request.Path.Substring(0, 9).Equals("/conectar")))
            {
                String nueva = request.Path.Split('-')[1]; ;
                if (nueva != session)
                {
                    session = nueva;
                }
                GenerateResponse("", stream, OK200);

                
            }
          else if ((request.Path.Length >= 7) && (request.Path.Substring(0, 7).Equals("/status")))
            {
                
                String valor = request.Path.Split(new string[] { "/status/" }, StringSplitOptions.None)[1];
                if (valor.Length > 0)
                {
                    String idval = valor.Split(new string[] { "watch?v=" }, StringSplitOptions.None)[1];
                    int i = coladedescargasa.IndexOf(idval);
                    if (i > -1)
                    {
                        GenerateResponse("@" + estadocoladescas[i] + "@", stream, OK200);
                    }
                    else
                    {
                        GenerateResponse("@A@", stream, OK200);
                    }
                }
                else
                {
                    GenerateResponse("@WTH@", stream, OK200);
                }
            }
              else if ((request.Path.Length >= 10) && (request.Path.Substring(0, 10).Equals("/descargar")))
              {
                  String valor = request.Path.Split(new string[] { "/descargar/" }, StringSplitOptions.None)[1];
                  if (valor.Length > 0)
                  {
                      String idval = valor.Split(new string[] { "watch?v=" }, StringSplitOptions.None)[1];
                   

                      if (actualmente == true)
                      {
                          if ((coladedescargasa.Contains(idval)) || (coladedescargasb.Contains(valor)))
                          {

                          }
                          else
                          {
                              coladedescargasa.Add(idval);
                              coladedescargasb.Add(valor);
                              titulsdescargas.Add("");
                              estadocoladescas.Add("W");
                              Actualizarcliente(idval, "W");

                          }

                      }
                      else
                      {
                          coladedescargasa.Add(idval);
                          coladedescargasb.Add(valor);
                          titulsdescargas.Add("");
                          estadocoladescas.Add("W");
                          Actualizarcliente(idval, "W");
                          descargarfyoutube(idval, valor);
                      }
                      enviosupdate.Add("[{A}][{A}][{UPDATE}]");
                      GenerateResponse("", stream, OK200);
                     // GenerateResponse("", stream, OK200);
                  }
                  else
                  {
                      GenerateResponse("@WTH@", stream, OK200);
                  }
              }
            else if ((request.Path.Length >= 11) && (request.Path.Substring(0, 10).Equals("/solicitar")))
            {

                String dato = Uri.UnescapeDataString(request.Path.Split(new string[] { "r=@" }, StringSplitOptions.None)[1]);
                procesarsolicitud(dato);
                GenerateResponse("", stream, OK200);
               
            }
          else if ((request.Path.Length >= 9) && (request.Path.Substring(0, 9).Equals("/procesos")))
          {
              if (coladedescargasa.Count > 0)
              {
                  string temp = "";
                  for (int x = 0; x <= coladedescargasa.Count - 1; x++)
                  {

                      temp += coladedescargasa[x] + "/nay/" + coladedescargasb[x] + "/nay/" + titulsdescargas[x] + "/nay/" + estadocoladescas[x];
                      if (x < mp3s.Count - 1)
                      {
                          temp += "/yol/";
                      }
                  }
                  GenerateResponse("@" + temp + "@", stream, OK200);
              }
              else
              {
                  GenerateResponse("@@", stream, OK200);
              }
          }
          else if ((request.Path.Length >= 6) && (request.Path.Substring(0, 6).Equals("/lista")))
          {
              mp3s = obtenerArchivosDirectorio(Application.StartupPath + "\\Musica\\");

              if (mp3s.Count > 0)
              {
                  string temp = "";
                  for (int x = 0; x <= mp3s.Count - 1; x++)
                  {
                      /*/UTF8Encoding utf8 = new UTF8Encoding();
                     
                      byte[] encodedBytes = utf8.GetBytes(mp3subi[x]);
                      string message = System.Text.Encoding.Default.GetString(encodedBytes);
                      MessageBox.Show(message);/*/
                      temp += mp3s[x] + "/nay/song" + x;
                      if (x < mp3s.Count - 1)
                      {
                          temp += "/yol/";
                      }
                  }
                  GenerateResponse("@" + temp + "@", stream, OK200);
              }
              else
              {
                  GenerateResponse("@@", stream, OK200);
              }


              if (iniciadomonitoreo == false)
              {

                  monitorear();
                  iniciadomonitoreo = true;
              }
          }
          else
          {


              if (request.Path.Equals("/"))
                  request.Path = "/index.html";
              ParsePath(request);
              if (File.Exists(request.Path))
              {
                  /*/   if (request.Path.Equals("/index.html"))
                     {

                     }/*/

                  var fileContent = File.ReadAllText(request.Path);
                  GenerateResponse(fileContent, stream, OK200);
                  return;
              }

              GenerateResponse("Not found", stream, NOTFOUND404);
          }
        }

        private void ParsePath(Request request)
        {
            request.Path.Replace('/', '\\');
            request.Path = home + request.Path;
        }

        private void GenerateResponse(string content, 
            NetworkStream stream,
            string responseHeader)
        {
            string response = "HTTP/1.1 200 OK\r\n\r\n\r\n";
            response = response + content;
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);
            stream.Write(msg, 0, msg.Length);
            return;
        }

        private void StopServer()
        {
            listener.Stop();
        }

        private Request GetRequest(string data)
        {
            Request request = new Request();
            var list = data.Split(' ');
            if (list.Length < 3)
                return null;

            request.Command = list[0];
            request.Path = list[1];
            request.Protocol = list[2].Split('\n')[0];
            
          //  MessageBox.Show("Instruction: {0}\nPath: {1}\nProtocol: {2}",request.Command,request.Path, request.Protocol);
            
           
            return request;
        }

        private TcpListener listener;
        private int port;
        private string home;
        private static string NOTFOUND404 = "HTTP/1.1 404 Not Found";
        private static string OK200 = "HTTP/1.1 200 OK\r\n\r\n\r\n";
        private static int MAX_SIZE = 1000;
    

    public class Request
    {
        public string Command;
        public string Path;
        public string Protocol;
    }

  
    public void procesarsolicitud(String datos)
    {
       

        String[] soli = Uri.UnescapeDataString(datos).Split('&');
        String idval = soli[0].Split('=')[1].TrimEnd(']');
        String tipo = soli[1].Split('=')[1].TrimEnd(']');
        String valor = soli[2].Split(new string[] { "codigo=" }, StringSplitOptions.None)[1].TrimEnd(']');

     //   MessageBox.Show("Es una solicitud de tipo " + tipo + "\n\r  que lleva como valor " + valor + " con id: " + idval);
        if (tipo == "uno")
        {
           //descargar video
            if (actualmente == true)
            {
                if((coladedescargasa.Contains(idval)) || (coladedescargasb.Contains(valor))){
                 
                }else{
                    coladedescargasa.Add(idval);
                coladedescargasb.Add(valor);
                titulsdescargas.Add("NI");
                estadocoladescas.Add("W");
                Actualizarcliente(idval, "W");
              
                }
                
            }
            else
            {
                coladedescargasa.Add(idval);
                coladedescargasb.Add(valor);
                titulsdescargas.Add("NI");
                estadocoladescas.Add("W");
                Actualizarcliente(idval, "W");
                descargarfyoutube(idval, valor);
            }
            
            
        }
        else if (tipo == "dos")
        {
          //eliminar cancion
        }
        else if (tipo == "tres")
        {
           
        }
    }
    
    public void Actualizarcliente(String id,String codigo)
    {
        //tipos de renvio
        //B: Buscando
        //D: Descargando
        //C: Convirtiendo
        //E: Enviando al telefono
        //W: Esperando
        //X: Error
        //esto va acompañado de un id que hace referencia a un elemento al cual debe actualizarse
        //enviar al cliente
       /*/ StreamWriter ficheroEscritura = new StreamWriter(Application.StartupPath + "\\web\\comunicador.txt");
        ficheroEscritura.WriteLine("[{" + actualsend + "}][{" + id + "}][{" + codigo + "}]");
        ficheroEscritura.Close();/*/
        /*/if (ultimarespuesta == "")
        {
            ultimarespuesta = "[{" + actualsend + "}][{" + id + "}][{" + codigo + "}]";
        }
        else
        {
            ultimarespuesta = ultimarespuesta + "&[{" + actualsend + "}][{" + id + "}][{" + codigo + "}]";
        }/*/
        if (coladedescargasb.Count > 0)
        {
            int idx = coladedescargasa.IndexOf(id);
            estadocoladescas[idx] = codigo;
        }
        

        


        
    }
    public void descargarfyoutube(String idval,String enla)
    {
        actualmente = true;
        //descargamos el video de youtube
        Thread threadprin = new Thread(() =>
        {
            actualizar(idval,"Buscando");
            IEnumerable<VideoInfo> videos = DownloadUrlResolver.GetDownloadUrls(enla);

            VideoInfo video = videos.First(p => p.VideoType == VideoType.Mp4 && p.Resolution == 360);
         //   MessageBox.Show(video.DownloadUrl.ToString());
            if (video.RequiresDecryption)
                DownloadUrlResolver.DecryptDownloadUrl(video);
            int danivas = coladedescargasb.IndexOf(enla);
            titulsdescargas[danivas] = ajustar(video.Title);
            VideoDownloader downloader = new VideoDownloader(video, Path.Combine(Application.StartupPath + "\\Musica\\", ajustar(video.Title) + video.VideoExtension));

            Thread thread = new Thread(() =>
            {
                actualizar(idval, "Descargando");
                downloader.Execute();
                actualizar(idval, "Convirtiendo");
             //   MessageBox.Show("extra");
                Extraer(Path.Combine(Application.StartupPath + "\\Musica\\", ajustar(video.Title) + video.VideoExtension));
                actualizar(idval, "Terminado");
                //eliminamos el archivo original de video para que solo quede el mp3
                //String oldname = Path.GetFileNameWithoutExtension(Path.Combine(Application.StartupPath + "\\Musica\\", ajustar(video.Title) + video.VideoExtension)) + ".pmp3";
              //  String newname = Path.GetFileNameWithoutExtension(Path.Combine(Application.StartupPath + "\\Musica\\", ajustar(video.Title) + video.VideoExtension))+".mp3";
                File.Delete(Path.Combine(Application.StartupPath + "\\Musica\\", ajustar(video.Title) + video.VideoExtension));
              //  pmp renombramos el archivo de la cancion por mp3
                //Path.GetFileNameWithoutExtension(Path.Combine(Application.StartupPath + "\\Musica\\", ajustar(video.Title) + video.VideoExtension))+"pmp3";
               

            }) { IsBackground = true };
            thread.Start();
        });
        threadprin.Start();
    }
    public string ajustar(String cadena)
    {
        String nueva = cadena;
        String[] MyChar = { "/", "\\", ":", "*", "\"", "<", ">", "|" };
        for (int x = 0; x <= MyChar.Length - 1; x++)
        {
            nueva = nueva.Replace(MyChar[x], "");
        }
        
        
        return nueva;
    }
    public List<string> obtenerArchivosDirectorio(string rutaArchivo)
    {

        List<String> listaRuta = new List<String>();
        DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(rutaArchivo));
        FileInfo[] files = directory.GetFiles("*.mp3");
        for (int i = 0; i < files.Length; i++)
        {
                listaRuta.Add(((FileInfo)files[i]).Name.Replace(".mp3",""));
                mp3subi.Add(((FileInfo)files[i]).FullName);
        }
        //listaRuta = Directory.GetFiles(Path.GetDirectoryName(rutaArchivo), Path.GetFileName(rutaArchivo)).ToList();
           return listaRuta;
    }
    static bool _continue = true;
    public void monitorear()
    {
        FileSystemWatcher watcher = new FileSystemWatcher();
        watcher.Path = Application.StartupPath + "\\Musica\\";
        watcher.Filter = "*.mp3";
        watcher.IncludeSubdirectories = false; 
        watcher.EnableRaisingEvents = true;
        watcher.Created += new FileSystemEventHandler(OnCreated);
        watcher.Deleted += new FileSystemEventHandler(FSWatcher_Deleted);
     // watcher.Changed += new FileSystemEventHandler(FSWatcher_Changed);
        watcher.Renamed += new RenamedEventHandler(FSWatcher_Renamed);
    }
    public void OnCreated(object sender, FileSystemEventArgs e)
    {
        mp3s.Add(e.Name.Replace(".mp3", ""));
        mp3subi.Add(e.FullPath);
        int indx = mp3subi.IndexOf(e.FullPath);
        enviosupdate.Add("[{A}][{song" + indx + "/yol/" + e.Name.Replace(".mp3", "") + "}][{MAKE}]");
    }
    public void FSWatcher_Deleted(object sender, FileSystemEventArgs e)
    {
       
        int indx = mp3subi.IndexOf(e.FullPath);

        enviosupdate.Add("[{A}][{song"+indx+"}][{DELETE}]");
       
    }
    static void FSWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        MessageBox.Show("Se ha creado el archivo '{0}'", e.Name);
    }
    public void FSWatcher_Renamed(object sender, RenamedEventArgs e)
    {
       /*/ MessageBox.Show("Se ha creado el archivo '{0}'", e.Name);
        Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);/*/
        int indx = mp3subi.IndexOf(e.OldFullPath);
        mp3s[indx] = e.Name.Replace(".mp3", "");
        mp3subi[indx] = e.FullPath;
        enviosupdate.Add("[{A}][{song" + indx + "/yol/" + e.Name.Replace(".mp3", "") + "}][{EDIT}]");
    }
    private void button3_Click(object sender, EventArgs e)
    {
       // Actualizarcliente("hola");
    }


    
    }
}
