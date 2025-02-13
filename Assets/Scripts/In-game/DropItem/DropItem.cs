using System.Text.RegularExpressions;
using UnityEngine;

namespace Item
{
    // Drop Item Enums
    public enum DropItemType
    {
        Medikit,
        Magnet,
        Sight,
        Experience
    }
    public enum DropItemEffectType
    {
        Growth,
        Support,
        Attack
    }

    public abstract class DropItem : MonoBehaviour
    {
        private DropItemSO _dropItemSO;
        protected DropItemSO dropItemSO
        {
            get
            {
                return _dropItemSO;
            }
            set
            {
                _dropItemSO = value;
                if (value != null)
                {
                    dropItemType = value.dropItemType;
                    dropItemEffectType = value.dropItemEffectType;
                    dropItemName = value.dropItemName;
                    dropItemDescription = value.dropItemDescription;
                    effectValue = value.effectValue;
                    dragDistance = value.dragDistance;
                    dragSpeed = value.dragSpeed;
                    duration = value.duration;
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sprite = value.dropItemSprite;
                    }
                }
            }
        }
        protected SpriteRenderer spriteRenderer;
        public DropItemType dropItemType;
        public DropItemEffectType dropItemEffectType;
        public string dropItemName;
        public string dropItemDescription;
        // effectValue is the value of the effect, like heal amount, attack damage, sight radius, etc.
        public float effectValue;
        public float dragDistance;
        public float dragSpeed;
        // if duration is -1, it just affects once (like medikit)
        public float duration;

        // this attribute is used to get the name of the derived class
        protected string DerivedClassName => GetType().Name;
        protected string ReplaceVariables(string text)
        {
            string pattern = @"\{\{(.*?)\}\}";
            return Regex.Replace(text, pattern, match =>
            {
                string expression = match.Groups[1].Value.Trim();
                // handle multiplication
                if (expression.Contains("*"))
                {
                    string[] parts = expression.Split('*');
                    if (parts.Length == 2)
                    {
                        // return the result of the multiplication
                        float value = GetVariableValue(parts[0].Trim());
                        if (float.TryParse(parts[1], out float multiplier))
                        {
                            return (value * multiplier).ToString();
                        }
                    }
                }
                // return the value of the variable
                return GetVariableValue(expression).ToString();
            });
        }

        protected float GetVariableValue(string variableName)
        {
            return variableName switch
            {
                "duration" => duration,
                "effectValue" => effectValue,
                "dragSpeed" => dragSpeed,
                _ => 0f
            };
        }
        // item effect
        public virtual void UseItem()
        {
#if UNITY_EDITOR
            string processedDescription = ReplaceVariables(dropItemDescription);
            DebugConsole.Line itemLog = new()
            {
                text = $"[{GameManager.Instance.gameTimer} - {dropItemName}:{dropItemSO?.name}({DerivedClassName})] {processedDescription}",
                messageType = DebugConsole.MessageType.Local,
                tick = GameManager.Instance.gameTimer
            };
            DebugConsole.Instance.MergeLine(itemLog, "#00FF00");
#endif
        }

        // item drag
        public Vector3 GetPlayerPosition()
        {
            return GameManager.Instance.playerScript.transform.position;
        }
        public void DragItem(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(transform.position, targetPosition);
            float newDragDistance = dropItemType == DropItemType.Experience ? dragDistance * GameManager.Instance.dragDistanceMultiplier : dragDistance;
            float newDragSpeed = dropItemType == DropItemType.Experience ? dragSpeed * GameManager.Instance.dragSpeedMultiplier : dragSpeed;
            if (distance < newDragDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, newDragSpeed * Time.deltaTime);
            }
        }

        // unity event
        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError($"[{gameObject.name}] SpriteRenderer component is missing!");
                return;
            }

            DropItemSO loadedSO = Resources.Load<DropItemSO>($"ScriptableObjects/DropItem/{DerivedClassName}");
            if (loadedSO == null)
            {
                Debug.LogError($"[{gameObject.name}] Failed to load ScriptableObject for {DerivedClassName}!");
                return;
            }
            dropItemSO = loadedSO;
        }

        void Update()
        {
            DragItem(GetPlayerPosition());
        }
    }
}
