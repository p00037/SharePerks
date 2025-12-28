using System.Text;
using Admin.Client.Services;
using Admin.Client.Services.Api.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using static MudBlazor.CategoryTypes;

namespace Admin.Client.Components
{
    public abstract class FormComponentBase<TModel> :Microsoft.AspNetCore.Components.ComponentBase, IDisposable where TModel : class, new()
    {
        [Inject] public OverlayState Overlay { get;  set; } = default!;

        protected TModel _formModel = new();
        protected EditContext? _editContext;
        protected ValidationMessageStore? _messageStore;
        protected string? _serverErrorMessage;

        protected void InitializeEditContext(TModel formModel)
        {
            if (_editContext is not null)
            {
                _editContext.OnValidationRequested -= HandleValidationRequested;
                _editContext.OnFieldChanged -= HandleFieldChanged;
            }

            _formModel = formModel;
            _editContext = new EditContext(_formModel);
            _messageStore = new ValidationMessageStore(_editContext);
            _editContext.OnValidationRequested += HandleValidationRequested;
            _editContext.OnFieldChanged += HandleFieldChanged;
            Overlay.OnChange += OnOverlayChanged;
        }

        private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs e)
        {
            _messageStore?.Clear();
            _serverErrorMessage = null;
        }

        private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
        {
            _messageStore?.Clear(e.FieldIdentifier);
        }

        private void OnOverlayChanged()
        {
            InvokeAsync(StateHasChanged);
        }

        protected async Task RunAsync(Func<Task> func, string dafaultErrorMessage = "処理に失敗しました。もう一度お試しください")
        {
            if (Overlay.Visible)
            {
                return;
            }

            try
            {
                Overlay.Show();
                await func();
            }
            catch (ApiValidationException ex)
            {
                ApplyServerValidationErrors(ex.Errors);
                _serverErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _serverErrorMessage = dafaultErrorMessage;
                Console.Error.WriteLine(ex);
            }
            finally
            {
                Overlay.Hide();
                StateHasChanged();
            }
        }

        protected async Task Run(Action onValidSubmit)
        {
            await RunAsync(async () =>
            {
                onValidSubmit();
                await Task.CompletedTask;
            });
        }

        private void ApplyServerValidationErrors(IReadOnlyDictionary<string, string[]> errors)
        {
            if (_editContext is null || _messageStore is null)
            {
                return;
            }

            _messageStore.Clear();

            foreach (var (fieldName, messages) in errors)
            {
                var identifier = new FieldIdentifier(_editContext.Model, fieldName);
                _messageStore.Add(identifier, messages);
            }

            _editContext.NotifyValidationStateChanged();
        }

        protected virtual Task ResetForm()
        {
            InitializeEditContext(new TModel());
            _serverErrorMessage = null;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_editContext is null)
            {
                return;
            }

            _editContext.OnValidationRequested -= HandleValidationRequested;
            _editContext.OnFieldChanged -= HandleFieldChanged;
            Overlay.OnChange -= OnOverlayChanged;
        }
    }
}
