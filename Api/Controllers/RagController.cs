// Usings necesarios
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Domain.Servicio;
using NuGet.Common;
using Azure.Core;
using Syncfusion.Pdf.Parsing;
using System.Security.Cryptography.X509Certificates;
using Domain.Models;
using Syncfusion.Pdf;
using static System.Net.WebRequestMethods;
using Qdrant.Client;
using Qdrant.Client.Grpc;

[ApiController]
[Route("rag")]
public class RagController : ControllerBase
{
    private readonly RagService _ragService;

    public RagController(RagService ragService)
    {
        _ragService = ragService;
    }
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(PdfRequest request)
    {
       
        byte[] pdfActual = request.PdfBytes;
        string textoCompleto = "";
        using var ms = new MemoryStream(pdfActual);
        var document = new PdfLoadedDocument(ms);
        for (int i = 0; i < document.Pages.Count; i++)
        {
            PdfLoadedPage pagina = document.Pages[i] as PdfLoadedPage;

            // Extraer texto de la página
            string texto = pagina.ExtractText();
            textoCompleto += texto ;
          
        }
        await _ragService.IndexarTextoAsync(textoCompleto);
        return Ok("Embeddings subidos correctamente.");
    }
    [HttpPost("indexar")]
    public async Task<IActionResult> IndexarTexto([FromBody] IndexRequest request)
    {
        await _ragService.IndexarTextoAsync(request.Texto);
        return Ok("Texto indexado correctamente.");
    }

    [HttpPost("consultar")]
    public async Task<IActionResult> Consultar([FromBody] ConsultaRequest request)
    {
        var respuesta = await _ragService.ConsultarAsync(request.Pregunta);
        return Ok(respuesta);
    }
}

public class IndexRequest
{
    public string Texto { get; set; } = string.Empty;
}

public class ConsultaRequest
{
    public string Pregunta { get; set; } = string.Empty;
}
public class PdfRequest

{
    public byte[] PdfBytes { get; set; }

}
public class RagService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public RagService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task IndexarTextoAsync(string texto)
    {
        var chunks = SplitTexto(texto);
        var embeddings = await GenerarEmbeddings(chunks);

        for (int i = 0; i < chunks.Count; i++)
        {
            await InsertarEnQdrant(chunks[i], embeddings[i]);
        }
    }

    public async Task<string> ConsultarAsync(string pregunta)
    {
        var embedding = await GenerarEmbedding(pregunta);
        var resultados = await BuscarEnQdrant(embedding);
        var contexto = string.Join("\n", resultados);

        var respuesta = await LlamarOpenAI(pregunta, contexto);
        return respuesta;
    }

    private List<string> SplitTexto(string texto, int maxTokens = 200)
    {
        return texto.Split(".", StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
    }

    private async Task<List<float[]>> GenerarEmbeddings(List<string> textos)
    {
        var vectors = new List<float[]>();
        try
        {
            var client = _httpClientFactory.CreateClient("ConFirmaLenta");
            
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["OpenAI:ApiKey"]}");
            if (textos == null || textos.Any(t => string.IsNullOrWhiteSpace(t)))
                throw new ArgumentException("Los textos no deben estar vacíos ni contener valores nulos.");
            if (textos.Any(t => t.Length > 15000))
            {
                throw new Exception("Uno o más textos exceden el límite permitido.");
            }
            foreach (var fragmento in textos)
            {
                var requestBody = new
                {
                    input = fragmento,
                    model = "text-embedding-3-large"
                };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.openai.com/v1/embeddings", content);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error {response.StatusCode}: {error}");
                }

                var json3 = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json3);


                foreach (var item in doc.RootElement.GetProperty("data").EnumerateArray())
                {
                    var embedding = item.GetProperty("embedding").EnumerateArray().Select(x => x.GetSingle()).ToArray();
                    vectors.Add(embedding);
                }
                //return doc.RootElement.GetProperty("data")
                //    .EnumerateArray()
                //    .Select(d => d.GetProperty("embedding")
                //                  .EnumerateArray()
                //                  .Select(e => e.GetSingle())
                //                  .ToArray())
                //    .ToList();
            }
        }
        catch (Exception ex) {
            var res = ex.InnerException;
        }
    

return vectors;

       
    }

    private async Task<float[]> GenerarEmbedding(string texto)
    {
        return (await GenerarEmbeddings(new List<string> { texto }))[0];
    }

    private async Task InsertarEnQdrant(string texto, float[] embedding)
    {     
        try
        {

            var client2 = new QdrantClient(host: _configuration["Qdrant:Endpoint"] ,https: true,apiKey: _configuration["Qdrant:ApiKey"]);

    //        await client2.DeleteCollectionAsync("prueba");
    //        await client2.CreateCollectionAsync("prueba",
    //new VectorParamsMap
    //{
    //    Map =
    //    {
    //        ["text_embedding"] = new VectorParams
    //        {
    //            Size = 3072,
    //            Distance = Distance.Cosine
    //        }
    //    }
    //});

            var point = new PointStruct
            {
                Id = new PointId { Uuid = Guid.NewGuid().ToString() }, // ID del punto
                Vectors = new Vectors
                {
                    Vectors_ = new NamedVectors
                    {
                        Vectors = {
                ["text_embedding"] = new Vector
                {
                    Data = { embedding }
                }
            }
                    }
                },
                Payload = // Payload opcional (puede ser nulo)
                {
                      { "texto", texto }
                }
            };

            //List<PointStruct> points = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PointStruct>>(json);
            var operationInfo = await client2.UpsertAsync(collectionName: "prueba", points: new List<PointStruct> { point });
         


        }
        catch (Exception ex) {
            var m = ex.Message;
        }
    

    }

    private async Task<List<string>> BuscarEnQdrant(float[] embedding)
    {

        var client = new QdrantClient(host: _configuration["Qdrant:Endpoint"], https: true, apiKey: _configuration["Qdrant:ApiKey"]);
       
        List<string> lista = new List<string>();
        // Consulta con vector nombrado


        var searchResults = await client.SearchAsync(
            collectionName: "prueba",
            vectorName: "text_embedding", // vector nombrado
            vector: embedding,
            limit: 5                      // cantidad de resultados a devolver
          
              // si quieres obtener los datos guardados en payload
        );
        foreach (var point in searchResults)
        {
            Console.WriteLine($"ID: {point.Id}, Score: {point.Score}");
            if (point.Payload != null && point.Payload.TryGetValue("texto", out var texto))
            {
                Console.WriteLine($"Texto guardado: {texto}");
                lista.Add(texto.ToString());
            }
            Console.WriteLine("-----");
        }
        return lista;


    }

    private async Task<string> LlamarOpenAI(string pregunta, string contexto)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["OpenAI:ApiKey"]}");

        var request = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "system", content = "Responde usando el siguiente contexto:", },
                new { role = "user", content = contexto },
                new { role = "user", content = pregunta }
            }
        };

        var response = await client.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        );

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }
}
