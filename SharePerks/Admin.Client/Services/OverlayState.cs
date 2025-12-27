namespace Admin.Client.Services
{
    public sealed class OverlayState
    {
        public bool Visible { get; private set; }

        public event Action? OnChange;

        public void Show()
        {
            if (Visible) return;
            Visible = true;
            OnChange?.Invoke();
        }

        public void Hide()
        {
            if (!Visible) return;
            Visible = false;
            OnChange?.Invoke();
        }

        public void Set(bool visible)
        {
            if (Visible == visible) return;
            Visible = visible;
            OnChange?.Invoke();
        }
    }
}
