using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Bellissimo.IikoFront.LoyaltyPlugin.Api;
using Bellissimo.IikoFront.LoyaltyPlugin.Api.Dtos;
using Bellissimo.IikoFront.LoyaltyPlugin.Iiko;
using Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure;

namespace Bellissimo.IikoFront.LoyaltyPlugin.UI
{
    public sealed class LoyaltyViewModel : INotifyPropertyChanged
    {
        private readonly LoyaltyApiClient api;
        private readonly PluginLogger logger;
        private readonly PluginSettings settings;
        private readonly IikoCredentialsProvider creds;
        private readonly IikoOrderSnapshotBuilder snapshotBuilder;
        private readonly IikoDiscountApplier discountApplier;
        private readonly IikoFreeItemApplier freeItemApplier;
        private readonly IdempotencyKeyFactory idempotency;
        private readonly SystemClock clock;
        private LookupResponse cachedLookup;
        private string previewId;
        private DateTimeOffset? previewExpiresAt;
        private long lastApplicationId;

        public LoyaltyViewModel(
            LoyaltyApiClient api,
            PluginLogger logger,
            PluginSettings settings,
            IikoCredentialsProvider creds,
            IikoOrderSnapshotBuilder snapshotBuilder,
            IikoDiscountApplier discountApplier,
            IikoFreeItemApplier freeItemApplier,
            IdempotencyKeyFactory idempotency,
            SystemClock clock)
        {
            this.api = api;
            this.logger = logger;
            this.settings = settings;
            this.creds = creds;
            this.snapshotBuilder = snapshotBuilder;
            this.discountApplier = discountApplier;
            this.freeItemApplier = freeItemApplier;
            this.idempotency = idempotency;
            this.clock = clock;

            LookupCommand = new RelayCommand(async () => await Lookup());
            PreviewCommand = new RelayCommand(async () => await Preview());
            ApplyCommand = new RelayCommand(async () => await Apply());
            CancelCommand = new RelayCommand(async () => await Cancel());
        }

        public string Phone { get; set; }
        public string CustomerInfo { get; set; }
        public string StatusMessage { get; set; }
        public List<RewardDto> Rewards { get; private set; } = new List<RewardDto>();
        public RewardDto SelectedReward { get; set; }
        public ICommand LookupCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand ApplyCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task Lookup()
        {
            try
            {
                var req = new LookupRequest
                {
                    phone = Phone,
                    branch_id = settings.BranchId,
                    terminal_group_id = creds.TerminalGroupId,
                    pos_id = creds.PosId,
                    cashier_id = snapshotBuilder.BuildCurrentOrderSnapshot().CashierId
                };

                logger.Info("Lookup " + logger.MaskPhone(Phone));
                cachedLookup = await api.LookupAsync(req);
                Rewards = cachedLookup.available_coupons ?? new List<RewardDto>();
                CustomerInfo = $"{cachedLookup.name}; BellCoin: {cachedLookup.bellcoin?.available_balance ?? 0}";
                StatusMessage = "Поиск выполнен";
                NotifyAll();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async Task Preview()
        {
            try
            {
                if (cachedLookup == null)
                {
                    StatusMessage = "Сначала выполните поиск";
                    NotifyAll();
                    return;
                }

                var o = snapshotBuilder.BuildCurrentOrderSnapshot();
                var req = new PreviewRequest
                {
                    source = "iiko_pos",
                    customer_id = cachedLookup.customer_id,
                    branch_id = settings.BranchId,
                    terminal_group_id = creds.TerminalGroupId,
                    iiko_order_id = o.IikoOrderId,
                    pos_id = creds.PosId,
                    cashier_id = o.CashierId,
                    customer_coupon_id = SelectedReward?.customer_coupon_id,
                    items = o.Items,
                    use_bellcoin = false,
                    bellcoin_amount = 0
                };

                var r = await api.PreviewAsync(req);
                previewId = r.preview_id;
                previewExpiresAt = r.expires_at;
                StatusMessage = r.allowed ? "Превью успешно" : "Ошибка: " + MapFailureReason(r.failure_reason, r.message);
                NotifyAll();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async Task Apply()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(previewId))
                {
                    StatusMessage = "Сначала получите превью";
                    NotifyAll();
                    return;
                }

                if (previewExpiresAt.HasValue && previewExpiresAt.Value <= clock.UtcNow())
                {
                    StatusMessage = MapFailureReason("PREVIEW_EXPIRED", null);
                    NotifyAll();
                    return;
                }

                var o = snapshotBuilder.BuildCurrentOrderSnapshot();
                var req = new ApplyRequest
                {
                    preview_id = previewId,
                    idempotency_key = idempotency.ForApply(o.IikoOrderId, previewId),
                    source = "iiko_pos",
                    customer_id = cachedLookup.customer_id,
                    iiko_order_id = o.IikoOrderId,
                    branch_id = settings.BranchId,
                    terminal_group_id = creds.TerminalGroupId,
                    pos_id = creds.PosId,
                    cashier_id = o.CashierId,
                    items = o.Items
                };

                var r = await api.ApplyAsync(req);
                if (r.applied)
                {
                    discountApplier.ApplyDiscounts(r, o.IikoOrderId);
                    freeItemApplier.ApplyFreeItems(r.free_items);
                    lastApplicationId = r.application_id;
                    logger.Info("Applied application_id=" + lastApplicationId);
                    StatusMessage = "Успешно применено";
                }

                NotifyAll();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async Task Cancel()
        {
            try
            {
                var o = snapshotBuilder.BuildCurrentOrderSnapshot();
                if (o.IsClosed)
                {
                    StatusMessage = MapFailureReason("ORDER_ALREADY_CLOSED", null);
                    NotifyAll();
                    return;
                }

                var req = new CancelRequest
                {
                    application_id = lastApplicationId,
                    iiko_order_id = o.IikoOrderId,
                    reason = "ORDER_CANCELLED_OR_REWARD_REMOVED",
                    idempotency_key = idempotency.ForCancel(o.IikoOrderId, lastApplicationId)
                };

                var r = await api.CancelAsync(req);
                StatusMessage = r.cancelled ? "Применение отменено" : "Ошибка отмены";
                NotifyAll();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void HandleException(Exception ex)
        {
            var code = (ex as LoyaltyApiException)?.Code ?? "LOYALTY_SERVICE_UNAVAILABLE";
            StatusMessage = MapFailureReason(code, ex.Message);
            logger.Error("ViewModel operation failed", ex);
            NotifyAll();
        }

        private static string MapFailureReason(string code, string fallback){switch(code){case "CUSTOMER_NOT_FOUND": return "Клиент не найден";case "NO_AVAILABLE_REWARDS": return "Нет доступных наград";case "COUPON_NOT_FOUND": return "Купон не найден";case "CUSTOMER_COUPON_NOT_AVAILABLE": return "Купон недоступен";case "CUSTOMER_COUPON_NOT_OWNED_BY_CUSTOMER": return "Купон не принадлежит клиенту";case "COUPON_CONDITION_NOT_MATCHED": return "Условия купона не выполнены";case "BELLCOIN_INSUFFICIENT_BALANCE": return "Недостаточно BellCoin";case "BELLCOIN_REDEMPTION_LIMIT_EXCEEDED": return "Превышен лимит списания BellCoin";case "COUPON_AND_BELLCOIN_NOT_STACKABLE": return "Купон и BellCoin нельзя использовать вместе";case "MULTIPLE_COUPONS_NOT_ALLOWED": return "Можно применить только один купон";case "MAX_TOTAL_DISCOUNT_EXCEEDED": return "Превышен максимальный размер скидки";case "IIKO_PRODUCT_NOT_FOUND": return "Товар iiko не найден";case "IIKO_COMBO_NOT_FOUND": return "Комбо iiko не найден";case "REWARD_ALREADY_APPLIED": return "Награда уже применена";case "REWARD_APPLICATION_NOT_FOUND": return "Применение награды не найдено";case "PREVIEW_EXPIRED": return "Время превью истекло. Выполните превью заново";case "ORDER_ALREADY_CLOSED": return "Заказ уже закрыт";case "LOYALTY_SERVICE_UNAVAILABLE": return "Сервис лояльности недоступен";case "INVALID_POS_CREDENTIALS": return "Неверные POS-учетные данные";default:return fallback??"Неизвестная ошибка";}}
        private void NotifyAll(){PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(CustomerInfo)));PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(StatusMessage)));PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(nameof(Rewards)));}

        private sealed class RelayCommand : ICommand { private readonly Func<Task> execute; public RelayCommand(Func<Task> execute){this.execute=execute;} public bool CanExecute(object p)=>true; public event EventHandler CanExecuteChanged; public async void Execute(object p)=>await execute(); }
    }
}
