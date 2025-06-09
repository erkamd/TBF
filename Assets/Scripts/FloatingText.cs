using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    private float duration = 1f;
    private float timer = 0f;

    public static void Create(string message, Vector3 position, float time = 1f)
    {
        var obj = new GameObject("FloatingText");
        obj.transform.position = position + new Vector3(0, 0, -0.1f);
        var text = obj.AddComponent<TextMeshPro>();
        text.text = message;
        text.fontSize = 3;
        text.alignment = TextAlignmentOptions.Center;
        var ft = obj.AddComponent<FloatingText>();
        ft.duration = time;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}
