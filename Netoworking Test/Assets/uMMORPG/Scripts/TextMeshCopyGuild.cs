// Copies a GameObject's guild to the TextMesh's text. We use the Update method
// because some GameObjects may change their guild during the game, like players
// renaming themself etc.
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class TextMeshCopyGuild : MonoBehaviour {
    public Player source;
    public string prefix = "[";
    public string suffix = "]";

    void Update() {
        GetComponent<TextMesh>().text = source.guild != "" ? prefix + source.guild + suffix : "";
    }
}
