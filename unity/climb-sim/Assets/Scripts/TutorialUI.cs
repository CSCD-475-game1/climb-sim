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
        "<size=34><b>Tutorial 1/5: The Story</b></size>\n\n" + 
        "<align=left>" + "<color=#B6FFB0>What a beautiful day for hike at the Snow Creek Trail. You and your friend Alice decided to hike the trail. She arrived early and started without you...</color>\n\n" + 
        "<size=22>Next: N Close: T</size>",

        "<size=34><b>Tutorial 2/5: Movement</b></size>\n\n" +
        "<align=left>" + 
        "<b><color=#88C9FF>WASD</color></b> - Move\n" +
        "<b><color=#88C9FF>Mouse</color></b> - Look around\n" +
        "<b><color=#88C9FF>Shift</color></b> - Run\n" +
        "<b><color=#88C9FF>Space</color></b> - Jump\n\n" +
        "</align>" +
        "<color=#B6FFB0>Explore carefully and stay near the trail.</color>\n\n" +
        "<size=22>Previous: P     Next: N     Close: T</size>",

        "<size=34><b>Tutorial 3/5: Items</b></size>\n\n" +
        "<align=left>" + 
        "<b><color=#FFD966>E</color></b> Interact\n" +
        "<b><color=#FFD966># 1-4</color></b> Use items in quick slots\n" +
        "<b><color=#FFD966>R</color></b> Resupply from your car.\n" +
        "<b><color=#FFD966>Tab</color></b>  Open inventory\n\n" +
        "</align>" +
        "Use supplies when needed. Some items may help you survive hazards.\n\n" +
        "<size=22>Previous: P     Next: N     Close: T</size>",


        "<size=34><b>Tutorial 4/5: Chat</b></size>\n\n" +
        "<align=left>" + 
        "<b><color=#FFD966>C</color></b> Enter Chat Mode\n" +
        "<b><color=#FFD966>Esc</color></b> Pause / Leave Chat Mode\n\n" +
        "</align>" +
        "Chat with Alice the lost hiker. Ask questions to determine where to go.\n\n" +
        "<size=22>Previous: P     Next: N     Close: T</size>",

        "<size=34><b>Tutorial 5/5: Objective</b></size>\n\n" +
        "Guide Alice safely back to the trailhead.\n\n" +
        "<color=#FF8080>Avoid rivers, bears, and dangerous terrain.</color>\n\n" +
        "<color=#B6FFB0>Reach the trailhead with Alice to win.</color>\n\n" +
        "<size=22>Previous: P     Close: T</size>"
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
