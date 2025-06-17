using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

[ApiController]
[Route("[controller]")]
public class AssistantController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!; // Reemplaza por tu API Key
    private readonly string _assistantId = ""; // Reemplaza por tu Assistant ID

    public AssistantController()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskAssistant([FromBody] AskRequest request)
    {


        //  string assistantMessage = "No se encontró respuesta del asistente.";

        //foreach (var msg in messages.EnumerateArray())
        //{
        //    if (msg.GetProperty("role").GetString() == "assistant")
        //    {
        //        if (msg.TryGetProperty("content", out var contentArray) &&
        //            contentArray.GetArrayLength() > 0 &&
        //            contentArray[0].TryGetProperty("text", out var text) &&
        //            text.TryGetProperty("value", out var value))
        //        {
        //            assistantMessage = value.GetString();
        //            break;
        //        }
        //    }
        //}
        
       
        var threadResponse = await _httpClient.PostAsync("https://api.openai.com/v1/threads", new StringContent("{}", Encoding.UTF8, "application/json"));
        var threadJson = await threadResponse.Content.ReadAsStringAsync();
        var threadId = JsonDocument.Parse(threadJson).RootElement.GetProperty("id").GetString();

        // 2. Agregar mensaje del usuario
        var messagePayload = new
        {
            role = "user",
            content = request.Question
        };
        var msgResponse = await _httpClient.PostAsync(
            $"https://api.openai.com/v1/threads/{threadId}/messages",
            new StringContent(JsonSerializer.Serialize(messagePayload), Encoding.UTF8, "application/json")
        );

        // 3. Ejecutar el asistente
        var runPayload = new
        {
            assistant_id = _assistantId
        };
        var runResponse = await _httpClient.PostAsync(
            $"https://api.openai.com/v1/threads/{threadId}/runs",
            new StringContent(JsonSerializer.Serialize(runPayload), Encoding.UTF8, "application/json")
        );
        var runJson = await runResponse.Content.ReadAsStringAsync();
        var runId = JsonDocument.Parse(runJson).RootElement.GetProperty("id").GetString();

        // 4. Esperar a que se complete la ejecución
        string runStatus = "queued";
        while (runStatus != "completed" && runStatus != "failed")
        {
            await Task.Delay(1000);
            var runStatusResponse = await _httpClient.GetAsync($"https://api.openai.com/v1/threads/{threadId}/runs/{runId}");
            var runStatusJson = await runStatusResponse.Content.ReadAsStringAsync();
            var runRoot = JsonDocument.Parse(runStatusJson).RootElement;
            runStatus = runRoot.GetProperty("status").GetString();
        }

        if (runStatus == "failed")
            return BadRequest("Falló la ejecución del asistente.");

        // 5. Obtener mensajes del thread
        var msgListResponse = await _httpClient.GetAsync($"https://api.openai.com/v1/threads/{threadId}/messages");
        var msgListJson = await msgListResponse.Content.ReadAsStringAsync();
        var messages = JsonDocument.Parse(msgListJson).RootElement.GetProperty("data");

        var assistantMessage = messages.EnumerateArray()
            .FirstOrDefault(m => m.GetProperty("role").GetString() == "assistant")
            .GetProperty("content")[0].GetProperty("text").GetProperty("value").GetString();
        string respuestaFormateada = assistantMessage
    .Replace("\n", Environment.NewLine)   // Para que respete saltos de línea
    .Replace("•", "• ")                   // Mejorar legibilidad de listas
    .Trim();
        return Ok(new { response = respuestaFormateada });
    }
}

public class AskRequest
{
    public string Question { get; set; }
}
