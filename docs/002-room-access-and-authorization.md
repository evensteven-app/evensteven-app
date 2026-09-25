# ADR-002: Авторизация доступа к комнатам (ключи, пароль, будущие аккаунты)

**Статус:** Принято  
**Дата:** Сентябрь 2026 г.  
**Заменяет:** пункт «RoomAuthMiddleware» в ADR-001 (контракты и реализация авторизации описываются здесь).

## 1. Контекст и проблема

Доступ к комнате выдаётся одним из способов, **по приоритету (first-match-wins)**:

1.  `X-Edit-Key` — ключ владельца комнаты → роль `admin`;
2.  `X-Participant-Key` — ключ участника → роль `participant`;
3.  `X-Room-Password` — пароль комнаты (Argon2id verify) → роль `guest`.

В перспективе добавляется доступ по аккаунту (JWT + Identity). Одновременно приватное ядро `EvenSteven.Core` обязано централизованно контролировать доступ и мгновенно отсекать комнату при истечении подписки («как у VPN»), не зная ничего об HTTP.

Проверять доступ в каждом контроллере нельзя: логика сквозная, дублируется и конфликтует с будущим auth-слоем. Требуется защита от перебора: после 5 неудачных попыток подряд — `429`, блокировка на 5 минут по ключу `IP + roomId`.

## 2. Принятое решение

**Механизм — MVC authorization filter `RoomAuthorizationFilter`** (`IAsyncAuthorizationFilter`), а не глобальный middleware:

*   работает после роутинга и видит `roomId` из route values;
*   подключается декларативно — **атрибутом `[RoomAuthorization]` только на защищённые эндпоинты** (открытые, например создание комнаты, атрибута не получают);
*   итог — `context.Result = StatusCodeResult` (401/403/429), как в контроллере;
*   соответствует структуре проекта (классические контроллеры, ADR-001).

Доступ оформляется как **грант** — `RoomGrant(RoomId, Role, ParticipantId?)` с ролью из enum `RoomRole { Admin, Participant, Guest }`. Грант собирается **один раз на запрос** в scoped-контексте `ICurrentRoomAccess`. Резолверы (`IRoomAccessProvider`) опрашиваются в порядке приоритета (EditKey → ParticipantKey → Password); первый давший результат выигрывает. Собранные гранты прогоняются через шлюз `IRoomAccessGate` — точку входа приватного Core. Базовый контроллер `RoomControllerBase` открывает контекст (`Access`) во всех обработчиках; `Access.Require(roomId)` остаётся страховкой для вложенных проверок.

Брутфорс-защита `BruteForceGuard` считается на **любые неудачные попытки** (неверный EditKey, ParticipantKey и пароль): 5 подряд → `429` на 5 минут по `IP|roomId`; успех сбрасывает счётчик.

JWT добавляется позже стандартным путём (`UseAuthentication()` + Identity) как ещё один резолвер — цепочка и эндпоинты не меняются.

## 3. Детали

### 3.1. Распределение по сборкам

| Сборка | Что содержит | Почему |
|---|---|---|
| **EvenSteven.Shared** | контракты: `RoomGrant`, `RoomRole`, `RoomAccessRequest`, `ICurrentRoomAccess`, `IRoomAccessProvider`, `IRoomAccessGate` | приватное ядро Core реализует их через DI (Strict Interfaces из README Core); контракты без ASP.NET-типов — Core остаётся Framework Agnostic |
| **EvenSteven.Api** | механика: `RoomAuthorizationFilter`, резолверы (`EditKey` / `ParticipantKey` / `Password`), `BruteForceGuard`, `AllowAllGate` (заглушка), `RoomAccessContext`, `RoomControllerBase`, `GlobalExceptionHandler`, DI-регистрации | HTTP-обвязка открыта и не содержит коммерческой ценности |
| **EvenSteven.Infrastructure** | репозитории (`GetRoomByEditKeyAsync`, `GetParticipantByKeyAsync`, `GetRoomByIdAsync`), Argon2id-хэшер в `Shared/Utils` | обычная работа с БД (SQLite + Dapper) и чистый алгоритм |
| **EvenSteven.Core** (приватное) | `PremiumGate : IRoomAccessGate` — применяет подписку/блокировку, режет гранты при истечении оплаты | коммерческая ценность; подключается в Deploy-пайплайне как submodule |

### 3.2. Контракты

Контракты задают форму, не привязываясь к ASP.NET (Framework Agnostic):

*   `RoomRole` — роли доступа: `Admin` (владелец), `Participant` (участник), `Guest` (вошёл по паролю).
*   `RoomGrant(RoomId, Role, ParticipantId?)` — одна выдача доступа: комната, роль, id участника для роли `Participant`.
*   `RoomAccessRequest` — нейтральный ввод фильтра: `EditKey`, `ParticipantKey`, `Password`, `IpAddress`, `RoomId`.
*   `IRoomAccessProvider` — резолвер с порядковым `Priority`; возвращает список грантов (непустой = победа в цепочке).
*   `IRoomAccessGate` — шлюз коммерческих правил Core: принимает гранты, возвращает разрешённые.
*   `ICurrentRoomAccess` — scoped-контекст: `Grants`, `Require(roomId)` (бросает `ForbiddenException` → 403), `CanEdit(roomId)`.

### 3.3. Приоритетная цепочка в фильтре

| # | Источник | Проверка | Грант |
|---|---|---|---|
| 1 | `X-Edit-Key` | `GetRoomByEditKeyAsync` | `{ roomId, Admin, null }` |
| 2 | `X-Participant-Key` | `GetParticipantByKeyAsync` | `{ roomId, Participant, participantId }` |
| 3 | `X-Room-Password` | Argon2id verify + BruteForceGuard | `{ roomId, Guest, null }` |

Ни один не прошёл → `403`. Пароль приходит заголовком `X-Room-Password`, поэтому фильтр выполняет всю цепочку до привязки модели. По HTTPS заголовки шифруются транзитом; заголовки аутентификации в логи не пишем.

### 3.4. Конвейер запроса

```text
[RoomAuthorizationFilter]  // атрибут [RoomAuthorization] на защищённых эндпоинтах
    └─ RoomAccessRequest (заголовки, IP, roomId из маршрута)
    └─ резолверы по Priority → первый непустой → грант
    └─ IRoomAccessGate                    // точка входа Core (заглушка AllowAllGate)
    └─ ICurrentRoomAccess (scoped) + RoomRole
Controller → Access.Require(roomId)       // 200 или 403
```

### 3.5. BruteForceGuard

*   Считает неудачные попытки **всех трёх веток** (неверные EditKey, ParticipantKey, пароль).
*   Ключ состояния — `IP|roomId`; 5 неудач подряд → `429`, блокировка на 5 минут; успех → сброс.
*   Хранилище — `IMemoryCache` (`Microsoft.Extensions.Caching.Memory`, sliding expiration) в singleton.
*   **P2:** зачистка устаревших записей; после истечения лока новые попытки считаются заново (счётчик = 1), иначе гард перестаёт блокировать после первой истёкшей блокировки; при мульти-инстансном развёртывании счётчик переезжает в БД/Redis.

### 3.6. Связанные решения

*   Пароль комнаты хэшируется **Argon2id** (`Isopoh.Cryptography.Argon2`); функция в `EvenSteven.Shared/Utils/PasswordHasher` (PHC-строка `$argon2id$...` со встроенными солью и параметрами).
*   Валидация входных моделей — **DataAnnotations** (встроенная обработка `[ApiController]`).
*   Оба ключа и пароль проверяются фильтром с первого дня.
*   Dev-строка подключения задаётся в `appsettings.Development.json`; `appsettings.json` — только пример.

## 4. Последствия

*   **Позитивные:**
    *   единый путь проверки доступа с фиксированной приоритетной цепочкой (admin → participant → guest);
    *   ролевой доступ (`RoomRole`) — легко ограничивать отдельные операции по роли (например, только `Admin` может удалить комнату);
    *   точный стык с приватным ядром: `IRoomAccessGate` — единственная точка внедрения коммерческих правил;
    *   JWT добавляется как резолвер, без переписывания контроллеров;
    *   брутфорс-защита по всем трём источникам (`429` + блокировка `IP|roomId`).
*   **Негативные:**
    *   фильтр работает только для MVC-контроллеров (у нас они и используются, ADR-001);
    *   атрибутную защиту нужно не забывать вешать на новые эндпоинты;
    *   `provider` + `gate` + `BruteForceGuard` — три абстракции, риск переусложнения для новичка;
    *   в open-source сборке работает `AllowAllGate` — при сборке продакшена нужно явно включать настоящий `PremiumGate` из Core;
    *   in-memory brute-force счётчик живёт в пределах одного инстанса (мульти-инстанс — P2).
