namespace MentalHealthSupport.Services;

/// <summary>
/// Mensagens constantes para suporte de IA quando a integração não está configurada ou há erros.
/// </summary>
public static class AiSupportMessages
{
    /// <summary>
    /// Mensagem fornecida quando a integração com OpenAI não está configurada.
    /// </summary>
    public const string NotConfiguredFallback =
        "Obrigado por compartilhar o que você está sentindo.\n\n" +
        "No momento, a integração com a IA ainda não está configurada neste ambiente, " +
        "mas aqui vão algumas orientações gerais:\n" +
        "- Procure alguém de confiança para conversar sobre o que você está passando.\n" +
        "- Se estiver em sofrimento intenso ou com pensamentos de autoagressão, " +
        "busque imediatamente um serviço de emergência ou um profissional de saúde mental.\n\n" +
        "Quando a integração com a IA for configurada, esta resposta será gerada de forma personalizada " +
        "com base na sua mensagem.";

    /// <summary>
    /// Mensagem fornecida quando há erro ao chamar o serviço de IA.
    /// </summary>
    public const string ServiceErrorFallback =
        "Não foi possível falar com a IA agora. Isso pode ser algo temporário do serviço externo.\n\n" +
        "Se estiver em sofrimento intenso, por favor procure um profissional de saúde mental " +
        "ou um serviço de emergência na sua região.";

    /// <summary>
    /// Mensagem fornecida quando a IA não consegue gerar uma resposta válida.
    /// </summary>
    public const string NoResponseFallback =
        "A IA não conseguiu gerar uma resposta no momento. Tente novamente em alguns instantes.";

    /// <summary>
    /// Prompt padrão do sistema quando não configurado.
    /// </summary>
    public const string DefaultSystemPrompt =
        "Você é um assistente de apoio emocional em língua portuguesa. " +
        "Ofereça acolhimento, valide sentimentos e sugira caminhos saudáveis, " +
        "sempre reforçando que sua resposta não substitui acompanhamento profissional. " +
        "Se a pessoa mencionar risco à própria vida ou à de terceiros, " +
        "oriente a procurar imediatamente serviços de emergência da região.";
}
