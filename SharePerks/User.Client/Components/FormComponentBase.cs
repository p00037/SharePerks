using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using User.Client.Services;
using User.Client.Services.Api.Exceptions;

namespace User.Client.Components
{
    public class FormComponentBase<TModel> : ComponentBase, IDisposable where TModel : class, new()
    {
        [Inject] public OverlayState Overlay { get; set; } = default!;

        protected TModel _formModel = default!;
        protected EditContext? _editContext = default!;
        protected ValidationMessageStore? _messageStore;
        protected string? _serverErrorMessage;

        protected void InitializeEditContext(TModel formModel)
        {
            if (_editContext is not null)
            {
                _editContext.OnValidationRequested -= HandleValidationRequested;
                _editContext.OnFieldChanged -= HandleFieldChanged;
            }

            Overlay.OnChange -= OnOverlayChanged;

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

        protected async Task RunAsync(
                   Func<Task> func,
                   string defaultErrorMessage = "処理に失敗しました。もう一度お試しください")
        {
            if (!OperatingSystem.IsBrowser())
            {
                Overlay.Show();
                return;
            }

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
                var pageLevelErrors = ApplyServerValidationErrors(ex.Errors);

                // キーなし + UIにマップできなかったキー付きエラーを上部表示へ
                var topErrors = ex.Errors
                    .Where(x => string.IsNullOrWhiteSpace(x.Key))
                    .SelectMany(x => x.Value)
                    .Concat(pageLevelErrors)
                    .Distinct()
                    .ToArray();

                _serverErrorMessage = string.Join(Environment.NewLine, topErrors);
            }
            catch (Exception ex)
            {
                _serverErrorMessage = defaultErrorMessage;
                Console.Error.WriteLine(ex);
            }
            finally
            {
                StateHasChanged();
                Overlay.Hide();
            }
        }

        protected async Task Run(Action onValidSubmit, string defaultErrorMessage = "処理に失敗しました。もう一度お試しください")
        {
            await RunAsync(async () =>
            {
                onValidSubmit();
                await Task.CompletedTask;
            }, defaultErrorMessage);
        }

        /// <summary>
        /// UIのフィールドに関連付けできなかったエラーを返す
        /// </summary>
        private IReadOnlyList<string> ApplyServerValidationErrors(IReadOnlyDictionary<string, string[]> errors)
        {
            var pageLevelErrors = new List<string>();

            if (_editContext is null || _messageStore is null)
            {
                return errors.SelectMany(x => x.Value).ToArray();
            }

            _messageStore.Clear();

            foreach (var (fieldName, messages) in errors)
            {
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    pageLevelErrors.AddRange(messages);
                    continue;
                }

                if (CanMapToField(fieldName))
                {
                    var identifier = new FieldIdentifier(_editContext.Model, fieldName);
                    _messageStore.Add(identifier, messages);
                }
                else
                {
                    // キー付きだがUIのValidationMessageで拾えないものは上部へ
                    pageLevelErrors.AddRange(messages);
                }
            }

            _editContext.NotifyValidationStateChanged();
            return pageLevelErrors;
        }

        /// <summary>
        /// 最低限の判定。必要ならAPIキー→UIフィールド名の変換テーブルに置き換える
        /// </summary>
        private bool CanMapToField(string fieldName)
        {
            // 単純なプロパティ名のみ許可する例
            return typeof(TModel).GetProperty(fieldName) is not null;
        }

        protected virtual Task ResetForm(TModel formModel)
        {
            InitializeEditContext(formModel);
            _serverErrorMessage = null;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Overlay.OnChange -= OnOverlayChanged;

            if (_editContext is null)
            {
                return;
            }

            _editContext.OnValidationRequested -= HandleValidationRequested;
            _editContext.OnFieldChanged -= HandleFieldChanged;
        }
    }
}
