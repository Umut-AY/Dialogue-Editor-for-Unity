// DialogueEditorWindow.cs - Unity Editor
// Assets/Editor klasörüne yerleştirilmelidir

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

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
    public List<DialogueChoice> choices = new();
    public bool isStartNode;
}

[System.Serializable]
public class DialogueData
{
    public List<DialogueNodeData> nodes = new();
}

public class DialogueEditorWindow : EditorWindow
{
    private List<DialogueNode> nodes = new();
    public DialogueNode selectedOutputNode, selectedNode;
    private Vector2 drag, offset, mousePos;

    [MenuItem("Tools/Dialogue Editor")]
    public static void OpenWindow()
    {
        GetWindow<DialogueEditorWindow>("Dialogue Editor");
    }

    private void OnGUI()
    {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawConnections();
        DrawNodes();
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();

        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Save", EditorStyles.toolbarButton)) SaveToJSON();
        if (GUILayout.Button("Load", EditorStyles.toolbarButton)) LoadFromJSON();
        GUILayout.EndHorizontal();
    }

    private void DrawGrid(float spacing, float opacity, Color color)
    {
        int widthDivs = Mathf.CeilToInt(position.width / spacing);
        int heightDivs = Mathf.CeilToInt(position.height / spacing);
        Handles.BeginGUI();
        Handles.color = new Color(color.r, color.g, color.b, opacity);
        offset += drag * 0.5f;
        Vector3 newOffset = new(offset.x % spacing, offset.y % spacing);

        for (int i = 0; i < widthDivs; i++)
            Handles.DrawLine(new Vector3(spacing * i, 0, 0) + newOffset, new Vector3(spacing * i, position.height, 0f) + newOffset);
        for (int j = 0; j < heightDivs; j++)
            Handles.DrawLine(new Vector3(0, spacing * j, 0) + newOffset, new Vector3(position.width, spacing * j, 0f) + newOffset);

        Handles.EndGUI();
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;
        mousePos = e.mousePosition;

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            selectedNode = GetNodeAtPoint(mousePos);
            ShowContextMenu();
        }
        else if (e.type == EventType.MouseDown && e.button == 0)
        {
            selectedNode = GetNodeAtPoint(mousePos);
        }
        else if (e.type == EventType.MouseDrag && e.button == 0 && selectedNode == null)
        {
            foreach (DialogueNode node in nodes)
            {
                node.Drag(e.delta);
            }
            drag = e.delta;
            GUI.changed = true;
        }
        else if (e.type == EventType.MouseDrag && e.button == 0 && selectedNode != null)
        {
            selectedNode.Drag(e.delta);
            GUI.changed = true;
        }
    }

    private DialogueNode GetNodeAtPoint(Vector2 point)
    {
        foreach (var node in nodes)
            if (node.rect.Contains(point)) return node;
        return null;
    }

    private void ShowContextMenu()
    {
        GenericMenu menu = new();
        menu.AddItem(new GUIContent("Add Start Node"), false, () => OnClickAddNode(mousePos, true));
        menu.AddItem(new GUIContent("Add Dialogue Node"), false, () => OnClickAddNode(mousePos, false));

        if (selectedNode != null)
        {
            menu.AddItem(new GUIContent("Set as Output"), false, () => selectedOutputNode = selectedNode);
            if (selectedOutputNode != null && selectedNode != selectedOutputNode)
                menu.AddItem(new GUIContent("Link To"), false, () => selectedOutputNode.AddChild(selectedNode));
            menu.AddItem(new GUIContent("Delete"), false, () => DeleteNode(selectedNode));
        }

        menu.ShowAsContext();
    }

    private void OnClickAddNode(Vector2 position, bool isStart)
    {
        DialogueNode node = new(position, isStart);
        nodes.Add(node);
    }

    private void DeleteNode(DialogueNode node)
    {
        nodes.Remove(node);
        foreach (var n in nodes) n.children.Remove(node);
    }

    private void DrawNodes()
    {
        foreach (var node in nodes) node.Draw(this);
    }

    private void DrawConnections()
    {
        foreach (var node in nodes)
        {
            for (int i = 0; i < node.children.Count; i++)
            {
                DialogueNode child = node.children[i];
                DialogueChoice choice = node.choices[i];

                Vector3 start = new Vector3(node.rect.xMax, node.rect.center.y + i * 15);
                Vector3 end = new Vector3(child.rect.xMin, child.rect.center.y);
                Vector3 startTangent = start + Vector3.right * 50;
                Vector3 endTangent = end + Vector3.left * 50;

                // Puan rengine göre çizgi
                Color color = Color.white;
                if (choice.requiredRelation >= 50) color = Color.green;
                else if (choice.requiredRelation >= 20) color = Color.yellow;
                else color = Color.red;

                Handles.DrawBezier(start, end, startTangent, endTangent, color, null, 3f);

                // Ortaya puan etiketi
                Vector2 midPoint = (start + end) * 0.5f;
                Handles.Label(midPoint, $"Puan: {choice.requiredRelation}", EditorStyles.whiteLabel);
            }
        }
    }


    private void SaveToJSON()
    {
        DialogueData data = new();
        foreach (var node in nodes)
            data.nodes.Add(node.ToData(nodes));
        File.WriteAllText(Application.dataPath + "/dialogue.json", JsonUtility.ToJson(data, true));
        Debug.Log("Saved.");
    }

    private void LoadFromJSON()
    {
        string path = Application.dataPath + "/dialogue.json";
        if (!File.Exists(path)) return;

        DialogueData data = JsonUtility.FromJson<DialogueData>(File.ReadAllText(path));
        nodes.Clear();

        foreach (var nodeData in data.nodes)
        {
            DialogueNode node = new(nodeData.position, nodeData.isStartNode);
            node.FromData(nodeData);
            nodes.Add(node);
        }

        for (int i = 0; i < data.nodes.Count; i++)
        {
            foreach (var choice in data.nodes[i].choices)
            {
                if (choice.targetNodeIndex >= 0 && choice.targetNodeIndex < nodes.Count)
                    nodes[i].children.Add(nodes[choice.targetNodeIndex]);
            }
        }
    }

    public class DialogueNode
    {
        public Rect rect;
        public string speakerName = "Konuşmacı";
        public string questionText = "Soru";
        public string dialogueText = "Cevap";
        public bool isStartNode = false;
        public List<DialogueChoice> choices = new();
        public List<DialogueNode> children = new();
        private GUIStyle style;

        public DialogueNode(Vector2 pos, bool isStart)
        {
            rect = new Rect(pos.x, pos.y, 300, isStart ? 130 : 230);
            isStartNode = isStart;
        }

        public void Drag(Vector2 delta) => rect.position += delta;

        public void Draw(DialogueEditorWindow window)
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.box);
                style.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
                style.border = new RectOffset(12, 12, 12, 12);
            }

            GUI.BeginGroup(rect, style);
            float y = 10;

            GUI.Label(new Rect(10, y, 280, 20), "Konuşmacı:");
            speakerName = GUI.TextField(new Rect(10, y + 20, 280, 20), speakerName);
            y += 50;

            if (!isStartNode)
            {
                GUI.Label(new Rect(10, y, 280, 20), "Soru:");
                questionText = GUI.TextArea(new Rect(10, y + 20, 280, 40), questionText);
                y += 65;
            }

            GUI.Label(new Rect(10, y, 280, 20), "Cevap:");
            dialogueText = GUI.TextArea(new Rect(10, y + 20, 280, 40), dialogueText);
            y += 65;

            for (int i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                GUI.Label(new Rect(10, y, 60, 20), $"Seçenek {i + 1}:");
                choice.questionText = GUI.TextField(new Rect(75, y, 140, 20), choice.questionText);
                choice.requiredRelation = EditorGUI.IntField(new Rect(220, y, 40, 20), choice.requiredRelation);
                if (GUI.Button(new Rect(265, y, 20, 20), "x"))
                {
                    children.RemoveAt(i);
                    choices.RemoveAt(i);
                    break;
                }
                y += 30;
            }

            if (GUI.Button(new Rect(rect.width - 25, rect.height - 25, 20, 20), "+"))
                window.selectedOutputNode = this;

            if (GUI.Button(new Rect(5, rect.height - 25, 20, 20), ">"))
            {
                if (window.selectedOutputNode != null && window.selectedOutputNode != this)
                {
                    window.selectedOutputNode.AddChild(this);
                }
            }

            // Dinamik yükseklik ayarlama
            rect.height = y + 35;
            GUI.EndGroup();
        }

        public void AddChild(DialogueNode target)
        {
            if (!children.Contains(target))
            {
                children.Add(target);
                choices.Add(new DialogueChoice
                {
                    questionText = "Yeni Soru",
                    targetNodeIndex = -1,
                    requiredRelation = 0
                });
            }
        }

        public DialogueNodeData ToData(List<DialogueNode> all)
        {
            DialogueNodeData data = new()
            {
                speakerName = speakerName,
                questionText = questionText,
                dialogueText = dialogueText,
                position = rect.position,
                isStartNode = isStartNode
            };

            for (int i = 0; i < children.Count; i++)
            {
                DialogueChoice choice = choices[i];
                choice.targetNodeIndex = all.IndexOf(children[i]);
                data.choices.Add(choice);
            }

            return data;
        }

        public void FromData(DialogueNodeData data)
        {
            speakerName = data.speakerName;
            questionText = data.questionText;
            dialogueText = data.dialogueText;
            isStartNode = data.isStartNode;
            choices = data.choices;
        }
    }
}
