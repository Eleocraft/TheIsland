using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextTooltip : MonoBehaviour, ITooltipObject
{
    [SerializeField] private int spacing;
    public Vector2 Initialize(string Information)
    {
        TMP_Text textMesh = GetComponent<TMP_Text>();
        textMesh.text = Information;
        textMesh.ForceMeshUpdate();
        return (Vector2)textMesh.textBounds.extents * 2 + new Vector2(0, spacing);
    }
}