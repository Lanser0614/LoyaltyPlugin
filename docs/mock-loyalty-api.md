# Mock Loyalty API

Документ описывает mock-сервис для тестирования Bellissimo iikoFront Loyalty Plugin.

## Base URL

Render:

```text
https://bellissimo-loyalty-mock-service.onrender.com
```

Локально:

```text
http://localhost:8080
```

В `app.config` плагина:

```xml
<add key="ApiBaseUrl" value="https://bellissimo-loyalty-mock-service.onrender.com"/>
```

## Общий Flow

1. Оператор вводит телефон клиента в плагине.
2. Плагин отправляет `lookup`.
3. API возвращает клиента, баланс BellCoin и доступные купоны.
4. Оператор выбирает один из вариантов:
   - списать BellCoin;
   - применить скидочный купон.
5. Плагин отправляет `preview`.
6. API возвращает предварительный расчёт скидки.
7. Оператор нажимает `Apply`.
8. Плагин отправляет `apply`.
9. API подтверждает применение, а плагин применяет `flexible_sum` скидку в iikoFront.
10. Если нужно отменить применение, плагин отправляет `cancel`.

## Endpoints

```text
POST /api/pos/incentives/lookup
POST /api/pos/incentives/preview
POST /api/pos/incentives/apply
POST /api/pos/incentives/cancel
```

Все запросы и ответы используют JSON.

## 1. Lookup

Ищет клиента по телефону.

### Request

```json
{
  "phone": "+998901234567",
  "branch_id": 10,
  "terminal_group_id": "iiko-terminal-group-id",
  "pos_id": "front-01",
  "cashier_id": "cashier-1"
}
```

### Response

```json
{
  "customer_id": 1001,
  "name": "Mock Customer",
  "phone": "+998901234567",
  "bellcoin": {
    "available_balance": 15000,
    "expires_soon_amount": 0
  },
  "available_coupons": [
    {
      "customer_coupon_id": 501,
      "coupon_id": 101,
      "name": "Скидочный купон 10 000 сум",
      "action_type": "discount",
      "expires_at": "2026-12-31T23:59:59+05:00"
    }
  ]
}
```

Mock всегда возвращает одного клиента:

- BellCoin balance: `15000`
- Coupon: скидка `10000`
- Coupon id для preview: `customer_coupon_id = 501`

## 2. Preview: BellCoin

Используется, когда оператор вводит сумму BellCoin для списания.

### Request

```json
{
  "source": "iikoFront",
  "customer_id": 1001,
  "branch_id": 10,
  "terminal_group_id": "iiko-terminal-group-id",
  "iiko_order_id": "order-guid",
  "pos_id": "front-01",
  "cashier_id": "cashier-1",
  "coupon_code": null,
  "customer_coupon_id": null,
  "use_bellcoin": true,
  "bellcoin_amount": 15000,
  "items": []
}
```

### Response

```json
{
  "allowed": true,
  "preview_id": "mock_bellcoin_15000",
  "expires_at": "2026-06-02T16:24:20.533Z",
  "total_discount_amount": 15000,
  "failure_reason": null,
  "message": "BellCoin discount can be applied",
  "discounts": [
    {
      "type": "bellcoin",
      "amount": 15000,
      "apply_mode": "flexible_sum"
    }
  ],
  "free_items": []
}
```

## 3. Preview: Coupon

Используется, когда оператор выбирает скидочный купон.

### Request

```json
{
  "source": "iikoFront",
  "customer_id": 1001,
  "branch_id": 10,
  "terminal_group_id": "iiko-terminal-group-id",
  "iiko_order_id": "order-guid",
  "pos_id": "front-01",
  "cashier_id": "cashier-1",
  "coupon_code": null,
  "customer_coupon_id": 501,
  "use_bellcoin": false,
  "bellcoin_amount": 0,
  "items": []
}
```

Можно также передать:

```json
{
  "coupon_code": "MOCK10000"
}
```

### Response

```json
{
  "allowed": true,
  "preview_id": "mock_coupon_10000",
  "expires_at": "2026-06-02T16:24:21.147Z",
  "total_discount_amount": 10000,
  "failure_reason": null,
  "message": "Coupon discount can be applied",
  "discounts": [
    {
      "type": "coupon",
      "amount": 10000,
      "apply_mode": "flexible_sum"
    }
  ],
  "free_items": []
}
```

## 4. Apply: BellCoin

Подтверждает BellCoin preview.

### Request

```json
{
  "preview_id": "mock_bellcoin_15000",
  "idempotency_key": "unique-key-1",
  "source": "iikoFront",
  "customer_id": 1001,
  "iiko_order_id": "order-guid",
  "branch_id": 10,
  "terminal_group_id": "iiko-terminal-group-id",
  "pos_id": "front-01",
  "cashier_id": "cashier-1",
  "items": []
}
```

### Response

```json
{
  "applied": true,
  "application_id": 9001,
  "total_discount_amount": 15000,
  "discounts": [
    {
      "type": "bellcoin",
      "amount": 15000,
      "apply_mode": "flexible_sum"
    }
  ],
  "free_items": [],
  "instructions_for_pos": [
    "Apply BellissimoLoyalty BellCoin flexible sum discount"
  ]
}
```

## 5. Apply: Coupon

Подтверждает coupon preview.

### Request

```json
{
  "preview_id": "mock_coupon_10000",
  "idempotency_key": "unique-key-2",
  "source": "iikoFront",
  "customer_id": 1001,
  "iiko_order_id": "order-guid",
  "branch_id": 10,
  "terminal_group_id": "iiko-terminal-group-id",
  "pos_id": "front-01",
  "cashier_id": "cashier-1",
  "items": []
}
```

### Response

```json
{
  "applied": true,
  "application_id": 9002,
  "total_discount_amount": 10000,
  "discounts": [
    {
      "type": "coupon",
      "amount": 10000,
      "apply_mode": "flexible_sum"
    }
  ],
  "free_items": [],
  "instructions_for_pos": [
    "Apply BellissimoLoyalty coupon flexible sum discount"
  ]
}
```

## 6. Cancel

Отменяет применение скидки/купона на стороне loyalty API.

### Request

```json
{
  "application_id": 9002,
  "iiko_order_id": "order-guid",
  "reason": "operator_cancelled",
  "idempotency_key": "unique-cancel-key"
}
```

### Response

```json
{
  "cancelled": true,
  "coupon_usage_status": "cancelled",
  "customer_coupon_status": "available",
  "bellcoin_status": "refunded"
}
```

## Error Scenario

Если не выбран BellCoin и не выбран купон, preview возвращает:

```json
{
  "allowed": false,
  "preview_id": "mock_no_reward",
  "expires_at": "2026-06-02T16:24:21.147Z",
  "total_discount_amount": 0,
  "failure_reason": "NO_REWARD_SELECTED",
  "message": "Select BellCoin or the mock coupon",
  "discounts": [],
  "free_items": []
}
```

## Notes

- `free_items` в текущем mock всегда `[]`.
- Все скидки возвращаются как `apply_mode: "flexible_sum"`.
- Плагин применяет эти скидки в iikoFront через discount type `BellissimoLoyalty`.
- Для coupon сценария используйте `customer_coupon_id: 501`.
- Для BellCoin сценария используйте `use_bellcoin: true` и `bellcoin_amount: 15000`.
