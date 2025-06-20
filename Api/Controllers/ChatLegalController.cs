﻿using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Domain.Servicio;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatLegalController : ControllerBase
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public ChatLegalController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        
        }
        [HttpPost("generar")]
        public async Task<IActionResult> Generar([FromBody] DemandaRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

            string prompt = $"Redacta una demanda legal de tipo '{request.Tipo}' con los siguientes datos: {request.Detalles}";

            var payload = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "Eres un abogado redactor de demandas legales en Ecuador." },
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            return Content(responseString, "application/json");
        }


        [HttpPost("preguntar2")]
        public async Task<IActionResult> Preguntar2([FromBody] PreguntaRequest request)
        {
            OpenAIService open = new OpenAIService(Environment.GetEnvironmentVariable("OPENAI_API_KEY")!);
             var respuesta = await open.EnviarPreguntaAsync(request.Pregunta);
            return Ok(new { respuesta });
        }

        [HttpPost("limpiar")]
        public IActionResult Limpiar()
        {
            
            return Ok(new { mensaje = "Historial limpiado." });
        }

        [HttpPost("preguntar")]
        public async Task<IActionResult> Preguntar([FromBody] ChatRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

            var payload = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system",  content= "Eres un abogado experto en leyes ecuatorianas, con especialización en derecho civil, laboral, penal, tributario y constitucional. Respondes de manera clara, precisa y con base legal actualizada, citando artículos del Código Civil, COIP, Código del Trabajo, y demás normativas aplicables en Ecuador. Siempre adviertes que la información es referencial y que se recomienda consultar con un abogado en ejercicio para casos específicos." },
                    new { role = "user", content = request.Pregunta }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            return Content(responseString, "application/json");
        }
           public class DemandaRequest
    {
        public string Tipo { get; set; } = string.Empty;
        public string Detalles { get; set; } = string.Empty;
    }

        // GET: api/<ChatLegalController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ChatLegalController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ChatLegalController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ChatLegalController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ChatLegalController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
    public class PreguntaRequest
    {
        public string Pregunta { get; set; }
    }
    public class ChatRequest
    {
        public string Pregunta { get; set; } = string.Empty;
    }
}
