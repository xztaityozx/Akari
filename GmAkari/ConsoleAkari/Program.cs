using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CommandLine;

namespace ConsoleAkari {
    internal class Program {
        private static async Task Main(string[] args) {
            try {
                var opt = Parser.Default.ParseArguments<CliOption>(args)
                    .MapResult(o => o, e => throw new Exception("Failed post or parse"));
                using var client = opt.BuildClient();
                var (cid, text) = (opt.Avatar, string.Join(" ", opt.Text));
                var res = string.IsNullOrEmpty(opt.SaveFile)
                    ? await client.Talk(cid, text)
                    : await client.Save(cid, text, opt.SaveFile);
                Console.WriteLine(res);
            }
            catch (HttpRequestException e) {
                Console.WriteLine("[Error] Failed request to server");
                Console.WriteLine(e);
            }
            catch (Exception e) {
                Console.WriteLine("[Error] Unknown error has occured");
                Console.WriteLine(e);
            }
        }
    }

    public class CliOption {
        [Option('t',"url",Default = "http://localhost:7180", HelpText = "hostname:port")]
        public string Host { get; set; }
        [Option('u', "username",HelpText = "username for basic authorization")]
        public string Username { get; set; }
        [Option('p',"password", HelpText = "Password for basic authorization")]
        public string Password { get; set; }

        [Option('a',"avatar", HelpText = "avatar", Default = 2000)]
        public int Avatar { get; set; }

        [Option('s', "save", Default = null, HelpText = "save to file")]
        public string SaveFile { get; set; }

        [Value(0, HelpText = "Text", MetaName = "text", Required = true)]
        public IEnumerable<string> Text { get; set; }

        public SeikaCenterClient.SeikaCenterClient BuildClient() {
            return new SeikaCenterClient.SeikaCenterClient(Host,Username,Password);
        }
    }

}
