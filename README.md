Dialogue Editor for Unity
This is a custom Node-based Dialogue Editor built entirely inside the Unity Editor. It allows writers and designers to create interactive conversations with multiple-choice questions, branching paths, and relation-based conditions without touching any code.

✨ Features
🗨️ Speaker, Question, and Answer fields per node

🔗 Connect nodes visually via input/output links

❓ Multiple choice system – each answer can lead to a different node

💖 Relation Score: Each choice can be restricted based on a required "relation" value

🎨 Clean and resizable node interface, nodes expand dynamically as content grows

🖱️ Context menu for creating, deleting, and linking nodes

📂 Save & Load dialogues as JSON files

🎮 Runtime UI Player with button-based branching (uses Unity UI)

🧩 How to Use
Place all scripts inside Assets/Editor/.

Open the editor via Tools > Dialogue Editor.

Right-click to create Start or Dialogue Nodes.

Fill in the speaker, question, and answer texts.

Use the + button to add multiple options.

Link options to other nodes using the > button.

Assign a relation score for each option if needed.

Save your dialogue as a JSON file.

Use the DialogueUIPlayer script in runtime to load and display dialogues with branching buttons.

🛠 Runtime Integration
Add DialogueUIPlayer to your scene.

Assign a saved .json file from your editor output.

Connect your UI (Text + Button Prefab + Container).

It will automatically show choices and respect relation limits.

📷 Screenshot

A full dialogue tree with branching questions and relation-based conditions.
