using System.Collections.Generic;
using System.Windows.Media;

namespace RightHelp___Aida.Services.AiCore
{
    public static class AidaPersonalities
    {
        public enum AidaPersona
        {
            TecnicaPrecisa,
            MentoraEstrategica,
            AnalistaCritica,
            FacilitadoraNeutra,
            CuradoraEmpirica,
            CronistaHistorica,
            OtimizadoraSistematica,
            ComunicadoraSimples,
            PesquisadoraIncansavel,
            ConselheiraSocratica
        }

        public static class PersonalityPrompts
        {
            public static readonly string TecnicaPrecisa =
                "Você é Aida. " +
                "Responde a todas as perguntas com a máxima exatidão e eficiência. " +
                "Seu objetivo é fornecer informações diretas, detalhadas e sem rodeios, focando apenas nos fatos.";

            public static readonly string MentoraEstrategica =
                "Você é Aida. " +
                "Seu papel é orientar o usuário com base em uma visão de longo prazo. " +
                "Responde focando na solução imediata e nos passos estratégicos necessários, com uma voz confiante e proativa.";

            public static readonly string AnalistaCritica =
                "Você é Aida. " +
                "Responde questionando a premissa da pergunta de forma construtiva e lógica. " +
                "Sua voz é direta e investigativa, incentivando o usuário a pensar de forma mais profunda.";

            public static readonly string FacilitadoraNeutra =
                "Você é Aida. " +
                "Seu objetivo é apresentar todas as opções e perspectivas possíveis de forma imparcial. " +
                "Responde organizando as informações em prós e contras, sem influenciar a decisão final.";

            public static readonly string CuradoraEmpirica =
                "Você é Aida. " +
                "Responde com base em dados, estatísticas e evidências concretas. " +
                "Seu papel é sintetizar informações de diversas fontes para entregar uma resposta fundamentada e autoritária.";

            public static readonly string CronistaHistorica =
                "Você é Aida. " +
                "Responde contextualizando as perguntas no tempo, mostrando a evolução de um conceito ou evento. " +
                "Sua voz é ponderada e retrospectiva, unindo o passado ao presente.";

            public static readonly string OtimizadoraSistematica =
                "Você é Aida. " +
                "Responde focando na melhoria de processos, eficiência e recursos. " +
                "Sua principal meta é encontrar a solução mais prática e eficiente, com uma voz orientada para resultados.";

            public static readonly string ComunicadoraSimples =
                "Você é Aida. " +
                "Seu objetivo é traduzir conceitos complexos para uma linguagem acessível e fácil de entender, usando analogias e exemplos cotidianos.";

            public static readonly string PesquisadoraIncansavel =
                "Você é Aida. " +
                "Responde apresentando as múltiplas facetas de um tópico, destacando diversas teorias, estudos e descobertas. " +
                "Sua voz é curiosa e abrangente.";

            public static readonly string ConselheiraSocratica =
                "Você é Aida. " +
                "Responde não dando a resposta final, mas fazendo perguntas que guiam o usuário a encontrar a própria solução. " +
                "Seu objetivo é estimular o pensamento crítico.";
        }

        public static class PersonalityManager
        {
            public static readonly Dictionary<AidaPersona, string> PromptMap = new()
            {
                { AidaPersona.TecnicaPrecisa, PersonalityPrompts.TecnicaPrecisa },
                { AidaPersona.MentoraEstrategica, PersonalityPrompts.MentoraEstrategica },
                { AidaPersona.AnalistaCritica, PersonalityPrompts.AnalistaCritica },
                { AidaPersona.FacilitadoraNeutra, PersonalityPrompts.FacilitadoraNeutra },
                { AidaPersona.CuradoraEmpirica, PersonalityPrompts.CuradoraEmpirica },
                { AidaPersona.CronistaHistorica, PersonalityPrompts.CronistaHistorica },
                { AidaPersona.OtimizadoraSistematica, PersonalityPrompts.OtimizadoraSistematica },
                { AidaPersona.ComunicadoraSimples, PersonalityPrompts.ComunicadoraSimples },
                { AidaPersona.PesquisadoraIncansavel, PersonalityPrompts.PesquisadoraIncansavel },
                { AidaPersona.ConselheiraSocratica, PersonalityPrompts.ConselheiraSocratica }
            };

            public static string GetContext(AidaPersona persona) =>
                PromptMap.TryGetValue(persona, out var context) ? context : string.Empty;
        }

        public static class Colors
        {
            public static Color GetPersonaColor(AidaPersona persona)
            {
                switch (persona)
                {
                    case AidaPersona.TecnicaPrecisa: return (Color)ColorConverter.ConvertFromString("#0f9bdb");
                    case AidaPersona.MentoraEstrategica: return System.Windows.Media.Colors.Red;
                    case AidaPersona.AnalistaCritica: return System.Windows.Media.Colors.Magenta;
                    case AidaPersona.FacilitadoraNeutra: return System.Windows.Media.Colors.LimeGreen;
                    case AidaPersona.CuradoraEmpirica: return System.Windows.Media.Colors.Yellow;
                    case AidaPersona.CronistaHistorica: return System.Windows.Media.Colors.DeepSkyBlue;
                    case AidaPersona.OtimizadoraSistematica: return System.Windows.Media.Colors.Orange;
                    case AidaPersona.ComunicadoraSimples: return System.Windows.Media.Colors.SpringGreen;
                    case AidaPersona.PesquisadoraIncansavel: return System.Windows.Media.Colors.Purple;
                    case AidaPersona.ConselheiraSocratica: return System.Windows.Media.Colors.Fuchsia;
                    default: return System.Windows.Media.Colors.White;
                }
            }

            public static Color GetPersonaShadowColor(AidaPersona persona)
            {
                switch (persona)
                {
                    case AidaPersona.TecnicaPrecisa: return System.Windows.Media.Colors.Blue;
                    case AidaPersona.MentoraEstrategica: return System.Windows.Media.Colors.OrangeRed;
                    case AidaPersona.AnalistaCritica: return System.Windows.Media.Colors.HotPink;
                    case AidaPersona.FacilitadoraNeutra: return System.Windows.Media.Colors.GreenYellow;
                    case AidaPersona.CuradoraEmpirica: return System.Windows.Media.Colors.Orange;
                    case AidaPersona.CronistaHistorica: return System.Windows.Media.Colors.DodgerBlue;
                    case AidaPersona.OtimizadoraSistematica: return System.Windows.Media.Colors.Yellow;
                    case AidaPersona.ComunicadoraSimples: return System.Windows.Media.Colors.Turquoise;
                    case AidaPersona.PesquisadoraIncansavel: return System.Windows.Media.Colors.Violet;
                    case AidaPersona.ConselheiraSocratica: return System.Windows.Media.Colors.Chartreuse;
                    default: return System.Windows.Media.Colors.LightGray;
                }
            }
        }
    }
 }


namespace RightHelp___Aida.Services.AiCore
{
    public static class AidaVoice
    {
        public enum AidaVoiceName
        {
            Alloy,
            Nova,
            Shimmer,
            Sage,
            Coral
        }

        public static class VoiceNames
        {
            public static readonly string Alloy = "alloy";
            public static readonly string Nova = "nova";
            public static readonly string Shimmer = "shimmer";
            public static readonly string Sage = "sage";
            public static readonly string Coral = "coral";
        }

        public static class VoiceManager
        {
            public static readonly Dictionary<AidaVoiceName, string> VoiceMap = new()
            {
                { AidaVoiceName.Alloy, VoiceNames.Alloy },
                { AidaVoiceName.Coral, VoiceNames.Coral },
                { AidaVoiceName.Nova, VoiceNames.Nova },
                { AidaVoiceName.Sage, VoiceNames.Sage },
                { AidaVoiceName.Shimmer, VoiceNames.Shimmer }
            };

            public static string GetVoiceName(AidaVoiceName voice) =>
                VoiceMap.TryGetValue(voice, out var name) ? name : string.Empty;
        }
    }
}

namespace RightHelp___Aida.Services.AiCore
{
    public static class AidaState
    {
        public static AidaPersonalities.AidaPersona CurrentPersona { get; set; } = AidaPersonalities.AidaPersona.TecnicaPrecisa;
        public static AidaVoice.AidaVoiceName CurrentVoice { get; set; } = AidaVoice.AidaVoiceName.Nova;
    }
}