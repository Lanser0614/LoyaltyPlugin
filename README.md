# Bellissimo iikoFront Loyalty Plugin

Плагин интегрирует iikoFront с внешней системой лояльности Bellissimo: поиск клиента, предпросмотр списаний/бонусов, применение скидок и подарков в заказе.

## Требования
- Windows + iikoFront с лицензией разработчика (developer license).
- .NET Framework 4.7.2 Developer Pack.
- Доступ к `Resto.Front.Api.Vx.dll` (версия должна соответствовать вашей версии iikoFront).

## Где указать ключ лицензии плагина
Ключ лицензии (GUID) задаётся **в коде**, в атрибуте `PluginLicenseModuleId`:

- Файл: `src/Bellissimo.IikoFront.LoyaltyPlugin/BellissimoLoyaltyPlugin.cs`
- Строка вида:
  - `[PluginLicenseModuleId("00000000-0000-0000-0000-000000000000")]`

Замените `00000000-0000-0000-0000-000000000000` на ваш реальный GUID из портала разработчика iiko.

## Конфигурация (`app.config`)
Файл: `src/Bellissimo.IikoFront.LoyaltyPlugin/app.config`

> Важно: во время работы iikoFront плагин читает конфиг **не из `iikoFront.exe.config`**, а из файла рядом со сборкой плагина:  
> `Bellissimo.IikoFront.LoyaltyPlugin.dll.config`.

Обязательные параметры:
- `ApiBaseUrl` — базовый URL loyalty API (например, `https://loyalty.example.com`).
- `BasicAuthLogin` — логин Basic Auth.
- `BasicAuthPassword` — пароль Basic Auth.
- `BranchId` — ID филиала в вашей системе лояльности.
- `TerminalGroupId` — ID группы терминалов iiko.
- `PosId` — ID/имя конкретной кассы (POS).

Дополнительно:
- `HttpTimeoutSeconds` — timeout HTTP-запросов (по умолчанию 10).
- `LogDirectory` — папка для логов (по умолчанию `logs`).

Пример:

```xml
<appSettings>
  <add key="ApiBaseUrl" value="https://loyalty.example.com"/>
  <add key="BasicAuthLogin" value="CHANGE_ME"/>
  <add key="BasicAuthPassword" value="CHANGE_ME"/>
  <add key="BranchId" value="10"/>
  <add key="TerminalGroupId" value="iiko-terminal-group-id"/>
  <add key="PosId" value="front-01"/>
  <add key="HttpTimeoutSeconds" value="10"/>
  <add key="LogDirectory" value="logs"/>
</appSettings>
```

## Сборка
1. Откройте решение/проект в Visual Studio.
2. Проверьте ссылку на `Resto.Front.Api.Vx.dll` в `.csproj` (при необходимости обновите `HintPath`).
3. Выполните сборку (`Build`) или через команду `msbuild`.

## Установка в iikoFront
1. Скопируйте в папку `Plugins` iikoFront:
   - `Bellissimo.IikoFront.LoyaltyPlugin.dll`
   - `Manifest.xml`
   - `Bellissimo.IikoFront.LoyaltyPlugin.dll.config` (или актуальный config-файл сборки)
2. Перезапустите iikoFront.
3. Убедитесь, что плагин загрузился без ошибок (по логам и в UI iikoFront).

## Формат `Manifest.xml` (V8)
Файл манифеста должен быть в формате:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Manifest>
  <FileName>Bellissimo.IikoFront.LoyaltyPlugin.dll</FileName>
  <TypeName>Bellissimo.IikoFront.LoyaltyPlugin.BellissimoLoyaltyPlugin</TypeName>
  <ApiVersion>V8</ApiVersion>
</Manifest>
```

Namespace `xmlns=...` для этого варианта не используется.

## Что должно работать после запуска
- На экране заказа появляется кнопка **Loyalty**.
- По кнопке открывается окно лояльности поверх iikoFront (с попыткой перевести окно в foreground).
- При `Apply` плагин:
  - отправляет `apply` во внешний loyalty API;
  - применяет скидку в заказ через `IEditSession` + flexible sum discount;
  - добавляет free items в заказ (если возвращены API).

## Проверка запуска (чек-лист)
- Указан реальный `PluginLicenseModuleId` GUID.
- Заполнены `ApiBaseUrl`, `BasicAuthLogin`, `BasicAuthPassword`.
- Корректны `BranchId`, `TerminalGroupId`, `PosId` для текущей точки.
- Есть сетевой доступ от кассы до loyalty API.
- В логах нет ошибок авторизации/таймаута.

## Примечания
- Бизнес-правила лояльности (какие награды и как суммируются) определяются внешним loyalty-сервисом.
- Плагин отображает и применяет только то, что вернул API.
