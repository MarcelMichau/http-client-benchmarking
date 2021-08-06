using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace HttpClientBenchmarks
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<HttpClientBenchmarks>();
        }
    }

    [MemoryDiagnoser]
    public class HttpClientBenchmarks
    {
        private HttpClient _httpClient;

        [GlobalSetup]
        public void Setup()
        {
            _httpClient = new HttpClient();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _httpClient.Dispose();
        }

        [Benchmark]
        public async Task WithoutHttpCompletionOption()
        {
            var response = await _httpClient.GetAsync("https://localhost:5001/weatherforecast/many");

            response.EnsureSuccessStatusCode();

            if (response.Content is object)
            {
                var stream = await response.Content.ReadAsStreamAsync();

                var data = await JsonSerializer.DeserializeAsync<List<WeatherForecast>>(stream);
            }
        }

        [Benchmark]
        public async Task WithHttpCompletionOption()
        {
            using var response = await _httpClient.GetAsync("https://localhost:5001/weatherforecast/many",
                HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            if (response.Content is object)
            {
                var stream = await response.Content.ReadAsStreamAsync();

                var data = await JsonSerializer.DeserializeAsync<List<WeatherForecast>>(stream);
            }
        }
        
        [Benchmark]
        public async Task WithGetStringAsync()
        {
            var content = await _httpClient.GetStringAsync("https://localhost:5001/weatherforecast/many");

            var data = JsonSerializer.Deserialize<List<WeatherForecast>>(content);
        }

        [Benchmark]
        public async Task WithGetStreamAsync()
        {
            await using var stream = await _httpClient.GetStreamAsync("https://localhost:5001/weatherforecast/many");

            var data = await JsonSerializer.DeserializeAsync<List<WeatherForecast>>(stream);
        }
        
        [Benchmark]
        public async Task WithGetFromJsonAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<WeatherForecast>>("https://localhost:5001/weatherforecast/many");
        }
    }
}