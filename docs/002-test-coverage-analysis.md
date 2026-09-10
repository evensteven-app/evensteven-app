# Анализ тестового покрытия EvenSteven

Дата: 2026-09-10
Проект: `tests/EvenSteven.RepositoryTests`

## Сводка

| Слой | Методы | Покрыто | Частично | Не покрыто |
|---|---|---|---|---|
| SqliteRoomRepository | 2 | 2 | 0 | 0 |
| SqliteParticipantRepository | 4 | 1 | 0 | 3 |
| SqliteExpenseRepository | 3 | 3 | 0 | 0 |
| ExpenseUtils | 1 | 1 | 0 | 0 |
| TypeHandlers (4 класса) | 8 | 0 | 0 | 8 |
| SqliteConnectionFactory | 3 | 1 | 1 | 1 |
| TaskResult\<T, E\> | 2 | 0 | 0 | 2 |
| Migration 001 | 2 | 1 | 0 | 1 |
| Api (WeatherForecast) | 2 | 0 | 0 | 2 |

---

## 1. SqliteRoomRepository

### `CreateRoomAsync` — покрыт
- Тест: `CreateRoom_ValidParameters_ReturnNewRoomId`
- Проверяет: генерация GUID, вставка, повторное чтение.

Не покрыто:
- `catch (DbException)` (строка 37-41) — не тестируется.
- Нарушение `UNIQUE` по `InviteCode` / `EditKey`.
- Передача cancellation token'а.

### `GetRoomByIdAsync` — покрыт
- Тесты: `GetRoom_ExistingGuid_ReturnsRoom`, `GetRoom_NonExistingGuid_ReturnsNull`.

Не покрыто:
- `catch (DbException)` (строка 62-66).
- Cancellation token.

---

## 2. SqliteParticipantRepository

### `AddParticipantAsync` — НЕ покрыт
- Участники в тестах вставляются через сырой SQL в `InitializeAsync`, а не через репозиторий.
- Возвращаемый GUID не проверяется.
- `catch (DbException)` (строка 36-40) не тестируется.
- Дубликат участника в комнате не тестируется.

### `DeleteParticipantAsync` — НЕ покрыт
- Нет тестов вообще.
- Happy path (удаление существующего): нет.
- Удаление несуществующего (тихий no-op — catch **проглатывает** ошибку, строка 60-63): нет.
- Проверка, что участник исчез из `GetParticipantsByRoomAsync`: нет.

### `GetParticipantsByRoomAsync` — покрыт
- Тесты: `GetParticipants_ValidRoomId_ReturnParticipantsWithBalances`, `GetParticipants_ParticipantWithoutSpending_IsIncludedWithZeroBalance`, `GetParticipants_RevertedExpense_DoesNotCountTowardsPayerBalance`, `GetParticipants_InvalidRoomId_ReturnsEmptyList`.
- Покрывает: расчёт баланса, zero balance, исключение reverted-трат, пустая комната.

> Примечание: баг с «задвоением» баланса (fan-out через два независимых LEFT JOIN) уже исправлен — запрос переписан на CTE `Shared`/`Paid` (строка 70-94).

Не покрыто:
- `catch (DbException)` (строка 102-106).
- Комната, где **все** траты reverted (балансы всех = 0).
- Несколько не-reverted трат одного плательщика (агрегация).
- Участник, который и платил, и участвовал (проверка чистого баланса).
- Участник, который только shared (отрицательный баланс).

### `UpdateParticipantNameAsync` — НЕ покрыт
- Нет тестов вообще.
- Happy path (обновление имени): нет.
- Обновление несуществующего (тихий no-op — catch **проглатывает** ошибку, строка 126-129): нет.
- Верификация переименования через `GetParticipantsByRoomAsync`: нет.

---

## 3. SqliteExpenseRepository

### `AddExpenseAsync` — покрыт
- Тесты: `AddExpense_ValidData_CreateExpenseAndExpenseEntries`, `AddExpense_ValidData_SplitAmountAmongParticipants`.
- Покрывает: одиночный участник, раздел на нескольких, сумма долей, коммит транзакции.

Не покрыто:
- `catch (DbException)` + `transaction.RollbackAsync` (строка 62-68) — откат транзакции не проверяется.
- Cancellation во время транзакции.
- Большой `Amount` (переполнение long).

### `GetExspensesByRoomAsync` — покрыт
- Тесты: `GetExpenses_ValidId_ReturnAllExpensesForGivenRoom`, `GetExpenses_UnknownId_ReturnEmptyList`.

Не покрыто:
- `catch (DbException)` (строка 88-92).
- Комната с пустым списком трат.
- Проверка, возвращаются ли reverted-траты (запрос без фильтра `IsReverted`).

### `RevertExpenseAsync` — покрыт
- Тесты: `RevertExpense_ValidExpenseId_SetExpenseAsRevertedAndDeleteAllExpenseEntries`, `RevertExpense_InvalidExpenseId_DoNothing`.

Не покрыто:
- `catch (DbException)` + `transaction.RollbackAsync` (строка 126-132).
- Повторный revert уже-reverted траты (идемпотентность).
- Проверка, что revert затрагивает только целевую трату.

---

## 4. ExpenseUtils.SplitAmount — покрыт

- Тесты: `SplitAmount_ReturnValidShareByParticipant` (Theory: 1, 2, 3, 7 участников), `SplitAmount_NoParticipants_ReturnEmptyDictionary`.

Не покрыто:
- `Amount = 0` → все доли 0.
- Отрицательный `Amount`.
- `Amount < количество участников` (напр. 2 участника, amount=1 → кому-то 1, кому-то 0).
- Очень большой `Amount` (переполнение `baseShare + 1`).
- Дубликаты GUID в `participantIds` (авария на `.Add` — Dictionary).
- Изменение исходного списка: метод вызывает `participantIds.Sort()` — **мутирует входной список**.

---

## 5. TypeHandlers — НЕ покрыты (все 4 класса)

Косвенно участвуют в работе (Dapper их вызывает при маппинге репозиториев), но прямо не тестируются:

### `SqliteGuidTypeHandler`
- `Parse(object)` — не покрыт. Нет проверки: валидный GUID-строки, невалидной строки (`FormatException`), null.
- `SetValue(IDbDataParameter, Guid)` — не покрыт.

### `SqliteDateTimeTypeHandler`
- `Parse(object)` — не покрыт.
- `SetValue(IDbDataParameter, DateTime)` — не покрыт.

### `SqliteBoolTypeHandler`
- `Parse(object)` — не покрыт. Ветки: null/DBNull → false; `"1"`/`"true"`/`"True"`/`"TRUE"` → true; `_` → false.
- `SetValue(IDbDataParameter, bool)` — не покрыт (true → 1, false → 0).

### `SqliteInt64TypeHandler`
- `Parse(object)` — не покрыт. Ветки: `DBNull` → 0; обычное значение.
- `SetValue(IDbDataParameter, long)` — не покрыт.

### `TypeHandlersManager.RegisterSqliteTypeHandlers`
- Идемпотентность повторного вызова не проверена.

---

## 6. SqliteConnectionFactory

- Конструктор `(string)` — покрыт косвенно через `SqliteClassFixture`.
- Конструктор `(IConfiguration)` — не покрыт.
- `CreateConnection()` — частично (когда `_connectionString == null` → `InvalidOperationException`, строка 23-25 — не проверено).
- `CreateOpenConnectionAsync` — не покрыт напрямую: проверка, что соединение реально открыто, отсутствует.

---

## 7. TaskResult\<T, E\> — НЕ покрыт

- `Success(T data)` — не проверяет `IsSuccess = true`, `Data` установлен, `Error = null`.
- `Failure(E error)` — не проверяет `IsSuccess = false`, `Error` установлен, `Data = null`.

---

## 8. Миграция 001

- `Up()` — покрыта косвенно (`TestDatabase.Create()` вызывает `MigrateUp()` на каждый класс-фикстуру).
- `Down()` — не покрыт. Также в `Down()` удаляются таблицы `Events` и `EventEntries`, которых нет в `Up()` — потенциальный баг.
- Дефолты не проверены: `Version = 1`, `IsReverted = false`, `RevertedAt = null`.

---

## 9. Api слой — НЕ покрыт

- `WeatherForecast.TemperatureF` — формула не тестируется.
- `WeatherForecastController.Get()` — не тестируется.
- `Program.Main()` — DI, миграции, регистрация type handlers — не тестируются.

---

## 10. Баг в тестовом хелпере

`SqliteExpenseRepositoryTests.GetExpenseEntriesByExpenseId` (строка 17-23):

```sql
WHERE Id = @ExpenseId
```

Фильтрует по `Id` записи, а не по `ExpenseId`. Из-за этого assertion в `RevertExpense_ValidExpenseId_...` (строка 186-188) фактически проверяет пустой список и «проходит вхолостую». Должно быть:

```sql
WHERE ExpenseId = @ExpenseId
```

---

## 11. Общее межсекторное

| Сценарий | Статус |
|---|---|
| CancellationToken отменён во время операции | не тестируется |
| `catch (DbException)` во всех репозиториях (9 блоков) | не тестируется |
| Откат транзакций (`AddExpenseAsync`, `RevertExpenseAsync`) | не тестируется |
| Параллельные вставки / конкурентность | не тестируется |
| `TypeHandlersManager.RegisterSqliteTypeHandlers` повторно | не тестируется |
| `TestDatabase.ResetAsync()` (цикл PRAGMA foreign_keys) | не тестируется |

---

# Как тестировать `catch (DbException)`

## Контекст

Все `catch`-блоки ловят `DbException` — базовый класс ошибок ADO.NET. В тестах БД реальная (SQLite), поэтому единственный способ честно попасть в catch — заставить SQLite выбросить ошибку.

Есть 4 подхода.

## Вариант 1 — Ломать реальную схему (`DROP TABLE`) — РЕКОМЕНДУЕТСЯ

Перед вызовом репозитория дропнуть таблицу, по которой пойдёт запрос. SQLite бросит `SqliteException` (наследник `DbException`) → сработает catch.

Примеры для каждого метода:

| Метод | Что дропнуть | Какая команда упадёт |
|---|---|---|
| `RoomRepository.CreateRoomAsync` | `Rooms` | INSERT |
| `RoomRepository.GetRoomByIdAsync` | `Rooms` | SELECT |
| `ParticipantRepository.AddParticipantAsync` | `Participants` | INSERT |
| `ParticipantRepository.DeleteParticipantAsync` | `Participants` | DELETE |
| `ParticipantRepository.UpdateParticipantNameAsync` | `Participants` | UPDATE |
| `ParticipantRepository.GetParticipantsByRoomAsync` | `Expenses` или `ExpenseEntries` | SELECT (через CTE) |
| `ExpenseRepository.AddExpenseAsync` | `ExpenseEntries` | INSERT entry (после INSERT expense) |
| `ExpenseRepository.GetExspensesByRoomAsync` | `Expenses` | SELECT |
| `ExpenseRepository.RevertExpenseAsync` | `ExpenseEntries` | DELETE (после UPDATE) |

Бонус для транзакций: дропая `ExpenseEntries` в `AddExpenseAsync`, первая команда (INSERT в `Expenses`) успевает выполниться, вторая падает → это **реальная проверка `RollbackAsync`**: после `Assert.ThrowsAsync<DbException>` утверждаем, что expense не сохранился, и `IsReverted` остался `false`.

Критичные правила:

1. **Отдельный класс тестов со своим фикстуром.** `SqliteClassFixture` создаётся xUnit'ом на каждый тестовый класс → свежая БД. Нельзя дропать таблицы в классе с обычными тестами: `ResetAsync()` чистит только строки (`DELETE`), таблицу не восстанавливает.
2. **В конце теста пересоздать таблицу** (`CREATE TABLE ...` из миграции), чтобы не сломать последующие тесты класса.
3. `Assert.ThrowsAsync<DbException>` — поймает и `SqliteException`, и пере-выброс из catch.

Минус: немного бойлерплейта (drop + recreate).

## Вариант 2 — Фейковая `IDbConnectionFactory`

Свой класс, реализующий `IDbConnectionFactory`, возвращающий обёртку над `SqliteConnection`, которая кидает `DbException` при выполнении команды.

Плюс: точный контроль ошибки, не трогает схему, можно переиспользовать в любом классе.
Минус: `DbConnection`/`DbCommand` — много абстрактных членов, обвязки больше, чем самих тестов. Нужно попасть в ту точку, которую реально вызывает Dapper (в текущих версиях Dapper выполняет `ExecuteNonQueryAsync`/`ExecuteReaderAsync` на `DbCommand`).

## Вариант 3 — Mocking-библиотека (Moq / NSubstitute)

Добавить пакет в тестовый проект (сейчас отсутствует). Мокаем `IDbConnectionFactory`, который возвращает connection, команда которого бросает `DbException`.

Плюс: минимум кода, читаемо.
Минус: новая зависимость; из-за того что Dapper зовёт методы на `DbCommand`, а не на connection, настройка mock-а чуть хитрее.

## Вариант 4 — Write lock (не рекомендуется)

Второе соединение держит `BEGIN EXCLUSIVE`, репозиторий пишет в занятую БД → `SQLITE_BUSY` → `DbException`. Хрупко, зависит от таймингов и настроек busy timeout.

---

# Рекомендации по приоритетам

**P0 — баг:**
1. Исправить `GetExpenseEntriesByExpenseId`: `WHERE Id = @ExpenseId` → `WHERE ExpenseId = @ExpenseId`.

**P1 — высокоимипактные дыры:**
1. Тесты `DeleteParticipantAsync` и `UpdateParticipantNameAsync` (полностью непокрыты).
2. Тесты `AddParticipantAsync` через репозиторий.
3. `SplitAmount`: `amount = 0`, `amount < участников`, отрицательный amount, дубликаты GUID.
4. `catch (DbException)` + откат транзакций (см. секцию выше, Вариант 1).

**P2 — средний приоритет:**
1. `SqliteBoolTypeHandler.Parse` (ветви: null/DBNull, "0", "1", "true"/"True"/"TRUE", дефолт).
2. `TaskResult<T, E>`.
3. `SqliteConnectionFactory` — `InvalidOperationException` при отсутствии строки подключения.
4. Комната, где все траты reverted, в тестах балансов.

**P3 — низкий приоритет:**
1. `SetValue` у TypeHandler'ов.
2. Идемпотентность `TypeHandlersManager.RegisterSqliteTypeHandlers`.
3. Миграция `Down()` (в т.ч. проверить баг с `Events`/`EventEntries`).
4. Api-слой.
5. CancellationToken.