using System.Linq;
using UnityEngine;


// ⚙️🔧🛠️🎮🕹️📦📁📜🧩✨⭐🔹🔸🔥⚡🧪🔍🎯💡🚀🐞🔒🔓📌📍🧱📊🧭

public class AddEmojiAttribute : PropertyAttribute
{
    public string[] Emojis { get; private set; }

    public AddEmojiAttribute(params string[] emojis)
    {
        // Allow "⭐🔹" as one string OR separate strings
        if (emojis.Length == 1)
            Emojis = emojis[0].ToCharArray()
                .Select(c => c.ToString())
                .ToArray();
        else
            Emojis = emojis;
    }
}