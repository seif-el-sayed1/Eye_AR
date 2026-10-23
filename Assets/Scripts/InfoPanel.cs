
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text contentText;
    [SerializeField] TMP_Text metaText;

    GameObject panelRoot => titleText != null && titleText.transform.parent != null
        ? titleText.transform.parent.gameObject
        : null;

    void Awake()
    {
        if (panelRoot == null) return;
        var closeButton = panelRoot.transform.Find("CloseButton")?.GetComponent<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }
        panelRoot.SetActive(false);
    }

    public void Show(PartData part)
    {
        if (part == null) return;
        titleText.text = part.title;
        contentText.text = part.content;
        metaText.text = part.type == "sign"
            ? $"Sign of: {part.disease}"
            : part.source;
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }
}
