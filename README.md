# Bellissimo iikoFront Loyalty Plugin

Плагин интегрирует iikoFront с внешней системой лояльности Bellissimo: поиск клиента, предпросмотр списаний/бонусов, применение скидок и подарков в заказе.

## Требования
- Windows + iikoFront с лицензией разработчика (developer license).
- .NET Framework 4.7.2 Developer Pack.
- `libs/Resto.Front.Api.V8.dll` из вашей установки iikoFront. Версия DLL должна соответствовать версии iikoFront на кассе.

## iikoFront API DLL
`Resto.Front.Api.V8.dll` не берётся из NuGet. Положите DLL в приватный репозиторий по пути:

```text
libs/Resto.Front.Api.V8.dll
```

Проект ссылается на этот файл через `HintPath` в `src/Bellissimo.IikoFront.LoyaltyPlugin/Bellissimo.IikoFront.LoyaltyPlugin.csproj`. Без DLL сборка остановится с явной ошибкой.

## Где указать ключ лицензии плагина
ID лицензии/модуля задаётся **в коде**, в атрибуте `PluginLicenseModuleId`:

- Файл: `src/Bellissimo.IikoFront.LoyaltyPlugin/BellissimoLoyaltyPlugin.cs`
- Строка вида:
  - `[PluginLicenseModuleId(0)]`

Замените `0` на реальный числовой module id из портала разработчика iiko.

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
### GitHub Actions
1. Запушьте изменения в GitHub.
2. Откройте вкладку **Actions**.
3. Запустите workflow **Build plugin** или дождитесь запуска после push.
4. Скачайте artifact **Bellissimo.IikoFront.LoyaltyPlugin** из успешного run.

Workflow собирает проект на Windows runner, использует `libs/Resto.Front.Api.V8.dll` для компиляции и публикует готовую папку плагина.

### Локально на Windows
1. Откройте проект в Visual Studio или Developer Command Prompt.
2. Выполните сборку (`Build`) или команду:

```powershell
msbuild src\Bellissimo.IikoFront.LoyaltyPlugin\Bellissimo.IikoFront.LoyaltyPlugin.csproj /restore /p:Configuration=Release
```

На macOS локальная сборка этого проекта не является целевым сценарием, потому что проект использует `.NET Framework 4.7.2` и WPF.

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
- Указан реальный `PluginLicenseModuleId`.
- Заполнены `ApiBaseUrl`, `BasicAuthLogin`, `BasicAuthPassword`.
- Корректны `BranchId`, `TerminalGroupId`, `PosId` для текущей точки.
- Есть сетевой доступ от кассы до loyalty API.
- В логах нет ошибок авторизации/таймаута.

## Примечания
- Бизнес-правила лояльности (какие награды и как суммируются) определяются внешним loyalty-сервисом.
- Плагин отображает и применяет только то, что вернул API.
