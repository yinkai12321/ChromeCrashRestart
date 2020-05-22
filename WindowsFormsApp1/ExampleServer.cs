using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTTPServerLib;
using System.IO;
using Newtonsoft.Json;
using System.Timers;

namespace HttpServer
{
    public class ExampleServer : HTTPServerLib.HttpServer
    {
        /// <summary>
        /// 是否连接
        /// </summary>
        public static bool ISCONNECT = false;

        private static System.Timers.Timer aTimer;

        public delegate void Callback();
        public Callback callback;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="port">端口号</param>
        public ExampleServer(string ipAddress, int port)
            : base(ipAddress, port)
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += (Object source, ElapsedEventArgs e) =>{
                ISCONNECT = false;
            };
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        public override void OnPost(HttpRequest request, HttpResponse response)
        {

            try
            {
                //获取客户端传递的参数
                string data = request.Params == null ? "" : string.Join(";", request.Params.Select(x => x.Key + "=" + x.Value).ToArray());


                var url = request.URL.Split('?')[0];
                var filename = request.URL.Split('?')[1].Split('=')[1];

                if (url.ToLower() == "/keep")
                {
                    ISCONNECT = true;
                    Resonse(response, 0, "Ok");
                }

            }
            catch (Exception)
            {
                Resonse(response,0, "系统错误");
                return;
            }


        }

        public void Resonse(HttpResponse response,int type,string message)
        {
            //设置返回信息
            string content = JsonConvert.SerializeObject(new { type = type, message = message });

            //构造响应报文
            response.SetContent(content);
            response.Content_Encoding = "utf-8";
            response.StatusCode = "200";
            response.Content_Type = "text/html;charset=UTF-8";
            response.Headers["Server"] = "ExampleServer";
            response.Headers["Access-Control-Allow-Origin"] = "*";
            response.Headers["Access-Control-Allow-Method"] = "POST,GET";

            //发送响应
            response.Send();
        }

        public override void OnGet(HttpRequest request, HttpResponse response)
        {
            if (request.URL.ToLower() == "/print")
            {
                response.StatusCode = "404";
                response.SetContent("Hello word", Encoding.UTF8);
                response.Content_Type = "text/html; charset=UTF-8";
                response.Send();
                return;
            }


            if (request.URL.ToLower() == "/keep")
            {
                ISCONNECT = true;

                callback();

                Resonse(response, 0, "Ok");
            }


            /////链接形式1:"http://localhost:4050/assets/styles/style.css"表示访问指定文件资源，
            /////此时读取服务器目录下的/assets/styles/style.css文件。

            /////链接形式1:"http://localhost:4050/assets/styles/"表示访问指定页面资源，
            /////此时读取服务器目录下的/assets/styles/style.index文件。

            ////当文件不存在时应返回404状态码
            //string requestURL = request.URL;
            //requestURL = requestURL.Replace("/", @"\").Replace("\\..", "").TrimStart('\\');
            //string requestFile = Path.Combine(ServerRoot, requestURL);

            ////判断地址中是否存在扩展名
            //string extension = Path.GetExtension(requestFile);

            ////根据有无扩展名按照两种不同链接进行处
            //if (extension != "")
            //{
            //    //从文件中返回HTTP响应
            //    response = response.FromFile(requestFile);
            //} 
            //else
            //{
            //    //目录存在且不存在index页面时时列举目录
            //    if (Directory.Exists(requestFile) && !File.Exists(requestFile + "\\index.html"))
            //    {
            //        requestFile = Path.Combine(ServerRoot, requestFile);
            //        var content = ListDirectory(requestFile, requestURL);
            //        response = response.SetContent(content, Encoding.UTF8);
            //        response.Content_Type = "text/html; charset=UTF-8";
            //    } 
            //    else
            //    {
            //        //加载静态HTML页面
            //        requestFile = Path.Combine(requestFile, "index.html");
            //        response = response.FromFile(requestFile);
            //        response.Content_Type = "text/html; charset=UTF-8";
            //    }
            //}

            //发送HTTP响应
            response.Send();
        }

        public override void OnDefault(HttpRequest request, HttpResponse response)
        {

        }

        private string ConvertPath(string[] urls)
        {
            string html = string.Empty;
            int length = ServerRoot.Length;
            foreach (var url in urls)
            {
                var s = url.StartsWith("..") ? url : url.Substring(length).TrimEnd('\\');
                html += String.Format("<li><a href=\"{0}\">{0}</a></li>", s);
            }

            return html;
        }

        private string ListDirectory(string requestDirectory, string requestURL)
        {
            //列举子目录
            var folders = requestURL.Length > 1 ? new string[] { "../" } : new string[] { };
            folders = folders.Concat(Directory.GetDirectories(requestDirectory)).ToArray();
            var foldersList = ConvertPath(folders);

            //列举文件
            var files = Directory.GetFiles(requestDirectory);
            var filesList = ConvertPath(files);

            //构造HTML
            StringBuilder builder = new StringBuilder();
            builder.Append(string.Format("<html><head><title>{0}</title></head>", requestDirectory));
            builder.Append(string.Format("<body><h1>{0}</h1><br/><ul>{1}{2}</ul></body></html>",
                 requestURL, filesList, foldersList));

            return builder.ToString();
        }
    }
}
