using canvasync.Components.Pages;

namespace canvasync.Containers;

public class StateContainer
{
    public bool _isHome = true;
    public Type? ComponentType { get; private set; }
    public List<string> imageUrls = new();


    public event Action? OnChange;

    public void SetComponent(Type? componentType)
    {
        _isHome = componentType == typeof(Home) ? true : false;
        ComponentType = componentType;
        NotifyStateChanged();
    }
    private void NotifyStateChanged() => OnChange?.Invoke();
}