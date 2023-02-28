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
    private static bool alive;
    private readonly StreamWriter logWriter;


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
        var logFilePath = Path.Combine(_baseFolder, "log.txt");
        logWriter = new StreamWriter(logFilePath, true, Encoding.UTF8);
    }
    public void Start()
    {
        listener.Start();
        Console.WriteLine("Сервер запущен...");
        var getUserTask = new HashSet<Task>();
        var listenerTask = ProccesAsync(listener);
        var cmd = Console.ReadLine();

        if (cmd.Equals("q", StringComparison.OrdinalIgnoreCase))
        {
            alive = false;
            Task.WhenAll(getUserTask).Wait();
        }

        async Task ProccesAsync(HttpListener listener)
        {
            while (alive)
            {
                var context = await listener.GetContextAsync();
                var currentTask = Perform(context);
                Console.WriteLine(currentTask.Id);

                _ = currentTask.ContinueWith(task =>
                    getUserTask.RemoveWhere(task =>
                    {
                        return task.Id == currentTask.Id;
                    })
                );

            }

        }

        async static Task Perform(HttpListenerContext context)
        {
            await Task.Delay(3000);
            var filename = context.Request.Url.AbsolutePath;
            Console.WriteLine($"Запрос: {filename} от {context.Request.RemoteEndPoint.Address}");


            var filePath = Path.Combine(_baseFolder, filename.TrimStart('/'));
            if (File.Exists(filePath))
            {
                context.Response.StatusCode = 200;
                await SendFile(context.Response, filePath);
                //logWriter.WriteLine($"{DateTime.Now} {context.Request.RemoteEndPoint.Address} {filename} {context.Response.StatusCode}");
                //logWriter.Flush();
            }
            else
            {
                context.Response.StatusCode = 404;
                //logWriter.WriteLine($"{DateTime.Now} {context.Request.RemoteEndPoint.Address} {filename} {context.Response.StatusCode}");
                //logWriter.Flush();
                context.Response.Close();
            }
        }

        static async Task SendFile(HttpListenerResponse response, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(response.OutputStream);
            }

            response.Close();
        }

    }
}

