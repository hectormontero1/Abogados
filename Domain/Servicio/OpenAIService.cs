
using OpenAI.Chat;
using OpenAI;


namespace Domain.Servicio
{
    public class OpenAIService
    {
        private readonly OpenAIClient _client;
        private readonly List<Message> _messages;

        public OpenAIService(string apiKey)
        {
            _client = new OpenAIClient(new OpenAIAuthentication(apiKey));
            _messages = new List<Message>
            {
            new(Role.System, "Eres un asistente legal, médico y técnico que responde en español.")
        };
        }

        public async Task<string> EnviarPreguntaAsync(string pregunta)
        {
            _messages.Add(new Message(Role.User, pregunta));

            var chatRequest = new ChatRequest(_messages, model: "gpt-4-turbo");

            var response = await _client.ChatEndpoint.GetCompletionAsync(chatRequest);

            var respuestaTexto = response.FirstChoice.Message.Content;

            _messages.Add(new Message(Role.Assistant, respuestaTexto));

            return respuestaTexto;
        }

        public void LimpiarHistorial()
        {
            _messages.Clear();
            _messages.Add(new Message(Role.System, "Eres un asistente legal, médico y técnico que responde en español."));
        }
    }
}
