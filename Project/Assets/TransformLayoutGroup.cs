using UnityEngine;
using UnityEditor;

public class TransformLayoutGroup : MonoBehaviour
{
    public enum LayoutDirection { Horizontal, Vertical }

    public LayoutDirection layout = LayoutDirection.Horizontal;
    public float spacing = 1.0f;

    public void ApplyLayout()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 pos = Vector3.zero;

            switch (layout)
            {
                case LayoutDirection.Horizontal:
                    pos = new Vector3(i * spacing, 0, 0);
                    break;
                case LayoutDirection.Vertical:
                    pos = new Vector3(0, -i * spacing, 0);
                    break;
            }

            transform.GetChild(i).localPosition = pos;
        }
    }

    private void OnValidate()
    {
        ApplyLayout();
    }
}





