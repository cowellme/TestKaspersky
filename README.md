# Vulnerability Tracker API

REST API сервис для управления уязвимостями программного обеспечения и асинхронными задачами сканирования репозиториев.

## Технологии

- .NET 10, ASP.NET Core
- In-memory хранилище (без СУБД, состояние живёт до перезапуска процесса)
- Очередь задач на System.Threading.Channels
- Фоновое выполнение через BackgroundService
- Swashbuckle (Swagger UI)
- xUnit для unit-тестов

## Требования

- .NET SDK 10.0
- Docker Desktop (опционально, только для запуска в контейнере)

## Запуск

### Локально

```bash
dotnet restore
dotnet run --project TestKaspersky
```

- API: http://localhost:5272
- Swagger UI: http://localhost:5272/swagger (Только в режиме Debug)

### В Docker

```bash
docker compose up -d --build
```

- API: http://localhost:8080

Примечание: сервис работает с in-memory-хранилищем и база данных ему не нужна. Объявленный в docker-compose.yml контейнер PostgreSQL остался от ранней версии проекта с EF Core и текущей реализацией не используется. 

## Конфигурация

Задаётся в appsettings.json или через переменные окружения.

| Параметр | Переменная окружения | По умолчанию | Описание |
|---|---|---|---|
| Scanning:MaxParallel | Scanning__MaxParallel | 2 | Максимальное число одновременно выполняемых сканирований, остальные задачи ждут в очереди |
| BackgroundQueue:Capacity | BackgroundQueue__Capacity | 500 | Ёмкость очереди задач сканирования |

## Эндпоинты

| Метод | Путь | Описание | Ответы |
|---|---|---|---|
| GET | /api/vulnerabilities | Список уязвимостей | 200, 204 |
| GET | /api/vulnerabilities/{id} | Уязвимость по идентификатору | 200, 404 |
| POST | /api/vulnerabilities | Создать уязвимость | 200 (возвращает ID), 400 |
| DELETE | /api/vulnerabilities/{id} | Удалить уязвимость | 204, 404 |
| GET | /api/scan | Список задач сканирования | 200, 204 |
| GET | /api/scan/{id} | Статус задачи сканирования | 200, 404 |
| POST | /api/scan?url=... | Запустить сканирование репозитория | 200 (возвращает ID задачи), 400, 404 |
| POST | /api/scan/{id}/cancel | Отменить задачу сканирования | 200, 404 |

Уровни критичности (Severity): 0 — Low, 1 — Medium, 2 — High, 3 — Critical.

## Примеры запросов

Создание уязвимости:

```bash
curl -X POST "http://localhost:5272/api/vulnerabilities" \
  -H "Content-Type: application/json" \
  -d "{\"title\": \"SQL-инъекция в модуле отчётов\", \"severity\": 3}"
```

Получение уязвимости:

```bash
curl "http://localhost:5272/api/vulnerabilities/00000000-0000-0000-0000-000000000000"
```

Запуск сканирования репозитория:

```bash
curl -X POST "http://localhost:5272/api/scan?url=https://gitlab.com/example/repo"
```

Получение статуса задачи:

```bash
curl "http://localhost:5272/api/scan/00000000-0000-0000-0000-000000000000"
```

Отмена задачи:

```bash
curl -X POST "http://localhost:5272/api/scan/00000000-0000-0000-0000-000000000000/cancel"
```

## Структура проекта

```
TestKaspersky/
├── Controllers/            # HTTP-эндпоинты
├── Data/                   # IRepository и InMemoryRepository
├── Models/                 # сущности, enum-ы и DTO запросов/ответов
├── Services/               # бизнес-логика, очередь задач, фоновый воркер, сканер, логгер в файл
├── Program.cs              # настройка приложения и DI
└── appsettings.json        # конфигурация
TestKaspersky.Tests/        # unit-тесты (xUnit)
docker-compose.yml          # запуск в Docker
CHECKLIST.md                # чек-лист тестового задания
```

## Логирование

- Консоль
- Файл logs/app.log (папка создаётся автоматически при первом запуске)
- HTTP-запросы логируются с методом, путём, кодом ответа и длительностью


Покрыты: in-memory репозиторий, сервисы уязвимостей и сканирования, реестр отмены, очередь, фоновый воркер, сканер, контроллеры. 
Общее покрытие кода 42%, тесты закрывают критическую бизнес логику задания. 

## Примечания

- Использовал изначально Postgres для проверки работы паттерна "репозиторий".
- Хранилище работает в памяти: после перезапуска сервиса все данные очищаются. Это требование задания, постоянная БД не предусмотрена.
- Сканер реализован как заглушка, генерирующая случайный набор уязвимостей с задержкой. Подключение реального анализатора выполняется за интерфейсом ICodeScanner без изменения остального кода.
