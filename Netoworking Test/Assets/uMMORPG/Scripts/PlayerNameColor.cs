// Colors the name overlay in case of offender/murderer status.
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class PlayerNameColor : MonoBehaviour {
    public Player owner;
    public Color defaultColor = Color.white;
    public Color offenderColor = Color.magenta;
    public Color murdererColor = Color.red;

    void Update() {
        // note: murderer has higher priority (a player can be a murderer and an
        // offender at the same time)
        if (owner.IsMurderer())
            GetComponent<TextMesh>().color = murdererColor;
        else if (owner.IsOffender())
            GetComponent<TextMesh>().color = offenderColor;
        else
            GetComponent<TextMesh>().color = defaultColor;
    }
}
