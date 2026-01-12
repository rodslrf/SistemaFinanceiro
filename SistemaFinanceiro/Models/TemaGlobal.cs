using System.Drawing;

namespace SistemaFinanceiro
{
    public static class TemaGlobal
    {
        public static bool ModoEscuro { get; set; } = true;

        public static Color CorFundo => ModoEscuro ? ColorTranslator.FromHtml("#0d1117") : Color.White;
        public static Color CorSidebar => ModoEscuro ? ColorTranslator.FromHtml("#161b22") : ColorTranslator.FromHtml("#f0f0f0");
        public static Color CorTexto => ModoEscuro ? ColorTranslator.FromHtml("#c9d1d9") : Color.Black;
        public static Color CorBorda => ModoEscuro ? ColorTranslator.FromHtml("#30363d") : Color.LightGray;
        public static Color CorBotaoHover => ModoEscuro ? ColorTranslator.FromHtml("#21262d") : Color.LightGray;
        public static Color CorBotaoAtivo => ModoEscuro ? ColorTranslator.FromHtml("#30363d") : Color.DarkGray;
    }
}