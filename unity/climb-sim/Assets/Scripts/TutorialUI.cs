using UnityEngine;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject tutorialTogglePanel;
    [SerializeField] private TMP_Text tutorialText;

    private bool tutorialOpen = true;
    private int pageIndex = 0;

    private readonly string[] pages =
    {
        "<size=34><b><color=#D8E2E8>Tutorial 1/5: The Story</color></b></size>\n\n" +
        "<align=left>" +
        "<color=#8FAF96>It was supposed to be a peaceful day on Snow Creek Trail. You and your friend Alice planned to hike together, but she arrived early and ventured ahead without you...</color>\n\n" +
        "<size=22><color=#A8B4BC>Next: N     Close: T</color></size>" +
        "</align>",

        "<size=34><b><color=#D8E2E8>Tutorial 2/5: Movement</color></b></size>\n\n" +
        "<align=left>" +
        "<b><color=#6FA8CC>WASD</color></b> - Move\n" +
        "<b><color=#6FA8CC>Mouse</color></b> - Look around\n" +
        "<b><color=#6FA8CC>Shift</color></b> - Run\n" +
        "<b><color=#6FA8CC>Space</color></b> - Jump\n\n" +
        "</align>" +
        "<color=#8FAF96>Explore carefully and stay near the trail.</color>\n\n" +
        "<size=22><color=#A8B4BC>Previous: P     Next: N     Close: T</color></size>",

        "<size=34><b><color=#D8E2E8>Tutorial 3/5: Items</color></b></size>\n\n" +
        "<align=left>" +
        "<b><color=#D6B85A>E</color></b> - Interact\n" +
        "<b><color=#D6B85A>1–4</color></b> - Use quick slot items\n" +
        "<b><color=#D6B85A>R</color></b> - Resupply from your car\n" +
        "<b><color=#D6B85A>Tab</color></b> - Open inventory\n\n" +
        "</align>" +
        "<color=#B8C4CC>Use supplies wisely. Some items may help you survive hazards.</color>\n\n" +
        "<size=22><color=#A8B4BC>Previous: P     Next: N     Close: T</color></size>",

        "<size=34><b><color=#D8E2E8>Tutorial 4/5: Chat</color></b></size>\n\n" +
        "<align=left>" +
        "<b><color=#D6B85A>C</color></b> - Enter Chat Mode\n" +
        "<b><color=#D6B85A>Esc</color></b> - Pause / Leave Chat Mode\n\n" +
        "</align>" +
        "<color=#B8C4CC>Talk with Alice, the lost hiker. Ask questions to determine where to go.</color>\n\n" +
        "<size=22><color=#A8B4BC>Previous: P     Next: N     Close: T</color></size>",

        "<size=34><b><color=#D8E2E8>Tutorial 5/5: Objective</color></b></size>\n\n" +
        "<color=#B8C4CC>Guide Alice safely back to the trailhead.</color>\n\n" +
        "<color=#C96E6E>Avoid rivers, bears, and dangerous terrain.</color>\n\n" +
        "<color=#8FAF96>Reach the trailhead with Alice to win.</color>\n\n" +
        "<size=22><color=#A8B4BC>Previous: P     Close: T</color></size>"
    };
    private void Start()
    {
        SetTutorialOpen(true);
        ShowPage(0);
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsChatMode)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            SetTutorialOpen(!tutorialOpen);

            if (tutorialOpen) {
                pageIndex = 0;
                ShowPage(pageIndex);
            }
        }

        if (!tutorialOpen) return;

        if (Input.GetKeyDown(KeyCode.N))
        {
            ShowPage(pageIndex + 1);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ShowPage(pageIndex - 1);
        }
    }

    private void SetTutorialOpen(bool open)
    {
        tutorialOpen = open;

        tutorialPanel.SetActive(open);
        tutorialTogglePanel.SetActive(!open);
    }

    private void ShowPage(int newIndex)
    {
        pageIndex = Mathf.Clamp(newIndex, 0, pages.Length - 1);
        tutorialText.text = pages[pageIndex];
    }
}
