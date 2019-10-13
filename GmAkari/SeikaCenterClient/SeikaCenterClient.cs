using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeikaCenterClient {
    public class SeikaCenterClient : IDisposable {
        private const string Avatar2 = "AVATOR2", Avatar = "AVATOR", App = "app", Play2 = "PLAY2", Save2 = "SAVE2";
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private readonly AuthenticationHeaderValue authentication;

        public SeikaCenterClient(string baseUrl, string username, string password) {
            this.baseUrl = baseUrl;
            httpClient = new HttpClient();
            authentication = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{username}:{password}")
                )
            );
        }

        private HttpRequestMessage BuildRequestMessage(HttpMethod method, params string[] segments) {
            return new HttpRequestMessage(method, JoinUrl(new[] {baseUrl}.Concat(segments))) {
                Headers = {Authorization = authentication}
            };
        }

        /// <summary>
        /// Access to /AVATOR entry point
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAvatar() {
            using var req = BuildRequestMessage(HttpMethod.Get, Avatar);
            return await (await httpClient.SendAsync(req)).Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Access to /AVATOR2 entry point
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAvatar2() {
            using var req = BuildRequestMessage(HttpMethod.Get, Avatar2);
            return await (await httpClient.SendAsync(req)).Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Access to /AVATOR2/cid entry point
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        public async Task<Parameters> GetDefaultParameters(int cid) {
            using var req = BuildRequestMessage(HttpMethod.Get, Avatar2, $"{cid}");
            var res = await httpClient.SendAsync(req);
            var doc = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Parameters>(doc);
        }

        /// <summary>
        /// Access to /AVATOR2/cid/current entry point
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        public async Task<Parameters> GetCurrentParameter(int cid) {
            using var req = BuildRequestMessage(HttpMethod.Get, Avatar2, $"{cid}", "current");
            var res = await httpClient.SendAsync(req);
            var doc = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Parameters>(doc);
        }

        /// <summary>
        /// Access to /app/path entry point
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetStaticContent(string path) {
            using var req = BuildRequestMessage(HttpMethod.Get, App, path);
            return await httpClient.SendAsync(req);
        }

        /// <summary>
        /// Post save request
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="text"></param>
        /// <param name="file"></param>
        /// <param name="speed"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="intonation"></param>
        /// <param name="emotions"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Save(int cid, string text, string file, double speed, double volume,
            double pitch,
            double intonation, Dictionary<string, double> emotions = null) {
            using var req = BuildRequestMessage(HttpMethod.Post, Save2, $"{cid}");
            req.Content = new Content(text, speed, volume, pitch, intonation, emotions).ToStringContent();

            var res = await httpClient.SendAsync(req);

            await using (var output = File.OpenWrite(file))
            await using (var input = await res.Content.ReadAsStreamAsync()) {
                input.CopyTo(output);
            }

            return res;
        }

        /// <summary>
        /// Save to file (default parameter)
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="text"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Save(int cid, string text, string file) {
            var param = await GetDefaultParameters(cid);
            return await Save(cid, text, file, param.Effects.Speed.Value, param.Effects.Volume.Value,
                param.Effects.Pitch.Value, param.Effects.Intonation.Value, param.Emotions);
        }

        /// <summary>
        /// Post save request
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="text"></param>
        /// <param name="file"></param>
        /// <param name="samplingRate"></param>
        /// <param name="speed"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="intonation"></param>
        /// <param name="emotions"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Save(int cid, string text, string file, int samplingRate, double speed, double volume,
            double pitch,
            double intonation,
            Dictionary<string, double> emotions = null) {
            using var req = BuildRequestMessage(HttpMethod.Post, Save2, $"{cid}", $"{samplingRate}");
            req.Content = new Content(text, speed, volume, pitch, intonation, emotions).ToStringContent();

            var res = await httpClient.SendAsync(req);

            await using (var output = File.OpenWrite(file))
            await using (var input = await res.Content.ReadAsStreamAsync()) {
                input.CopyTo(output);
            }

            return res;
        }

        /// <summary>
        /// Save to file (default parameter)
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="text"></param>
        /// <param name="file"></param>
        /// <param name="samplingRate"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Save(int cid, string text, string file,int samplingRate) {
            var param = await GetDefaultParameters(cid);
            return await Save(cid, text, file, samplingRate,param.Effects.Speed.Value, param.Effects.Volume.Value, 
                param.Effects.Pitch.Value, param.Effects.Intonation.Value, param.Emotions);
        }

        /// <summary>
        /// Post talk request
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="text"></param>
        /// <param name="speed"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="intonation"></param>
        /// <param name="emotions"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Talk(int cid, string text, double speed, double volume,
            double pitch,
            double intonation,
            Dictionary<string, double> emotions = null) {
            using var req = BuildRequestMessage(HttpMethod.Post, Play2, $"{cid}");
            req.Content = new Content(text, speed, volume, pitch, intonation, emotions).ToStringContent();

            return await httpClient.SendAsync(req);
        }

        /// <summary>
        /// Post talk request (default parameters)
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Talk(int cid, string text) {
            var param = await GetDefaultParameters(cid);
            using var req = BuildRequestMessage(HttpMethod.Post, Play2, $"{cid}");
            req.Content = new Content(text, param.Effects, param.Emotions).ToStringContent();

            return await httpClient.SendAsync(req);
        }


        public void Dispose() {
            httpClient?.Dispose();
        }

        private static string JoinUrl(IEnumerable<string> segments) => string.Join("/",
            segments.SelectMany(s => s.Split('/')));
    }
}