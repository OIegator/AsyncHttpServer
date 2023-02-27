using System;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

class AsyncHttpServer
{
    private const string UriPrefix = "http://127.0.0.1:8080/";
    static string? _baseFolder;
    private readonly HttpListener listener = new HttpListener();
    //private readonly StreamWriter logWriter;
    private static bool alive;
    

    static void Main(string[] args)
    {
        var server = new AsyncHttpServer(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName);
        server.Start();
    }
    public AsyncHttpServer(string baseFolder)
    {
        _baseFolder = baseFolder;
        alive = true;
        listener.Prefixes.Add(UriPrefix);
        //var logFilePath = Path.Combine(_baseFolder, "log.txt");
        //logWriter = new StreamWriter(logFilePath, true, Encoding.UTF8);
    }
    public void Start()
    {
        listener.Start();
        Console.WriteLine("Сервер запущен...");
       
        var listenerTask = ProccesAsync(listener);

        var cmd = Console.ReadLine();

        if (cmd.Equals("q", StringComparison.OrdinalIgnoreCase)) {
            alive = false;
            //await Task.WhenAll(getuserTask)        }
    }

    static async Task ProccesAsync(HttpListener listener)
    { 
        var getuserTask = new List<Task>();
        while (alive)
        {
            var context = listener.GetContextAsync();
            Task hr = HandleRequestAsync(context);
            getuserTask.Add(hr);
        }
        await Task.WhenAll(getuserTask);
    }

    static async Task HandleRequestAsync(Task<HttpListenerContext> context)
    {
        await Task.Delay(1000);
        Perform(context); // добавить в список тасков
    }
    static void Perform(Task<HttpListenerContext> context)
    {
        var filename = context.Result.Request.Url.AbsolutePath;
        Console.WriteLine($"Запрос: {filename} от {context.Result.Request.RemoteEndPoint.Address}");


        var filePath = Path.Combine(_baseFolder, filename.TrimStart('/'));
        if (File.Exists(filePath))
        {
            context.Result.Response.StatusCode = 200;
            SendFile(context.Result.Response, filePath);
            //logWriter.WriteLine($"{DateTime.Now} {context.Request.RemoteEndPoint.Address} {filename} {context.Response.StatusCode}");
            //logWriter.Flush();
        }
        else
        {
            context.Result.Response.StatusCode = 404;
            //logWriter.WriteLine($"{DateTime.Now} {context.Request.RemoteEndPoint.Address} {filename} {context.Response.StatusCode}");
            //logWriter.Flush();
            context.Result.Response.Close();
        }
    }

    static void SendFile(HttpListenerResponse response, string filePath)
    {
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            stream.CopyTo(response.OutputStream);
        }

        response.Close();
    }

}

