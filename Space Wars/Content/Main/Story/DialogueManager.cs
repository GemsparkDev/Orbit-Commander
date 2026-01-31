using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.Story;
public class DialogueManager
{
    private List<Dialogue> dialogues = [];
    public int QueuedDialogues { get { return dialogues.Count; } }
    public void Clear()
    {
        dialogues = [];
    }
    public void Update()
    {
        if (dialogues.Count <= 0)
        {
            return;
        }
        dialogues[0].Update();
        if (dialogues[0].IsComplete())
        {
            dialogues.RemoveAt(0);
        }
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        if (dialogues.Count <= 0)
        {
            return;
        }
        dialogues[0].Draw(_spriteBatch);
    }
    public void Add(Dialogue _dialogue)
    {
        if (_dialogue == null)
        {
            throw new ArgumentNullException(nameof(_dialogue), "Dialogue was null");
        }
        dialogues.Add(_dialogue);
    }
}
