using System.Text;
using Admin.Client.Services.Api.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Admin.Client.Components
{
    public abstract class FormComponentBase<TModel> : ComponentBase, IDisposable where TModel : class, new()
    {
        protected TModel _formModel = new();
        protected EditContext? _editContext;
        protected ValidationMessageStore? _messageStore;
        protected string? _serverErrorMessage;
        private bool _isRun = false;

        protected bool IsRun => _isRun;

        protected void InitializeEditContext(TModel formModel)
        {
            if (_editContext is not null)
            {
                _editContext.OnValidationRequested -= HandleValidationRequested;
                _editContext.OnFieldChanged -= HandleFieldChanged;
            }

            _formModel = new();
            _editContext = new EditContext(_formModel);
            _messageStore = new ValidationMessageStore(_editContext);
            _editContext.OnValidationRequested += HandleValidationRequested;
            _editContext.OnFieldChanged += HandleFieldChanged;
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

        protected async Task RunAsync(Func<Task> onValidSubmit)
        {
            if (_isRun)
            {
                return;
            }

            try
            {
                _isRun = true;
                await onValidSubmit();
            }
            catch (ApiValidationException ex)
            {
                ApplyServerValidationErrors(ex.Errors);
                _serverErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _serverErrorMessage = "フォームの送信に失敗しました。もう一度お試しください。";
                Console.Error.WriteLine(ex);
            }
            finally
            {
                _isRun = false;
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

        private void ResetForm()
        {
            InitializeEditContext(new TModel());
            _serverErrorMessage = null;
        }

        public void Dispose()
        {
            if (_editContext is null)
            {
                return;
            }

            _editContext.OnValidationRequested -= HandleValidationRequested;
            _editContext.OnFieldChanged -= HandleFieldChanged;
        }
    }
}
