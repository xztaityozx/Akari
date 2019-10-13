using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SeikaCenterClient {
    public class Content {
        [JsonPropertyName("talktext")]
        public string TalkText { get; set; }
        [JsonPropertyName("effects")] public Dictionary<string,double> Effects { get; set; }
        [JsonPropertyName("emotions")] public Dictionary<string,double> Emotions { get; set; }

        public Content(string text, double speed, double volume, double pitch, double intonation, Dictionary<string, double> emotions) {
            TalkText = text;
            Effects = new Dictionary<string, double> {
                [nameof(speed)] = speed, [nameof(volume)] = volume, 
                [nameof(pitch)] = pitch, [nameof(intonation)] = intonation
            };
            Emotions = emotions;
        }

        public Content(string text, Effects effects, Dictionary<string, double> emotions) : 
            this(text,effects.Speed.Value, effects.Volume.Value, effects.Pitch.Value, effects.Intonation.Value, emotions) { }


        public Content(string text): this(text, 1.0, 1.0, 1.0, 1.0, null) { }

        public StringContent ToStringContent() {
            return new StringContent(JsonSerializer.Serialize(this), Encoding.UTF8, @"application/json");
        }
    }

    public class Effects {
        [JsonPropertyName("volume")] public Parameter Volume { get; set; }
        [JsonPropertyName("speed")] public Parameter Speed { get; set; }
        [JsonPropertyName("pitch")] public Parameter Pitch { get; set; }
        [JsonPropertyName("intonation")] public Parameter Intonation { get; set; }

        public override string ToString() {
            return $"Volume:{Volume}, Speed:{Speed}, Pitch:{Pitch}, Intonation:{Intonation}";
        }
    }

    public class Parameter {
        [JsonPropertyName("value")] public double Value { get; set; }
        [JsonPropertyName("min")] public double Min { get; set; }
        [JsonPropertyName("max")] public double Max { get; set; }
        [JsonPropertyName("step")] public double Step { get; set; }

        public override string ToString() {
            return $"Value:{Value}, Min:{Min}, Max:{Max}, Step:{Step}";
        }
    }

    public class Parameters {
        [JsonPropertyName("effect")] public Effects Effects { get; set; }
        [JsonPropertyName("emotion")] public Dictionary<string,double> Emotions { get; set; }

        public override string ToString() {
            return $"Effects:{Effects}\nEmotion:{Emotions}";
        }
    }

}