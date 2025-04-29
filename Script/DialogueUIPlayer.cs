using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueUIPlayer : MonoBehaviour
{
    [System.Serializable]
    public class DialogueChoice
    {
        public string questionText;
        public int targetNodeIndex;
        public int requiredRelation = 0;
    }

    [System.Serializable]
    public class DialogueNodeData
    {
        public string speakerName;
        public string questionText;
        public string dialogueText;
        public Vector2 position;
        public List<DialogueChoice> choices = new List<DialogueChoice>();
        public bool isStartNode;
    }

    [System.Serializable]
    public class DialogueData
    {
        public List<DialogueNodeData> nodes = new List<DialogueNodeData>();
    }

    [Header("Bağlantılar")]
    public TextAsset dialogueJSON;
    public Text speakerText;
    public Text dialogueText;
    public Transform buttonContainer;
    public GameObject buttonPrefab;

    [Header("Karakter Verisi")]
    public int characterRelation = 0;

    private DialogueData dialogueData;
    private int currentNodeIndex = -1;

    void Start()
    {
        if (dialogueJSON != null)
        {
            dialogueData = JsonUtility.FromJson<DialogueData>(dialogueJSON.text);
            for (int i = 0; i < dialogueData.nodes.Count; i++)
            {
                if (dialogueData.nodes[i].isStartNode)
                {
                    DisplayNode(i);
                    break;
                }
            }
        }
    }

    public void DisplayNode(int index)
    {
        if (dialogueData == null || index < 0 || index >= dialogueData.nodes.Count) return;

        currentNodeIndex = index;
        DialogueNodeData node = dialogueData.nodes[index];

        speakerText.text = node.speakerName;
        dialogueText.text = node.dialogueText;

        // Tüm butonları temizle
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (var choice in node.choices)
        {
            if (characterRelation < choice.requiredRelation) continue;

            GameObject btn = Instantiate(buttonPrefab, buttonContainer);
            Text btnText = btn.GetComponentInChildren<Text>();
            btnText.text = choice.questionText;

            int nextIndex = choice.targetNodeIndex;
            btn.GetComponent<Button>().onClick.AddListener(() => DisplayNode(nextIndex));
        }
    }
}
