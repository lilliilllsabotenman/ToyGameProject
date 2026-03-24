using UnityEngine;
using UnityEngine.UI;

public class ItemSrot : MonoBehaviour
{
    [SerializeField] private ItemType iType;
    private ItemColor itemColor;

    private void Start()
    {
        Image _image = this.GetComponent<Image>();

        itemColor = new ItemColor(_image);
    }

    public void SetColor(Color _color)
    {
        itemColor.setColor(_color);
    }
}

[System.Serializable]
public class ItemColor
{
    private Image _image;

    public ItemColor (Image _image)
    {
        this._image = _image;
    }

    public void setColor(Color color)
    {
        _image.color = color;
    }
}
